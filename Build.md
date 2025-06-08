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
You can find compiled binary and runtimes on `WebSocketGaugeServer/WebSocketServer/bin/Release/net8.0/publish`.

See [https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)

# Run from executable (in runtime bundled executable files)
Run `WebSocketServer` or `WebSoccketServer.exe` in `publish` directory.

# Run automatically on linux boot (by systemd)
`systemd` of linux can run `WebSocketServer` program at OS startup like daemon.

1. Copy all of executable asset files to `/opt/websocketgaugeserver`
2. Add follwing `websocketgaugeserver.service` to `/etc/systemd/system/`
```
[Unit]
Description=WebsocketGaugeServer

[Service]
Type=simple
ExecStart=/opt/websocketgaugeserver/WebSocketServer
WorkingDirectory=/opt/websocketgaugeserver
KillMode=process
Restart=always

[Install]
WantedBy=multi-user.target
```
3. Set `websocketgaugeserver.service` to enable, to run automatically.
```
sudo systemctl enable websocketgaugeserver.service
```

4. After system reboot, you can check the status of `websocketgaugeserver.service` by
```
journalctl -xeu websocketgaugeserver.service
```

* To disable autorun and uninstall,
1. Run `sudo systemctl disable websocketgaugeserver.service`
2. Remove `/etc/systemd/system/websocketgaugeserver.service`
3. Delete `/opt/websocketgaugeserver`

# Build Docker image
See [Build-Docker.md](Build-Docker.md)






