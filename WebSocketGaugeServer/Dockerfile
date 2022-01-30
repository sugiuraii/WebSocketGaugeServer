FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS build
RUN apt-get update
RUN apt-get install -y curl
RUN apt-get install -y libpng-dev libjpeg-dev curl libxi6 build-essential libgl1-mesa-glx
RUN curl -sL https://deb.nodesource.com/setup_lts.x | bash -
RUN apt-get install -y nodejs
WORKDIR /app
COPY ./bin/Debug/net5.0 ./
ENTRYPOINT ["dotnet", "WebSocketServer.dll"]