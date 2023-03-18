# WebSocketGaugeServer - Quick Start
* This document is a quick instruction to start the WebSocketGaugeServer program by OBDII+ELM327 communication.
* Procedures for using other ECU communication (Arduino,Defi,SSM) are currently under construction. To know how to use them, please refer to the source code or ask questions by e-mail.
    * Please note that this program has not been tested on a real car so foten, and the development tests were done mainly on the ECU simulator.

## Download the binary file.
* Download the binary archive file from [Release page on GitHub](https://github.com/sugiuraii/WebSocketGaugeServer/releases/) and extract it to an appropriate directory.

## Edit configuration file
* Edit `appsettings.json` in the extracted directory. Edit the following part according to your purpose.
```jsonc
"ServiceConfig": {
    "ELM327": {
      "enabled": true,
      "urlpath" : "/elm327",
      "virtualecu" : {"enabled": false, "waitmsec" : 15},
      "comport": "/dev/ttyUSB0",
      "baudrate": 115200,
　    "elm327ProtocolMode": "0",
      "elm327AdaptiveTimingControl": 1,
      "elm327Timeout" : 50,
      "elm327HeaderBytes" : "",
      "elm327BatchPIDQueryCount" : 1,
      "elm327PIDBatchQueryAvoidMultiFrameResponse" : false
```
In the above part.
* `"enabled"`:Enable ELM327+OBDII communication mode.
* `"urlpath"`:No need to change（Path to start WebSocket communication)
* `"virtualecu"`: Setup virtual ECU mode. 
    * When `"enabled"` is set to `true`, the server program disables serial port connection (to ELM327, etc) and enable internal "virtual" ECU.
        * By setting this to `true`, you can test the operaion by using "virtual" ECU in the program without connecting to ELM327. You can test and debug the gauge without connecting ELM327 by using this feature.
        * Thus, set to `false` to connect physical ELM327 (etc) devices.
    * `"waitmsec"` set the wait time of virtual ECU (to simulate slow ECU communication).
    
* `"comport"`: Set the name of the serial port to connect the ELM327. For linux, set like `/dev/tty*`. For windows set like `COM*`.
    * From 3.5/Beta2, serial port connection can be tunneled through TCP connection. This may be useful to use UART-Wifi adapter, ELM327-Wifi adaptor, or Serial-TCP tunneling program.
    * To use TCP wrapper, set this field to,
        ```jsonc
        "comport": "tcp:hostname.of.remote.com:xxxxx",
        ``` 
        or
        ```jsonc
        "comport": "tcp:192.168.xx.xx:xxxxx",
        ```
* `"baudrate"`: Set the serial port speed (baud rate) to communicate with the ELM327.
* `"elm327ProtocolMode"` : Set the ELM327-ECU communication protocol. The default is automatic (0). If the communication not works well, set it manually.
* `"elm327AdaptiveTimingControl"`: Sets the wait timing for the ELM327-ECU communication. This can configure the waiting time of ECU->ELM327 data transfer. The standard setting is "adaptive timing control" (1), If there is a large delay, "aggressive adaptime timing control" (2) can be used, or adaptive timing control can be disabled by setting it to 0, and the wait time can be set manually with `"elm327Timeout"`.
* `"elm327HeaderBytes"` : Set header of ELM327.
    * See the command instruction of "AT SH" in ELM327 data sheet.
    * This field (and AT SH command) will be ignored when the value is blank string ("").
    * (For example, you can manually set the CAN ID of ECU. By doing this, you can eliminate unnecessay CAN traffic when multiple ECUs (ECU+TCU, etc..) exists.)
       * (For the detail, see "Setting the Headers " in ELM327 data sheet.) 
* `"elm327BatchPIDQueryCount"`
* `"elm327PIDBatchQueryAvoidMultiFrameResponse"`
    * Number of PID batch query. (Default is 1 Max is 6.)
    * The SAE J1979 (ISO 15031-5) standard allows requesting multiple PIDs (max 6) with one message, but only if you connect to the vehicle with CAN (ISO 15765-4).
    * If this feature is supported by ECU and ELM327 adaptor, this can drastically improve PID rate.
        * On batched PID request, ECU response message may exceed CAN payload length (of 8bytes include mode and size bytes.)
        * In this case, ECU makes response message by multiple (splitted) CAN data frames using ISO-TP (ISO 15765-2)
        *  However, some of OBDII dongle cannot handle ISO TP (my no brand chinese ELM327V2.1 dongle (maybe unofficial clone) cannot handle ISO-TP).
        * In this case, set `"elm327PIDBatchQueryAvoidMultiFrameResponse"` enable to limit number of batched PID query to avoid multi CAN frame response.
            * For example, OBDLinx SX can support Max 6 PID request with ISO-TP support (so can speedup by setting `"elm327BatchPIDQueryCount" : 6` and `"elm327PIDBatchQueryAvoidMultiFrameResponse" : false` ) if the ECU supports. 
            * However, my no brand bluetooth ELM327 adaptor (maybe ELM327 is clone chip) do not fully support this feature, need to set `"elm327BatchPIDQueryCount" : 2` and `"elm327PIDBatchQueryAvoidMultiFrameResponse" : true`.

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
