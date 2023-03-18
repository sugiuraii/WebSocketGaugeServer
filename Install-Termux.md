# Install server on Android + Termux
* The server program is confirmed to work on Android, with using Termux.
* This document describe the instruction to install this server program to Termux.

## Requirement
* Android smartphone/tablet/PC.
    * arm64 is tested. (binaries of arm32 and x64 is available, but not tested.)
* Permission to install termux out of Google play store.
    * Recent version of termux is unavailable in Google play store and have to be installed by apk.
    * Termux at Google play store is currrently abondoned, and cannot be used.
* UART-LAN(Wifi, etc..) gateway, ELM327 wifi adaptor, or (USB/bluetooth) serial to TCP wrapper android app.
    * Since Termux is not allowed to communicate with bluetooth or USB-serial adaptor, tcp tunnel mode (of this server program) is needed.
        * [This server] <-(TCP connection with tcp tunnel(wrapper) mode) -> [TCP->Serial wrapper android app] <- (BT or USB serial) -> [ELM327 BT/USB adaptor]
        * [This server] <-(TCP connection with tcp tunnel(wrapper) mode) <- (Wifi) -> [ELM327 wifi adaptor]

## Install termux
* Download termux apk from [github release page](https://github.com/termux/termux-app/releases) or [the page of F-driod](https://f-droid.org/packages/com.termux/).
* Install the downloaded apk file. (You need to permit to install apk from unknown source.)
* Launch termux and update packages by runinng `pkg update` and `pkg upgrade`.

## Install debian (or ubuntu) assets to termux by proot-distro
* Install `proot-distro` package on termux.
    ```
    pkg install proot-distro
    ```
* Install `debian` distribution with `debian-wsgauge` alias.
    ```
    proot-distro install --override-alias debian-wsgauge debian
    ```
    * You can install `ubuntu` instead of `debian`
* Login to installed `debian` environment
    ```
    proot-distro login debian-wsgauge
    ```
* Update `debian` packages
    ```
    apt update
    apt upgrade
    ```
* Install `libicu` to `debian` environment. (libicu is needed to run dotnet runtime)
    ```
    apt install libicu-dev
    ```

## Install WebSocketGaugeServer binary
* After login to `debian` environment, download linux binary from [WebSocketGaugeServer release](https://github.com/sugiuraii/WebSocketGaugeServer/releases) page
    ```
    curl -OL https://github.com/sugiuraii/WebSocketGaugeServer/releases/download/4.0%2FRC1/WebsocketGaugeServer-4.0-RC1-linux-arm64.tar.xz
    ```
    * Change URL for version or target architecture.
* Excract tar archive
    ```
    xzcat WebsocketGaugeServer-4.0-RC1-linux-arm64.tar.xz | tar xvf -
    ```
* Move to directory
    ```
    cd WebsocketGaugeServer-4.0-RC1-linux-arm64
    ```
* (Optional) edit configuration file.
    * To change the setting, edit `appsettings.json`.
    * Before edit, install editor like `vim`
    ```
    apt install vim
    vi appsettings.json
    ```
* Finally, launch `WebSocketServer`
    ```
    ./WebSocketServer
    ```
* It may be better to get `ACQUIRE LOCK` in termux notification popup, in order to avoid forcebly stop by android system.
* After the boot, you can access to the frontpage by http://localhost:2016
## Install serial-TCP wrapper
* To tunnel serial communication to TCP, serial-TCP wrapper may be needed. 
* [Bluetooth Bridge (+TCP)](https://play.google.com/store/apps/details?id=masar.bb) is tested for bluetooth elm327 adaptor.
    * Default port number is 35000.
