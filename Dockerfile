# Select architecture of build machine
# Change platform option for your build architecture
FROM --platform=linux/amd64 cr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
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

# Copy build files to aspnet docker image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "WebSocketServer.dll"]