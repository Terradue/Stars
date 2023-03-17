# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
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
RUN dotnet publish -c release -o /app -r linux-x64 -f net6.0 --self-contained true --no-restore src/Stars.Console/

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-bullseye-slim-amd64

RUN apt-get update \
  && apt-get upgrade -y \
  && apt-get install -y hdf5-tools libssl1.1 libgssapi-krb5-2 ca-certificates \
  && rm -rf /var/lib/apt/lists/* /tmp/*
  
WORKDIR /app
COPY --from=build /app ./
COPY src/scripts/stars /bin/Stars


