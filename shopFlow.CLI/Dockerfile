# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-stage
WORKDIR /src

# Copy  everything else and build
COPY shopFlow.CLI/shopFlow.CLI.csproj /shopFlow.CLI/
COPY shopFlow.Main/shopFlow.csproj /shopFlow.Main/
RUN dotnet restore "/shopFlow.CLI/shopFlow.CLI.csproj"
COPY . .
WORKDIR "/src/shopFlow.CLI"
RUN dotnet build shopFlow.CLI.csproj -c Release -o /app/build

FROM build-stage as publish
RUN dotnet publish shopFlow.CLI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "shopFlow.CLI.dll"]
