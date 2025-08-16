# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-stage
WORKDIR /src

# Copy  everything else and build
COPY shopFlow.csproj /src/
COPY . /src/
RUN dotnet restore shopFlow.csproj

RUN dotnet publish shopFlow.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final-stage
COPY --from=build-stage /app /app
COPY shopFlow.csproj /app/
COPY ./Properties /app/Properties

# Runtime image
WORKDIR /app

RUN echo '\n\ 
    dotnet shopFlow.dll' > /app/start.sh
RUN chmod a+x /app/start.sh
CMD "./start.sh"
