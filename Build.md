# WebSocketGaugeServer - How to build

# Requirement
## OS
* Windows(x64) or linux(x64 or arm).

## SDK
* To build, please install dotnet sdk and nodejs
    * [dotnet sdk (8.0)](https://dotnet.microsoft.com/)
    * [nodejs (22)](https://nodejs.org/)
        * nodejs is required to compile javascript (used for web UI)

# Run from source
Clone source repository and run following command.

```
cd WebSocketGaugeServer/WebSocketServer
dotnet run
```

# Build with bundling runtime
```
cd WebSocketGaugeServer/WebSocketServer
dotnet publish
```
You can find compiled binary and runtimes on bin/Release.

See [https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)

# Build Docker image
See [Build-Docker.md](Build-Docker.md)






