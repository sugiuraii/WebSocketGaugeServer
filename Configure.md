# WebSocketGaugeServer - Configure

## Configuration file
Edit `appsettings.json`.
* `appsettings.Development.json` will be loaded to overwrite the setting, if you run the program in Development environment (for example, launch the program by `dotnet run`).
  * See [https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0)
* To build custom Docker image by writing setting, refer [Setup-Docker.md](Setup-Docker.md)

## `"ServiceConfig"` Section
### Basic (common) configuration of ECU/Sensor communication service
Here is an example of ELM327 (OBDII) ECU communication service.
```js
"ELM327": {
      /* ---- Common setting ---- */
      "enabled": false,
      "urlpath" : "/elm327",
      "virtualecu" : {"enabled": false, "waitmsec" : 15},
      "comport": "/dev/ttyUSB0",
      /* ---- ELM327 specific setting --- */
      "baudrate": 9600,
      //...
      "elm327ProtocolMode": "0",
      //...
      "elm327AdaptiveTimingControl": 1,
      "elm327Timeout" : 50
},
```
* `"enabled"`
  * Set true, to enable service.
* `"urlpath"`
  * Set the websocket connection path.
  * eg.) Websocket url is set to  `ws://(Server address):(port)/elm327` int this example.
  * Usually, not necessary to change.
* `"comport"`
  * Serial port device path to communicate to ECU/Sensor.
  * In windows, set like this.
    ```json
    "comport": "COM3",
    ```
  * In linux, set like,
    ```json
    "comport": "/dev/ttyUSB0",
    ```
  * Since version 3.5(beta2), comport communication can be tunneled by TCP (UART over TCP). To use it, set like this. (12345 is port number of TCP-UART tunnel server.)
    ```json
        "comport": "tcp:hostname.of.remote.com:12345"
    ```
    or, 
    ```json
        "comport": "tcp:192.168.12.34:12345",
    ```
* `"virtualecu"`
  * This section is used for virtual ecu mode.
  * Set `"enabled"` to `true` to use "virtual ECU/Sensor communication mode".
    * In this mode, you can emulate ECU/Sensor reading without connecting ECU/Sensor.
    * If virtual ecu mode is enabled, ECU/sensor communication is not performed (i.e. comport field will be ignored). ECU/Sensor reading value can be changed from Web UI.

### ELM327 specific configuration
In addition to basic configuration,　ELM327 service has some specific configurations.
* `"baudrate"`
  * Baudrate to communicate ELM327.
    * Check the spec of ELM327 adaptor.
      *  Software OBDII simulator [OBDSim](https://icculus.org/obdgpslogger/obdsim.html) uses 9600bps.
      * Typical USB/Bluetooth ELM327 adapters uses 115200bps.
      * On tcp wrapper mode (set `tcp:` in `"comport"` section, baudrate setting is not cared. The program may cause warning when non-standard bardrate is set, you can ignore the warning.).
        * In this case, Set bardrate on TCP-UART wrapper side.
* `"elm327ProtocolMode"`
  * Set the OBDII protocol of ELM327
    ```json
    /*
        ELM327 protocol setting.
        (See the section of "AT SP" commandd in ELM327 data sheet (p.24-25))
        (Use single character string to set this field.)

        "0" - Automatic
        "1" - SAE J1850 PWM (41.6 kbaud)
        "2" - SAE J1850 VPW (10.4 kbaud)
        "3" - ISO 9141-2  (5 baud init, 10.4 kbaud)
        "4" - ISO 14230-4 KWP (5 baud init, 10.4 kbaud)
        "5" - ISO 14230-4 KWP (fast init, 10.4 kbaud)
        "6" - ISO 15765-4 CAN (11 bit ID, 500 kbaud)
        "7" - ISO 15765-4 CAN (29 bit ID, 500 kbaud)
        "8" - ISO 15765-4 CAN (11 bit ID, 250 kbaud)
        "9" - ISO 15765-4 CAN (29 bit ID, 250 kbaud)
        "A" - SAE J1939 CAN (29 bit ID, 250* kbaud)
        "B" - USER1 CAN (11* bit ID, 125* kbaud)
        "C" - USER2 CAN (11* bit ID, 50* kbaud)
        */
    ```
* `"elm327AdaptiveTimingControl"`
* `"elm327Timeout"`
  * ```json
    /*
        ELM327 adaptive timeing control ("elm327AdaptiveTimingControl") (AT AT command), 
        and ELM327 timeout (AT ST command).

        "elm327AdaptiveTimingControl" : Set adaptive timing control mode for ELM327 (see AT0,1,2 command section of ELM327 data sheet (p.12)).
        0 : Disable adaptive timing control.
        1 : Enable adaptive timing control (default).
        2 : Enable aggressive adaptime timing control.

        "elm327Timeout" : Set ELM327 timeout (max 255, in increments of 4 msec (or 20 msec if in the J1939 protocol, with JTM5 selected)).
        (Default = 50 (50*4 = 200ms))
        (See AT ST commandd of ELM327 data sheet (p.26)). 
      */
    ```
* `"elm327HeaderBytes"`
  * ```json
     /*
        Set header of ELM327.
        See the command instruction of "AT SH" in ELM327 data sheet.
        This field (and AT SH command) will be ignored when the value is blank string ("").
        (For example, you can manually set the CAN ID of ECU. By doing this, you can eliminate unnecessay CAN traffic when multiple ECUs (ECU+TCU, etc..) exists.)
        (For the detail, see "Setting the Headers " in ELM327 data sheet.) 
      */
* `"elm327BatchPIDQueryCount"`
    * ```json      
      //  The SAE J1979 (ISO 15031-5) standard allows requesting multiple PIDs with one message, but only if you connect to the vehicle with CAN (ISO 15765-4).
      //  Up to 6 parameters may be requested at once,

      /*
        Number of PID batch query. (1 to 6 is allowed)
        Query several PID by single query.
        Multiple batched query may improve PID update rate.

        If the adaptor or ECU support batched query, you can set this value to 6.
        If the adaptor or ECU do not support, set this to 1.
        (However, some of OBDII dongle can handle less tha 6 PIDs. If you have trouble, reduce this PID count may help.)
        (For example, no brand ELM327 Ver2.1 bluetooth dongle can handle only 2 PIDs for batch.)

        From ELM327 data sheet p.45:
          Multiple PID Requests
          The SAE J1979 (ISO 15031-5) standard allows
          requesting multiple PIDs with one message, but only if
          you connect to the vehicle with CAN (ISO 15765-4).
          Up to six parameters may be requested at once, and
          the reply is one message that contains all of the
          responses.
        (To disable batch PID query, set this field to 1.)
      */
      ```
* `"elm327PIDBatchQueryAvoidMultiFrameResponse" `
  * ```json
      /*
        On batched PID request, ECU response message may exceed CAN payload length (of 8bytes include mode and size bytes.)
        In this case, ECU makes response message by multiple (splitted) CAN data frames using ISO-TP (ISO 15765-2)
        However, some of OBDII dongle cannot handle ISO TP (my no brand chinese ELM327V2.1 dongle (maybe unofficial clone) cannot handle ISO-TP).
        In this case, set this enable to limit number of batched PID query to avoid multi CAN frame response.
      */
    ```
---
## `"url"` option
* Server listen port number is set in this section.
  * Default :2016
---
## `"clientFiles"` section
```js
  "clientFiles":
  {
    // Set true to host client program files (html,js) by using asp.net Kestrel web server
    "enabled": false,
    "contentPath" : "clientfiles"
  },
```

* This program has a function to host files (such as websocket client html/js files), by using ASP.net kestrel webserver.
  * To enable file hosting, set `"enabled": true`
  * You can set directory path of hosted files by changing "contentPath". Both absolute path and relative path are accepted.
* Do not need to enable `"clientFiles"` features, if prepare separated web server to host websocket client html/js files. (Including Docker container of WebSocketGaugeClient)
