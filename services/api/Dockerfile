# syntax=docker/dockerfile:1
# Build context: repository root (shared Directory.*.props / global.json live there).
#   docker build -f services/api/Dockerfile -t nexora-api .

FROM mcr.microsoft.com/dotnet/sdk:9.0-noble@sha256:0f97a4002de8050867e7e55e03a0487aea4f4a933158fe1d2cc35f570fc81a0d AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY services/api/src/Nexora.Api/Nexora.Api.csproj services/api/src/Nexora.Api/packages.lock.json services/api/src/Nexora.Api/
RUN dotnet restore services/api/src/Nexora.Api/Nexora.Api.csproj --locked-mode
COPY services/api/src/ services/api/src/
RUN dotnet publish services/api/src/Nexora.Api/Nexora.Api.csproj -c Release --no-restore -o /app -p:UseAppHost=false

# Chiseled: no shell, no package manager, runs as non-root `app` (UID 1654) by default.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled@sha256:08624fa82cfd7bd278a5e502ec384be6eea09b7ef860f11bfd9dba4ce5244f13
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexora.Api.dll"]
