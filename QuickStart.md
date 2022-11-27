# WebSocketGaugeServer - Quick Start
* This document is a quick instruction to start the WebSocketGaugeServer program by OBDII+ELM327 communication.
* Procedures for using other ECU communication (Arduino,Defi,SSM) are currently under construction. To know how to use them, please refer to the source code or ask questions by e-mail.
    * Please note that this program has not been tested on a real car so foten, and the development tests were done mainly on the ECU simulator.

## Download the binary file.
* Download the binary archive file from [Release page on GitHub](https://github.com/sugiuraii/WebSocketGaugeServer/releases/) and extract it to an appropriate directory.

## Edit configuration file
* Edit `appsettings.json` in the extracted directory. Edit the following part according to your purpose.
```js
"ServiceConfig": {
    "ELM327": {
      "enabled": true,
      "urlpath" : "/elm327",
      "usevirtual" : false,
      "comport": "/dev/ttyUSB0",
      "baudrate": 115200,
　    "elm327ProtocolMode": "0",
      "elm327AdaptiveTimingControl": 1,
      "elm327Timeout" : 50
```
In the above part.
* `"enabled"`:Enable ELM327+OBDII communication mode.
* `"urlpath"`:No need to change（Path to start WebSocket communication)
* `"usevirtual"`: Set the virtual ECU setting. set to `false` when actually connecting the ELM327.
    * By setting this to `true`, you can test the operaion by using "virtual" ECU in the program without connecting to ELM327. You can test and debug the gauge without connecting ELM327 by using this feature.
* `"comport"`: Set the name of the serial port to connect the ELM327. For linux, set like `/dev/tty*`. For windows set like `COM*`.
* `"baudrate"`: Set the serial port speed (baud rate) to communicate with the ELM327.
* `"elm327ProtocolMode"` : Set the ELM327-ECU communication protocol. The default is automatic (0). If the communication not works well, set it manually.
* `"elm327AdaptiveTimingControl"`: Sets the wait timing for the ELM327-ECU communication. This can configure the waiting time of ECU->ELM327 data transfer. The standard setting is "adaptive timing control" (1), If there is a large delay, "aggressive adaptime timing control" (2) can be used, or adaptive timing control can be disabled by setting it to 0, and the wait time can be set manually with `"elm327Timeout"`.

Translated with www.DeepL.com/Translator (free version)
## Start the program
* Run the `WebSocketServer.exe`(Windows) or `WebSocketServer`(Linux) executable file in the extracted directory from Explorer or Terminal.

## Access the configuration page.
* Access port 2016 of the launched PC with a Web browser.
    * To connect to local server, set URL to `http://localhost:2016/`.
    * If accessing from another PC, smartphone, etc., use `http://(address of the PC that started the WebSocketServer):2016/`.
        * This program does not currently implement any security-related features, so please be careful to configure your Wifi security and firewall settings to avoid access from unintended devices.
* In the left menu, if the virtual ECU is enabled in the configuration file, you can go to the virtual ECU sensor value setting page from the VirtualELM327Control tab. After moving to the page, you can set the sensor value with the slider.

## Start the sample meter (client)
* After accessing the above setting page on the PC or smartphone on which you want to display the meter, click on the Gauge client menu.
* Click on the sample meter you wish to display to enter the meter selection menu.
