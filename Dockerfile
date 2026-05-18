# =========================
# Build Stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY "src/MechanicShop.Api/MechanicShop.Api.csproj" "src/MechanicShop.Api/"
COPY "src/MechanicShop.Infrastructure/MechanicShop.Infrastructure.csproj" "src/MechanicShop.Infrastructure/"
COPY "src/MechanicShop.Application/MechanicShop.Application.csproj" "src/MechanicShop.Application/"
COPY "src/MechanicShop.Domain/MechanicShop.Domain.csproj" "src/MechanicShop.Domain/"
COPY "src/MechanicShop.Contracts/MechanicShop.Contracts.csproj" "src/MechanicShop.Contracts/"
COPY "src/MechanicShop.slnx" .

RUN dotnet restore "src/MechanicShop.Api/MechanicShop.Api.csproj"

COPY . .

RUN dotnet publish "src/MechanicShop.Api/MechanicShop.Api.csproj" \
    -c Release \
    -o /app/publish

# =========================
# Runtime Stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 as final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "MechanicShop.Api.dll"]