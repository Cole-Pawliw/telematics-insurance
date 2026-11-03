# Build context is the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

# Restore first so package downloads are cached until a project file changes.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Contracts/TelematicsInsurance.Contracts.csproj src/Contracts/
COPY src/Services/Policy/Policy.Domain/Policy.Domain.csproj src/Services/Policy/Policy.Domain/
COPY src/Services/Policy/Policy.Application/Policy.Application.csproj src/Services/Policy/Policy.Application/
COPY src/Services/Policy/Policy.Infrastructure/Policy.Infrastructure.csproj src/Services/Policy/Policy.Infrastructure/
COPY src/Services/Policy/Policy.Api/Policy.Api.csproj src/Services/Policy/Policy.Api/
RUN dotnet restore src/Services/Policy/Policy.Api/Policy.Api.csproj

COPY src/Contracts/ src/Contracts/
COPY src/Services/Policy/ src/Services/Policy/
RUN dotnet publish src/Services/Policy/Policy.Api/Policy.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Policy.Api.dll"]
