Instruction of WebSocketGaugeServer pre-installed raspbian image.
===

Japanese version of this document is [here](./RasobianImageSetup.ja.md)

## Contents
- System requirement
- Write the image
- Modify setting files
- Boot the image
- Access to WebSocketGaugeClient
- Appendix

## System requirement
- Raspberry pi 3
- MicroSD card over 4GB.
- One of following ECU or controller with communication cable
	- ECU capable of Subaru select monitor(SSM), and cable (OpenPort 1.2 compatible).
	- ELM327 or compatible cable (USB cable)
		- If you want to use ELM327 bluetooth adapter, you have to setup rfcomm by yourself.
    - Defi Link and UART-USB converter cable.
    - Arduino with sensor.
    	- Please note that the fuelrate (Nenpi, in Japanese) and trip are supported only on SSM and ELM327.
- To know about the details, please refer [this site](https://github.com/sugiuraii/WebSocketGaugeServer).

## Writes the image
**Image is available on [this site](https://1drv.ms/f/s!ABvK9JwSE9xVkkc).**

This image can be written on microSD card by dd command or compatible program.
This image contains FAT32 partition (boot files and some setting files), btrfs partition (to store log), and ext4 (raspbian system).
Since websocket server related settings are stored on FAT32 partition, you can modify the settings from Windows.

## Modify setting files.
### <font color="red">[Highly recommend] Modify wifi SSID and password.</font>
For security, <font color="red">**it is highly recommended to modify Wifi SSID and password before you launch the image.**</font>

To modify the SSID and password, you have to modify hostapd cnfiguration file.
Configuration file path is ``[FAT32 partition drive:]\etc\hostapd.conf``.

```
interface=wlan0
driver=nl80211
#driver=rtl871xdrv
ssid=Pi3-AP
hw_mode=g
channel=6
ieee80211n=1
wmm_enabled=1
ht_capab=[HT40][SHORT-GI-20][DSSS_CCK-40]
macaddr_acl=0
auth_algs=1
ignore_broadcast_ssid=0
wpa=2
wpa_key_mgmt=WPA-PSK
wpa_passphrase=raspberry
rsn_pairwise=CCMP
```
At least, it is better to modify SSID and WPA_passphrase(password) by ```ssid=``` and ```wpa_passphrase=``` section.

### Enable (one of) WebSocketGaugeServer programs
**The image file is set to use SSM (Subaru select monitor) to communicate with ECU by default. If you use this program with SSM, you do not have to change the setting.**

In this raspbian image, WebSocketGaugeServer (Defi/SSM/Arduino/ELM327) can be started on the bootup by [supervisor](http://supervisord.org/).
SSM websocket server is enabled by default.
To use other websocket server programs
* Open ``[FAT32 partition drive:]\supervisor_conf_extra``.
* Rename ``ssm_websocket.conf`` to ``ssm_websocket.conf.sample`` (to disable SSM websocket server).
* And remove the ``.sample`` extension of one of following configuration files.
```
arduino_websocket.conf.sample
defi_websocket.conf.sample
elm327_websocket.conf.sample
ssm_websocket.conf.sample
```
For example, to enable ELM327COM_WebSocketServer, rename ``elm327_websocket.conf.sample`` to ``elm327_websocket.conf``.

Please note that you can enable only one websocket program at the same time (this is because all of these 4 websocket server program try to access the same COM port (of /dev/ttyUSB0). If you enable multiple websocket programs, each websocket programs conflict to occupy the COM port. (In order to use multiple websocket server programs, you can change COM port name by setting websocket server config file. See  [this site](https://github.com/sugiuraii/WebSocketGaugeServer).)

### Setup FUEL and TRIP logger program.
If you use **SSM or ELM327** for ECU communication, you can use ``FUELTRIP_Logger.exe`` program to calculate trip distance and fuel consumption (and get fuel rate (Nenpi)). (To know the detail of ``FUELTRIP_Logger.exe``, please see [FUELTRIPLogger.md](./FUELTRIPLogger.md)).

``FUELTRIP_Logger.exe`` uses **SSM websocket server by default (Thus, you do not have to change the setting if you use SSM.)**. If you use this program with ELM327, delete ``[FAT32 partition drive:]\websocket_programs\FUELTRIP\fueltriplogger_settings.xml``, and copy one of following xml files to ``[FAT32 partition drive:]\websocket_programs\FUELTRIP\fueltriplogger_settings.xml``.
```
(Sample config file 1 : Get fuel consumption rate from 'Engine fuel rate' OBDII PID.)
[FAT32 partition drive:]\websocket_programs\FUELTRIP\setting_examples\fueltriplogger_settings.ELM327.FUELRate.Sample.xml
(Sample config file 2 : Calcualte fuel consumption from mass air flow.)
[FAT32 partition drive:]\websocket_programs\FUELTRIP\setting_examples\fueltriplogger_settings.ELM327.MAF.Sample.xml
```

## Boot the image
After modifying setting files, insert the MicroSD card to RaspberryPi and connect power cable. Raspbian should starts and you may find the SSID of the raspberry pi (as you made the seeting on above section).

DHCP server is already setup on the raspbian image, and IP addresses of 192.168.56.xxx shold be assigned automatically to the Wifi connected devices.

## Access to WebSocketGaugeClient
The sample files of [WebSocketGaugeClientNeo](https://github.com/sugiuraii/WebSocketGaugeClientNeo) programs are installed this raspbian image file. [Nginx](https://nginx.org/) web server program is also installed. Thus, you can access to WebGL/[pixi.js](http://www.pixijs.com/) based gauges to view the ECU/sensor information.

To access sample dashboard gauge, open **``http://192.168.56.1/``** on web browser.

The client html/javascript files are stored in ``[FAT32 partition drive:]\public_html``. You can replase the web server contents by modifing this directory.

And it is possible to view WebSocketGaugeServer logs by accessing ``http://192.168.56.1/supervisor_log`` (This URL export ``/var/log/supervisor`` of raspian filesystem. If you face some issues, please refer this URL at first.

## Appendix
### Unlock raspbian root partition (change readonly to write enable)
In this image, root and /boot partition are set read-only (to avoid system partition crash by power-off without shutdown). All of the modification of these partition are actually write to ram drive (by overlayFS), and will be flushed after shutdown or reboot.

You can disable overlayFS and make root and /boot partition writable by renaming ``[FAT32 partition drive:]\noprotect.bak`` to ``[FAT32 partition drive:]\noprotect``. The raspbian system checks the existence of this ``noprotect`` files, and disable overlayFS if this file is found.

### Partitions of this image
- Primay partition 1
	- FAT32, will be mounted on /boot (readonly, overlayFS will be applied)
	- Store raspbian bootup files, WebSocketServer programs, html files, and some setting files.
- Primary partition 2
	- Btrfs, will be mounted on /var/log (read/write, overlayFS will NOT be applied)
	- Store logs.
- Primary partition 3
	- Ext4, will be mouned on / (readonly, overlayFS will be applied)
