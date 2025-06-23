# 1) Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

# Copy everything and restore/publish directly
COPY . .
RUN dotnet restore "Survey-App.csproj"
RUN dotnet publish "Survey-App.csproj" \
    --configuration Release \
    --output /app/out

# 2) Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Pull in published output
COPY --from=build-env /app/out ./

ENTRYPOINT ["dotnet", "Survey-App.dll"]

