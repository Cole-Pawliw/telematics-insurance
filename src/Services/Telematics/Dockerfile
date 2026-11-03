# Build context is the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

# Restore first so package downloads are cached until a project file changes.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Contracts/TelematicsInsurance.Contracts.csproj src/Contracts/
COPY src/Services/Telematics/Telematics.Domain/Telematics.Domain.csproj src/Services/Telematics/Telematics.Domain/
COPY src/Services/Telematics/Telematics.Application/Telematics.Application.csproj src/Services/Telematics/Telematics.Application/
COPY src/Services/Telematics/Telematics.Infrastructure/Telematics.Infrastructure.csproj src/Services/Telematics/Telematics.Infrastructure/
COPY src/Services/Telematics/Telematics.Api/Telematics.Api.csproj src/Services/Telematics/Telematics.Api/
RUN dotnet restore src/Services/Telematics/Telematics.Api/Telematics.Api.csproj

COPY src/Contracts/ src/Contracts/
COPY src/Services/Telematics/ src/Services/Telematics/
RUN dotnet publish src/Services/Telematics/Telematics.Api/Telematics.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Telematics.Api.dll"]
