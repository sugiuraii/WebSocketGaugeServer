Instruction manual of WebSocketGaugeServer pre-installed raspbian image.
===

## Contents
- System requirement
- Setup instruction

## System requirement
- Raspberry pi 2 or 3
- MicroSD card over 4GB.
- Wifi interface compatible to hostapd
 - On RaspberryPi 3, internal wifi can be used.
 - On RaspberryPi 2, realtek USB Wifi dongle can be used.
 -- (Original hostapd is not compatible with realtek USB dongle. However installed hostapd is patched with [this patch](https://github.com/pritambaral/hostapd-rtl871xdrv)).
- To know compatibe ECUs, cables and controllers, please refer [this site](https://github.com/sugiuraii/WebSocketGaugeServer).

## Writes the image
This image can be written on microSD card by dd command or compatible program.
This image contains FAT32 partition (boot files and some setting files), btrfs partition (to store log), and ext4 (raspbian system).
Since websocket server related settings are stored on FAT32 partition, you can modifi the settings from Windows.

## Modify setting files.
### Select the version of hostapd(software Wifi accesspoint)
This image contains two version of hostapd.
- Ver.2.3 installed from standard raspbian package.
	- This is for interbal Wifi controller of RaspberryPi3.
- Ver.2.6 patch for realtek lan card is applied.
	- This is for cheap USB Wifi dongle with realtek chip.

These two versions of hostapd can be switched by modifying ``[FAT32 partition drive:]\etc\hostapd_choose.conf``.
```
# Uncomment following 4 lines to use default version of hostapd(2.3)
# This setting may be prefereable on Raspberry Pi 3 internal wifi controller
#PATH=/sbin:/bin:/usr/sbin:/usr/bin
#DAEMON_SBIN=/usr/sbin/hostapd
#DAEMON_DEFS=/etc/default/hostapd
#DAEMON_CONF=/etc/hostapd.conf

# Uncomment following 4 lines to use special version of Reltek driver pached hostapd(2.6+patch)
# This setting may be prefereable to use Realtek USB wifi dongle
PATH=/sbin:/bin:/usr/sbin:/usr/bin:/usr/local/sbin/hostapd-2.6-realtek
DAEMON_SBIN=/usr/local/sbin/hostapd-2.6-realtek/hostapd
DAEMON_DEFS=/etc/default/hostapd
DAEMON_CONF=/etc/hostapd-realtek.conf
```
As mentioned on the comment of ```hostapd_choose.oonf```, uncomment line 3 to 6 (and comment out line 10 to 13) to use standard(2.3) version of hostapd, uncomment line 10 to 13 (and comment out line 3 to 6) to use realtek-patched(2.6) version of hostapd.

### <font color="red">[Highly recommend] Modify wifi SSID and password.</font>
For security reason, <font color="red">**it is highly recommended to modify Wifi SSID and password before you launch the image.**</font>

To modify the SSID and password, you have to modify hostapd cnfiguration file. 
Please note that the path of hostapd configuration file is different by the version of hostapd(you choosed at previous section).

For standard Ver.2.3, the configuration file path is ``[FAT32 partition drive:]\etc\hostapd.conf``.
For realtek-patched Ver.2.6, the configuration file path is ``[FAT32 partition drive:]\etc\hostapd-realtek.conf``.

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
If you have some connection issue, you may be able to solve by changing the channel by ```channel=``` section.

### Enable WebSocketGaugeServer programs
In this raspbian image, WebSocketGaugeServer (Defi/SSM/Arduino/ELM327) can be started at bootup by supervisor. However, all of these programs are disabled by default. To enable the programs, rename configuration files at ``[FAT32 partition drive:]\supervisor_conf_extra``
```
arduino_websocket.conf.sample
defi_websocket.conf.sample
elm327_websocket.conf.sample
ssm_websocket.conf.sample
```
by removing ``.sample`` extension. For example, to enable ELM327COM_WebSocketServer, rename ``elm327_websocket.conf.sample`` to ``elm327_websocket.conf``.

You can enable more than one WebSocketServer programs at the same time. But in that case, please take care not to overlap comport device name.

### Change settion of WebSocketGaugeServer programs
WebSocketGaugeServer programs are stored in c with configuration xml files.
Nommally, the setting xml files need not to be modified (the serial port device is set to default USB serialport device name of /dev/ttyUSB0). If you connect some other serial port device, you may have to change com port setting.

To know the detail of this xml setting files, please refer README.md of WebSocketGaugeServer.

## Boot the image
After modifying setting files, insert the MicroSD card to RaspberryPi and connect power cable. Raspbian shold boot and you may find the SSID of the raspberry pi (as you made the seeting on above section).

DHCP server is already setup on the raspbian image, and IP addresses of 192.168.56.xxx shold be assigned automatically to the Wifi connected devices.

## Access to WebSocketGaugeClient
The sample files of [WebSocketGaugeClientNeo](https://github.com/sugiuraii/WebSocketGaugeClientNeo) programs are installed this raspbian image file. [Nginx](https://nginx.org/) web server program is also installed. Thus, you can access to WebGL/[pixi.js](http://www.pixijs.com/) based gauges to view the ECU/sensor information.

To access sample dashboard gauge, open ``http://192.168.56.1/`` on web browser.

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
	- Ext4, will be mouned on / (readonly, overlayfS will be applied)
