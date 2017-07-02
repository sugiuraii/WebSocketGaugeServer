DefiSSMCOM_WebsocketServer
---

## Description
This program reads the car sensor data (such as vehicle speed, engine rpm, water temp, boost pressure, etc..) and broadcast the data on websocket.  

Currently, four types of sensors are implemented.  
* Defi-Link
* SSM(Subaru select monitor)
* Arduino pulse counter (to read vehicle speed and engine rpm) + A-D converter (to read water temp, oil-temp, boost pressure, oil pressure etc..)
** Now I am trying to support Autogauge sensor
* OBD-II with ELM327 (or compatible) adaptor  

---
![WebsocketDiagram](README.img/WebsocketServerDiagram.png)
---


