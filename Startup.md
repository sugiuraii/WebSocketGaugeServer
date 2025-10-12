# WebSocketGaugeServer - Program startup

## Run from binary executable
1. Download binary archive and extract it.
2. Edits setting file of `appsettings.json`.
    * See [Configure.md](Configure.md).
3. Run executable file of `WebSocketServer` (or `WebSocketServer.exe` in Windows).
4. To stop the server, type Ctrl+C.

## Run automatically on linux boot (by systemd)
`systemd` of linux can run `WebSocketServer` program at OS startup like daemon.

1. Copy all of executable asset files to `/opt/websocketgaugeserver`
2. Add follwing file of `websocketgaugeserver.service` to `/etc/systemd/system/`
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

* To disable autorun and uninstall..
1. Run `sudo systemctl disable websocketgaugeserver.service`
2. Remove `/etc/systemd/system/websocketgaugeserver.service`
3. Delete `/opt/websocketgaugeserver`

## Run by making Docker container
1. Move the directory of `Docker.image.install` (of the source code)
2. Edits setting file of `appsettings.json`.
    * See [Configure.md](Configure.md). 
3. Get docker image from dockerhub, and write appsettings.json to the image.
    * See [Setup-Docker.md](Setup-Docker.md) for detail.
4. Run docker image by creating container.
    * To use only "virtual ecu mode" (not communicate with ECU or sensor),
        ```
        docker run -p 2016:2016 --name wsgaugeserver local/wsgaugeserver
        ```
        * Change port number of `-p` option, if you change port number by `appsettings.json`
        * Replace `local/wsgaugeserver` with the image tag name you created in 3.
        * Set your favorite container name by `--name` option.
    * To communicate with ECU, Seosor, scantool, you have to export the device to Docker container by adding `--device` option.
        ```
        docker run -p 2016:2016 --name wsgaugeserver --device=/dev/ttyUSB0 local/wsgaugeserver
        ```
        * See [this document](https://docs.docker.com/engine/reference/commandline/run/#add-host-device-to-container---device)
    * To run the container automatically on system bootup, set  restart policy by `--restart` option
        ```
        docker run -p 2016:2016 --name wsgaugeserver --device=/dev/ttyUSB0 --restart always local/wsgaugeserver
        ```
        * See [this document](https://docs.docker.com/config/containers/start-containers-automatically/)
