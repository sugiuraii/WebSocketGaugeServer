# WebSocketGaugeServer - How to build

# Requirement
## OS
* Windows(x64) or linux(x64 or arm).

## SDK
* To build, please install dotnet sdk and nodejs
    * [dotnet sdk (6.0)](https://dotnet.microsoft.com/)
    * [nodejs (16)](https://nodejs.org/ja/)
        * nodejs is required to compile javascript (used for web UI)

# Run from source
Clone source repository and run following command.

```
cd WebSocketGaugeServer/WebSocketServer
dotnet run
```

# Build with bundling runtime
Replace `win-x64` with target os platform (`linux-x64`, `linux-arm`, `linux-arm64`)
```
cd WebSocketGaugeServer/WebSocketServers
dotnet publish -r win-x64 --self-contained
```
You can find compiled binary and runtimes on bin/Debug.

See [https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)

# Build Docker image
See [Build-Docker.md](Build-Docker.md)






