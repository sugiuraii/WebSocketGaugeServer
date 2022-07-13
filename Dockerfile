FROM mcr.microsoft.com/dotnet/sdk AS build
RUN apt-get update
RUN apt-get install -y curl
RUN apt-get install -y libpng-dev libjpeg-dev curl libxi6 build-essential libgl1-mesa-glx
RUN curl -sL https://deb.nodesource.com/setup_lts.x | bash -
RUN apt-get install -y nodejs
WORKDIR /source
COPY WebSocketGaugeServer/ ./WebSocketGaugeServer
WORKDIR /source/WebSocketGaugeServer
RUN dotnet restore
WORKDIR /source/WebSocketGaugeServer/WebSocketServer
RUN dotnet publish -o /app --no-restore
#FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim-arm32v7
#FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim-arm64v8
FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim-amd64
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "WebSocketServer.dll"]