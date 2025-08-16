# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-stage
WORKDIR /src

# Copy  everything else and build
COPY ["shopFlow.Main/shopFlow.csproj", "shopFlow.Main/"]
COPY ["shopFlow.Importer/shopFlow.Importer.csproj", "shopFlow.Importer/"]

RUN dotnet restore "shopFlow.Main/shopFlow.csproj"

# Copy everything else and build
COPY . .

RUN dotnet publish "shopFlow.Main/shopFlow.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:GenerateRuntimeConfigurationFiles=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Runtime image
WORKDIR /app

# Copy published content
COPY --from=build-stage /app/publish .

ENTRYPOINT ["dotnet", "shopFlow.dll"]