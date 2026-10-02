using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexora.BuildingBlocks.Http;
using Nexora.BuildingBlocks.Idempotency;
using Nexora.BuildingBlocks.Modules;
using Nexora.BuildingBlocks.Security;
using Nexora.Modules.Platform.Contracts;
using Nexora.Modules.Platform.Identity;

namespace Nexora.Modules.Platform;

/// <summary>The platform module (M0): identity now; tenancy, RBAC, settings and audit from P0-T07.</summary>
public sealed partial class PlatformModule : IModule
{
    /// <inheritdoc />
    public string Name => "platform";

    /// <inheritdoc />
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KeycloakOptions>().BindConfiguration("Keycloak");
        services.AddSingleton<KeycloakTokenCache>();
        services.TryAddSingletonTimeProvider();
        services.AddHttpClient<IIdentityAdmin, KeycloakIdentityAdmin>((sp, http) =>
            {
                var options = sp.GetRequiredService<IOptions<KeycloakOptions>>().Value;
                http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            })
            .AddStandardResilienceHandler(o => o.Retry.DisableForUnsafeHttpMethods()); // rule 6; never re-send an invite
    }

    /// <inheritdoc />
    public void MapEndpoints(IEndpointRouteBuilder group)
    {
        group.MapGet("/info", GetInfo)
            .AllowAnonymous()
            .WithName("GetPlatformInfo")
            .WithSummary("Service name, build version and environment.");

        group.MapGet("/me", GetMe)
            .WithName("GetMe")
            .WithSummary("The signed-in user: tenant(s), roles, MFA state.");

        group.MapPost("/invitations", InviteAsync)
            .RequireAuthorization(NexoraPolicies.TenantAdmin)
            .WithValidation<InviteUserRequest>()
            .WithIdempotency()
            .WithName("InviteUser")
            .WithSummary("Invite someone by e-mail to the current tenant (tenant admin, MFA).")
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static Ok<PlatformInfo> GetInfo(IHostEnvironment environment)
    {
        var version = typeof(PlatformModule).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
        return TypedResults.Ok(new PlatformInfo("nexora-api", version, environment.EnvironmentName));
    }

    private static Ok<MeResponse> GetMe(ICurrentUser user) =>
        TypedResults.Ok(new MeResponse(
            user.UserId!, user.Username, user.Email, user.Name,
            user.Tenant is { } t ? new TenantSummary(t.Id, t.Alias) : null,
            [.. user.Tenants.Select(t => new TenantSummary(t.Id, t.Alias))],
            user.Roles, user.HasMfa));

    private static async Task<Results<Accepted<InvitationResponse>, ProblemHttpResult>> InviteAsync(
        InviteUserRequest request, ICurrentUser user, IIdentityAdmin identity, ILogger<PlatformModule> logger, CancellationToken cancellationToken)
    {
        var tenant = user.Tenant!;
        try
        {
            await identity.InviteToTenantAsync(tenant.Id, request.Email.Trim(), request.FirstName, request.LastName, cancellationToken);
        }
        catch (IdentityConflictException ex)
        {
            return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Already a member");
        }

        // Audit trail lands with P0-T12; until then a structured log entry (tenant + user on the scope).
        LogInvited(logger, tenant.Id, user.UserId);
        return TypedResults.Accepted((string?)null, new InvitationResponse(request.Email.Trim(), tenant.Id, "sent"));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Invitation sent for tenant {TenantId} by {UserId}")]
    private static partial void LogInvited(ILogger logger, string tenantId, string? userId);
}

/// <summary>Validates <see cref="InviteUserRequest"/>.</summary>
internal sealed class InviteUserRequestValidator : AbstractValidator<InviteUserRequest>
{
    public InviteUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
    }
}

internal static class TimeProviderRegistration
{
    public static void TryAddSingletonTimeProvider(this IServiceCollection services)
    {
        if (services.All(d => d.ServiceType != typeof(TimeProvider)))
        {
            services.AddSingleton(TimeProvider.System);
        }
    }
}
