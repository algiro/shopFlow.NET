# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-stage

# Install comprehensive language and font support
RUN apt-get update && apt-get install -y \
    locales \
    locales-all \
    fontconfig \
    fonts-dejavu-core \
    fonts-dejavu-extra \
    fonts-liberation \
    fonts-noto \
    fonts-noto-color-emoji \
    fonts-freefont-ttf \
    fonts-opensymbol \
    && rm -rf /var/lib/apt/lists/*

# Set multiple locale support
ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8
ENV LANGUAGE=en_US:en
# Update font cache
RUN fc-cache -fv

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