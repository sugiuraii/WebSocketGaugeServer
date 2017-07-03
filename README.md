DefiSSMCOM_WebsocketServer
---

## Description
This program reads the car sensor data (such as vehicle speed, engine rpm, water temp, boost pressure, etc..) and broadcast the data on websocket.  

The data are brocasted on json format and can be viewed by dashboard webapp.  
The source code of dashboard webapp is available on [sugiuraii/WebSocketGaugeClientNeo](https://github.com/sugiuraii/WebSocketGaugeClientNeo)

Currently, four types of sensors are implemented.  
* Defi-Link
* SSM(Subaru select monitor)
* Arduino pulse counter (to read vehicle speed and engine rpm) + A-D converter (to read water temp, oil-temp, boost pressure, oil pressure etc..)
** Now I am trying to support Autogauge sensor
* OBD-II with ELM327 (or compatible) adaptor  
---
![WebsocketDiagram](README.img/WebsocketServerDiagram.png)

## Requirement
Windows with .net Framework 4.5 or linux with mono.

## Dependency
* [SuperWebSocket](https://github.com/kerryjiang/SuperWebSocket)
* [WebSocket4Net](https://github.com/kerryjiang/WebSocket4Net)
* [log4net](https://logging.apache.org/log4net/)
* [Json.NET](http://www.newtonsoft.com/json)

## How to build
The source code is written on Visual Studio 2013 Community. Open the DefiSSMCOM_Websocket.sln and select build. Required libraries shold be downloaded automatically by NuGet.

## Instllation and setup
Binary executable files are stored on DefiSSMCOM_WebsocketServer\DefiSSMCOM_Websocket\Debug_Distribution (or download the binary zip file on release).  
After copy (or extract) the binary files, modify the setting xml file. The file name is.  
* defiserver_settings.xml
* ssmserver_settings.xml
* arduinoserver_settings.xml
* elm327server_settings.xml

On these xml files, please modiy the `<comport>` tag

```
<!-- Set the COM port name where you connected the communication adaptor (defi-converter, SSM reader, Arduino or ELM327)-->
<comport>COM8</comport>
```
If you run this program on linux, please set the comport name by /dev/tty?? format, such as..
```
<comport>/dev/ttyUSB0</comport>
```

If you need, you can change the websocket portname by `<websocket_port>` tag. In the case of arduino or ELM327, you can also change the baudrate by `<baudrate>` tag.

## Run
On windows, please doubleclick the exe file on explorer. On linux, run with mono 
```
> mono DefiCOM_WebSocket_Server.exe
```

## License
[Apache-2.0](https://github.com/sugiuraii/DefiSSMCOM_WebsocketServer/blob/master/LICENSE)
