# WebSocketGaugeServer - Configure

## Configuration file
Edit `appsettings.json`.

## `"ServiceConfig"` Section
### Basic (common) configuration of ECU/Sensor communication service
Here is an example of ELM327 (OBDII) ECU communication service.
```json
"ELM327": {
      /* ---- Common setting ---- */
      "enabled": false,
      "urlpath" : "/elm327",
      "usevirtual" : false,
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
* `"usevirtual"`
  * Set true to use "virtual ECU/Sensor communication mode".
    * In this mode, you can emulate ECU/Sensor reading without connecting ECU/Sensor.
    * If usevirtual is enabled, ECU/sensor communication is not performed (i.e. comport field will be ignored). ECU/Sensor reading value can be changed from Web UI.
  * Set both `"enabled"` and `"usevirtual"` true, to use "virtual ECU/Sensor communication mode".

### ELM327 specific configuration
In addition to basic configuration,　ELM327 service has some specific configurations.
* `"baudrate"`
  * Baudrate to communicate ELM327.
    * Check the spec of ELM327 adaptor.
      *  Software OBDII simulator OBDSim (https://icculus.org/obdgpslogger/obdsim.html) uses 9600bps.
      * Typical USB/Bluetooth ELM327 adapters uses 115200bps.
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
---
## `"url"` option
* Server listen port number is set in this section.
  * Default :2016
---
## `"clientFiles"` section
```json
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
