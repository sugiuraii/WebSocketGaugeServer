# WebSocketGaugeServer - Program startup

## Run from binary executable
1. Download binary archive and extract it.
2. Edits setting file of `appsettings.json`.
    * See [Configure.md](Configure.md).
3. Run executable file of `WebSocketServer` (or `WebSocketServer.exe` in Windows).
4. To stop the server, type Ctrl+C.

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
