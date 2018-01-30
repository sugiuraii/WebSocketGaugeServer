WebSocketGaugeServerプリインストール済みRaspbianイメージファイル
===

## Contents
- 必要なもの
- イメージの書き込み
- 初期設定
- イメージの起動
- メーターパネル（クライアント）へのアクセス法
- Apendix

## 概要
Websocketを使ったHTML5(WebGL)ベースのメーターパネルを簡単に試すことができるように、プリインストール済みのRaspbianイメージを用意しました。
標準ではスバル車用のSubaru Select monitorで通信できるように設定されています。設定を変更すればELM327を使用することも可能（なはずです）。

## 必要なもの
- Raspberry pi 3
- 4GB以上のMicroSDカード
- 下記のうちどれか1つのECUを装備した車+通信ケーブル、またはセンサーコントローラ（メーター表示できる内容はそれぞれ異なります）
	- Subaru select monitor(SSM)で車両情報をOBDIIコネクタ経由で通信可能なスバル車、及びOpenPort 1.2互換ケーブル.
	- ELM327にてOBDIIコネクタ経由で通信可能な車両と、ELM327搭載OBDケーブル(USB接続)
		- 技術的にはELM327搭載Bluetoothアダプタも使用可能ですが、その場合はrfcommの設定を行ってください.
    - DefiLink(advanceではない)コントロールユニット、センサとDefiLink-シリアル(UART）変換ケーブル
    	- [参考](http://kaele.com/~kashima/car/defigate/)
    - Arduino UNOとAutogauge　圧力・温度センサ
    	- [Ardunoスケッチはこちら](https://github.com/sugiuraii/ArduinoPulseSensorGeneratorReader)
- 燃費測定用プログラム(FUELTRIP_Logger.exe)はSSMまたはELM327のみ可能.
- 開発はエミュレータで行い、実車でのテストは十分に行えていないのでバグ等あるかもしれません。
	- 当方海外赴任中のため手許に車がなく、一時帰国時に帰省した時のみ実車でのテストを行っています。
- 詳細は[このプロジェクトを参照ください](https://github.com/sugiuraii/WebSocketGaugeServer).

## イメージの書き込み
**イメージファイルは [こちらからダウンロード可能です](https://1drv.ms/f/s!ABvK9JwSE9xVkkc).**

イメージファイルはxzファイルを解凍後、ddコマンドまたは互換プログラムにてマイクロSDカードに書き込めます。.
書き込み後のマイクロSDカードにはFAT32(raspbianブート用、及び設定保存用)、btrfs(ログ記録用)、ext4（raspbianシステム用）の3パーティションが存在し、最初のFAT32パーテょションはWindowsエクスプローラで表示可能です。設定ファイルはFAT32パーテょションに集めていますので、Windows上のテキストエディタより設定編集可能です。

## 初期設定

### <font color="red">[強く推奨] Websocket配信用SSIDとWPAパスフレーズを変更する</font>
作成したマイクロSDカードでRaspberryPiを起動すると、RaspberryPi3に内蔵されているWifiを(DHCPつき）アクセスポイントにして、ECUやセンサユニットから読み込んだ車両情報をWifi（+Websocket)経由でPCやスマホに配信します。

セキュリティの観点より, <font color="red">**イメージ使用前にSSIDとパスフレーズを自分で設定されることを強く推奨します.(でないと配信された車両情報が筒抜けになります）**</font>

アクセスポイント(hostapd)設定用ファイルは``[FAT32 partition drive:]\etc\hostapd-realtek.conf``にあります。
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
少なくとも```ssid=``` and ```wpa_passphrase=``` のセクションは任意の設定に変更してください。

### Websocketサーバープログラムの起動設定変更
**このイメージファイルでは、スバル車用SSM (Subaru select monitor)で通信するように予め設定されています。SSMでの通信をする場合は設定変更は基本的に不要です**

このRasbianイメージでは、（ECUやセンサユニットからシリアル経由で情報を受信して、Websocketで配信する）WebSocketGaugeServer(Defi/SSM/Arduino/ELM327)をsupervisor経由で（デーモンのように）使用しています。
本節での設定によってraspbian起動時に自動的にWebSocketGaugeServerプログラムが起動されます。

SSM用のサーバープログラムがデフォルトで有効となっていますが、別のサーバープログラムを使用する際は、
* ``[FAT32 partition drive:]\supervisor_conf_extra``のフォルダを開く.
* ``ssm_websocket.conf`` を ``ssm_websocket.conf.sample``へリネームする (これでSSM用サーバープログラムが起動しないようになる).
* 下記4つの設定ファイルのうち、使用するえECU・センサユニットに応じて一つのファイルで ``.sample``の拡張子を消す.
```
arduino_websocket.conf.sample
defi_websocket.conf.sample
elm327_websocket.conf.sample
ssm_websocket.conf.sample
```
例えばELM327を使うなら``elm327_websocket.conf.sample``を``elm327_websocket.conf``とリネームする.

4つのサーバープログラムはどれも同一のCOMポート(``/dev/ttyUSB0``)を占有しようとするので、複数種類のサーバープログラムを同時に起動することはできません（COMポートの占有が衝突します）。ただし、サーバープログラムによって使用するCOMポートを変えれば同時に起動させることがは可能です。[参照](https://github.com/sugiuraii/WebSocketGaugeServer).)

### 燃料消費、トリップ、燃費計算プログラムの設定.
If you use **SSM or ELM327** for ECU communication, you can use ``FUELTRIP_Logger.exe`` program to calculate trip distance and fuel consumption (and get fuel rate (Nenpi)). (To know the detail of ``FUELTRIP_Logger.exe``, please see [FUELTRIPLogger.md](./FUELTRIPLogger.md)).

``FUELTRIP_Logger.exe`` uses **SSM websocket server by default (Thus, you do not have to change the setting if you use SSM.)**. If you use this program with ELM327, delete ``[FAT32 partition drive:]\websocket_programs\FUELTRIP\fueltriplogger_settings.xml``, and copy one of following xml files to ``[FAT32 partition drive:]\websocket_programs\FUELTRIP\fueltriplogger_settings.xml``.
```
(Sample config file 1 : Get fuel consumption rate from 'Engine fuel rate' OBDII PID.)
[FAT32 partition drive:]\websocket_programs\FUELTRIP\setting_examples\fueltriplogger_settings.ELM327.FUELRate.Sample.xml
(Sample config file 2 : Calcualte fuel consumption from mass air flow.)
[FAT32 partition drive:]\websocket_programs\FUELTRIP\setting_examples\fueltriplogger_settings.ELM327.MAF.Sample.xml
```

## Boot the image
After modifying setting files, insert the MicroSD card to RaspberryPi and connect power cable. Raspbian shold boot and you may find the SSID of the raspberry pi (as you made the seeting on above section).

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
	- Ext4, will be mouned on / (readonly, overlayfS will be applied)
