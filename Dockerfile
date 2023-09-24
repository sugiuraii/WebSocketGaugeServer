# Select architecture of build machine
# Change platform option for your build platform
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
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
# Add --platform option to specify target platform
FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim
WORKDIR /app
RUN apt-get update
RUN apt-get install -y vim
RUN apt-get install -y nano
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "WebSocketServer.dll"]