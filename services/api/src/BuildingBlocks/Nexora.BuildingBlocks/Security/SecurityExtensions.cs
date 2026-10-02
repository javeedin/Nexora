using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexora.BuildingBlocks.Tenancy;

namespace Nexora.BuildingBlocks.Security;

/// <summary>OIDC bearer authentication, Nexora policies and the request context (ADR 0010).</summary>
public static class SecurityExtensions
{
    /// <summary>
    /// JWT bearer validation against <c>Auth:Authority</c> (issuer, audience, signature, lifetime), tenant and role
    /// claims from the token, policies, and an authenticated-by-default fallback policy.
    /// </summary>
    public static IServiceCollection AddNexoraSecurity(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddOptions<AuthOptions>().BindConfiguration("Auth");
        services.AddScoped<ClaimsCurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<ClaimsCurrentUser>());
        services.Replace(ServiceDescriptor.Scoped<ITenantContext>(sp => sp.GetRequiredService<ClaimsCurrentUser>()));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AuthOptions>, IHostEnvironment>((jwt, auth, env) =>
            {
                var options = auth.Value;
                jwt.Authority = options.Authority;
                jwt.Audience = options.Audience;
                jwt.RequireHttpsMetadata = options.RequireHttpsMetadata || !env.IsDevelopment();
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters.NameClaimType = "preferred_username";
                jwt.TokenValidationParameters.RoleClaimType = NexoraClaims.Role;
                jwt.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
                        {
                            TokenClaims.Enrich(identity, context.Request.Headers[NexoraClaims.TenantHeader].FirstOrDefault());
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddSingleton<IAuthorizationHandler, MfaHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, StepUpResultHandler>();
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(NexoraPolicies.TenantMember, p => p.RequireAuthenticatedUser().RequireClaim(NexoraClaims.TenantId))
            .AddPolicy(NexoraPolicies.TenantAdmin, p => p.RequireAuthenticatedUser().RequireClaim(NexoraClaims.TenantId)
                .RequireRole(NexoraRoles.TenantAdmin).AddRequirements(new MfaRequirement()))
            .AddPolicy(NexoraPolicies.PlatformAdmin, p => p.RequireAuthenticatedUser()
                .RequireRole(NexoraRoles.PlatformAdmin).AddRequirements(new MfaRequirement()));
        return services;
    }

    /// <summary>
    /// Authentication, then tenant / user on the trace and log scope (NFR observability: tenant_id everywhere).
    /// Call before rate limiting so limits partition by tenant.
    /// </summary>
    public static IApplicationBuilder UseNexoraSecurity(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.Use(async (context, next) =>
        {
            var user = context.RequestServices.GetRequiredService<ICurrentUser>();
            var activity = Activity.Current;
            if (user.IsAuthenticated)
            {
                activity?.SetTag("enduser.id", user.UserId);
                activity?.SetTag("tenant.id", user.Tenant?.Id);
            }

            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Nexora.Request");
            using (logger.BeginScope(new Dictionary<string, object?> { ["tenant_id"] = user.Tenant?.Id, ["user_id"] = user.UserId }))
            {
                await next(context);
            }
        });
        return app;
    }
}

/// <summary>Requires a second factor (<c>acr = mfa</c>) when <see cref="AuthOptions.RequireMfaForAdmins"/> is on.</summary>
public sealed class MfaRequirement : IAuthorizationRequirement;

internal sealed class MfaHandler(IOptionsMonitor<AuthOptions> options) : AuthorizationHandler<MfaRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MfaRequirement requirement)
    {
        if (!options.CurrentValue.RequireMfaForAdmins || context.User.FindFirst("acr")?.Value == "mfa")
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// 401 / 403 as problem details. A 403 caused only by missing MFA carries the RFC 9470 step-up challenge
/// (<c>insufficient_user_authentication</c>, <c>acr_values="mfa"</c>) so the client re-authenticates with OTP.
/// </summary>
internal sealed class StepUpResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        var failed = authorizeResult.AuthorizationFailure?.FailedRequirements.ToList() ?? [];
        var problems = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        if (authorizeResult.Challenged)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer";
            await problems.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = { Status = 401, Title = "Sign-in required", Detail = "Send a valid bearer token." },
            });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        var mfaOnly = failed.Count > 0 && failed.All(r => r is MfaRequirement);
        if (mfaOnly)
        {
            context.Response.Headers.WWWAuthenticate =
                "Bearer error=\"insufficient_user_authentication\", error_description=\"A second factor is required\", acr_values=\"mfa\"";
        }

        var rejected = context.User.FindFirst(NexoraClaims.TenantRejected)?.Value;
        await problems.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails =
            {
                Status = 403,
                Title = mfaOnly ? "Second factor required" : "Forbidden",
                Detail = mfaOnly ? "Sign in again with your authenticator app (step-up to acr=mfa)."
                    : rejected is not null ? $"You are not a member of tenant '{rejected}'."
                    : context.User.FindFirst(NexoraClaims.TenantId) is null && context.User.FindAll(NexoraClaims.Membership).Skip(1).Any()
                        ? $"You belong to several tenants: choose one with the {NexoraClaims.TenantHeader} header."
                        : "You do not have permission for this action.",
            },
        });
    }
}
