# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY src/Stars.Interface/*.csproj ./src/Stars.Interface/
COPY src/Stars.Geometry/*.csproj ./src/Stars.Geometry/
COPY src/Stars.Services/*.csproj ./src/Stars.Services/
COPY src/Stars.Data/*.csproj ./src/Stars.Data/
COPY src/Stars.Console/*.csproj ./src/Stars.Console/
RUN dotnet restore src/Stars.Console/

# copy everything else and build app
COPY . .
RUN ls -l src/Stars.Services/
RUN dotnet publish -c release -o /app -r linux-x64 -f net5.0 --self-contained true --no-restore src/Stars.Console/

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-bullseye-slim-amd64
WORKDIR /app
COPY --from=build /app ./
COPY src/scripts/stars /bin/Stars
COPY src/scripts/stars /bin/stars
