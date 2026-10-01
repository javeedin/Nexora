# syntax=docker/dockerfile:1
# Build context: repository root (shared Directory.*.props / global.json live there).
#   docker build -f services/api/Dockerfile -t nexora-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble@sha256:35d40304542c8689331f8cab17c65926cdf48fe711e289321d71924b230a7d29 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY services/api/src/Nexora.Api/Nexora.Api.csproj services/api/src/Nexora.Api/packages.lock.json services/api/src/Nexora.Api/
RUN dotnet restore services/api/src/Nexora.Api/Nexora.Api.csproj --locked-mode
COPY services/api/src/ services/api/src/
RUN dotnet publish services/api/src/Nexora.Api/Nexora.Api.csproj -c Release --no-restore -o /app -p:UseAppHost=false

# Chiseled: no shell, no package manager, runs as non-root `app` (UID 1654) by default.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled@sha256:9651fa59abcdf177c30392cb44a820605ca5d618429ab37acbf6e7c644510b02
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexora.Api.dll"]
