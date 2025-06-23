# 1) Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

# Copy everything and restore/publish directly
COPY . .
RUN dotnet restore "Reporting-App.csproj"
RUN dotnet publish "Reporting-App.csproj" \
    --configuration Release \
    --output /app/out

# 2) Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build-env /app/out ./

ENTRYPOINT ["dotnet", "Reporting-App.dll"]
