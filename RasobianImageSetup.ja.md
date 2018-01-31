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
**SSMまたはELM327でECUとの通信を行っている場合** 、``FUELTRIP_Logger.exe`` プログラムでトリップ、燃料消費量、燃費を取得しWebsocket経由で配信することが可能です。 (``FUELTRIP_Logger.exe``の詳細は[FUELTRIPLogger.md](./FUELTRIPLogger.md)を参照してください).

``FUELTRIP_Logger.exe``は標準で **SSM websocket server を使用するように設定されています**. もしELM327で燃費計算を行う場合は, ``[FAT32 partition drive:]\websocket_programs\FUELTRIP\fueltriplogger_settings.xml``を削除し、下記2つのファイルのうちどれか一つを``[FAT32 partition drive:]\websocket_programs\FUELTRIP\fueltriplogger_settings.xml``へ上書きコピーしてください。
```
(Sample config file 1 : OBDII PIDのうち 'Engine fuel rate'のPIDから直接燃料消費量を取得.)
[FAT32 partition drive:]\websocket_programs\FUELTRIP\setting_examples\fueltriplogger_settings.ELM327.FUELRate.Sample.xml
(Sample config file 2 : エアフローメーター値から燃料消費量を換算（A/F比は14.7を仮定.)
[FAT32 partition drive:]\websocket_programs\FUELTRIP\setting_examples\fueltriplogger_settings.ELM327.MAF.Sample.xml
```
SSMでの標準設定ではインジェクターパルス幅と回転数から燃料消費量を計算していますが、燃料消費量は気筒数とインジェクタ容量によって異なります。さらに、計算されたトリップ・燃料消費量と実際の値のズレを補正するための補正係数（スケーリング）を設定することも可能です。これら設定については[FUELTRIPLogger.md](./FUELTRIPLogger.md)および``fueltriplogger_settings.xml``内のコメントを参照ください。

## MicroSDカードからの起動
設定ファイル編集後は、PCからSDカードをアンマウント・取り外しをしてRaspberryPiへ挿入、電源ケーブルをつなげれば起動します。Raspbian起動後数分で、設定されたSSIDによるWifiアクセスポイントが作成されます。

DHCPサーバーもRaspbianにて組み込まれており、上記SSIDへ接続すると192.168.56.xxxのIPアドレスが自動的に割り振られます.

## メーターパネル（Webベースクライアント）への接続
このRaspbianイメージにはWebサーバ(nginx)も組み込まれており、ここへアクセスすることでECU->Websocketへ配信された車両情報をメーターとして表示させることが可能になります。クライアントの詳細・対応Webブラウザは[WebSocketGaugeClientNeo](https://github.com/sugiuraii/WebSocketGaugeClientNeo)を参考にしてください。WebGLまたはHTML5 canvasに対応したブラウザにて表示可能です。

クライアントへアクセスする際は、スマホ・PC等で上記SSIDにWifiを接続させた上で Webブラウザにて**``http://192.168.56.1/``**のURLへアクセスしてください。

クライアントとなるメーターパネルのhtmlファイル、javascriptファイル等は ``[FAT32 partition drive:]\public_html``に格納されています. 自分でメーターパネルを書き換える等クライアントプログラムを改造・修正した際は上記パスへ上書きしてください。

またサーバープログラムのログは ``http://192.168.56.1/supervisor_log`` でアクセスできます。(このURLはRaspbianディレクトリツリーの ``/var/log/supervisor`` をエクスポートしています）

## Appendix
### Raspbianパーティション(ext4)のリードオンリー解除
このイメージファイルは車載を想定しているため、Raspbianを起動した際に突然の電源断に対応できるように（ログを格納するbtrfsパーティションを除いて）リードオンリーマウントされるように設定されています（ファイルシステムへの変更はOverlayFS経由でramドライブに書き込まれます）。Raspbian起動中にファイルシステムへ行われた変更は、次回起動時にすべてリセットされます。

このリードオンリー状態は``[FAT32 partition drive:]\noprotect.bak`` を ``[FAT32 partition drive:]\noprotect``へリネームすることで解除可能です（Raspbian起動時にこのファイルの存在をチェックして、存在していたらリードオンリーマウントをスキップします）.ただし、リードオンリーマウントがされているRaspbian上でこの変更を行っても、次回変更時に無効にされてしまいますので、この変更は別のPCなどで行ってください（FAT32上のファイルなのでWindows上からも変更可能です）。あるいは書き込み可能状態で再度マウントしてください。

### このイメージのパーティション構成
- Primay partition 1
	- FAT32, will be mounted on /boot (readonly, overlayFS will be applied)
	- Store raspbian bootup files, WebSocketServer programs, html files, and some setting files.
- Primary partition 2
	- Btrfs, will be mounted on /var/log (read/write, overlayFS will NOT be applied)
	- Store logs.
- Primary partition 3
	- Ext4, will be mouned on / (readonly, overlayfS will be applied)
