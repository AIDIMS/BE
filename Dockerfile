# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["AIDIMS.API/AIDIMS.API.csproj", "AIDIMS.API/"]
COPY ["AIDIMS.Application/AIDIMS.Application.csproj", "AIDIMS.Application/"]
COPY ["AIDIMS.Domain/AIDIMS.Domain.csproj", "AIDIMS.Domain/"]
COPY ["AIDIMS.Infrastructure/AIDIMS.Infrastructure.csproj", "AIDIMS.Infrastructure/"]
RUN dotnet restore "AIDIMS.API/AIDIMS.API.csproj"

# Copy all source files
COPY . .
WORKDIR "/src/AIDIMS.API"

# Build the application
RUN dotnet build "AIDIMS.API.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "AIDIMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIDIMS.API.dll"]
