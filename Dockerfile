# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY src/*.sln .
COPY src/Stars.Interface/*.csproj ./src/Stars.Interface/
COPY src/Stars.Geometry/*.csproj ./src/Stars.Geometry/
COPY src/Stars.Services/*.csproj ./src/Stars.Services/
COPY src/Stars.Data/*.csproj ./src/Stars.Data/
COPY src/Stars.Console/*.csproj ./src/Stars.Console/
RUN dotnet restore -r linux-x64 /p:PublishReadyToRun=true src/Stars.Console/

# copy everything else and build app
COPY src/Stars.Interface/* ./src/Stars.Interface/
COPY src/Stars.Geometry/* ./src/Stars.Geometry/
COPY src/Stars.Services/* ./src/Stars.Services/
COPY src/Stars.Data/* ./src/Stars.Data/
COPY src/Stars.Console/* ./src/Stars.Console/
RUN dotnet publish -c release -o /app -r linux-x64 -f net5.0 --self-contained true --no-restore src/Stars.Console/

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-bullseye-slim-amd64
WORKDIR /app
COPY --from=build /app ./
COPY src/scripts/stars /bin/Stars
COPY src/scripts/stars /bin/stars
