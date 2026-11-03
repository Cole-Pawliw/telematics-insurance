# Build context is the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

# Restore first so package downloads are cached until a project file changes.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Contracts/TelematicsInsurance.Contracts.csproj src/Contracts/
COPY src/Services/Billing/Billing.Domain/Billing.Domain.csproj src/Services/Billing/Billing.Domain/
COPY src/Services/Billing/Billing.Application/Billing.Application.csproj src/Services/Billing/Billing.Application/
COPY src/Services/Billing/Billing.Infrastructure/Billing.Infrastructure.csproj src/Services/Billing/Billing.Infrastructure/
COPY src/Services/Billing/Billing.Api/Billing.Api.csproj src/Services/Billing/Billing.Api/
RUN dotnet restore src/Services/Billing/Billing.Api/Billing.Api.csproj

COPY src/Contracts/ src/Contracts/
COPY src/Services/Billing/ src/Services/Billing/
RUN dotnet publish src/Services/Billing/Billing.Api/Billing.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Billing.Api.dll"]
