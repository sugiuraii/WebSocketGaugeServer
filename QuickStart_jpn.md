# WebSocketGaugeServer - クイックスタート
* このファイルではとりあえず動作を試したいときのために、OBDII+ELM327でECUと通信するモードの起動、設定方法を紹介します。
* それ以外のECU通信を使う手順は現在作成中です。ソースコードを参照するか、メール等にて質問ください。
    * 尚、本プログラムは実車での動作確認をあまり行ってなく（数年前に実家のプリウスで動作確認したのみ）、開発上のテストはECUシミュレータでのみ行っておりますので、実車動作においては不具合が出るかもしれませんのでご容赦ください。

## バイナリファイルの取得
* [GitHubのReleaseページ](https://github.com/sugiuraii/WebSocketGaugeServer/releases/)より対応するOSのバイナリアーカイブファイルをダウンロードの後、適当なディレクトリに展開する。
    * 3.5/Beta2よりWebSocketGaugeServerはlinux/windowsサーバーを別途用意しなくてもtermux+android上で動作可能になりました. See [Install-Termux_jpn.md](Install-Termux_jpn.md) for detail.

## 設定ファイルの編集
* 展開したディレクトリ内にある`appsettings.json`を編集する。下記の部分を目的に応じて編集ください。
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
上記部分のうち
* `"enabled"`:ELM327+OBDII通信機能を有効化します。
* `"urlpath"`:変更不要 (WebSocket通信を開始する際のPath)
* `"virtualecu"`: 仮想ECUの設定をします。
    * `"enabled"`を`true`にした場合、ELM327等への接続はせずに、代わりにプログラム内部で仮想ECUを作成します。実車がない場合でテストをするときやメーターGUI（メーターパネル）のデザイン、デバッグ時に使用してください。
        * 実際にELM327等を使用する場合は`false`にしてください。
    * `"waitmsec"`  仮想ECU使用時のウエイト時間を設定します。この数値を増やすと遅いECU通信をシミュレートすることができます。    
* `"comport"`: ELM327を接続するシリアルポート名を設定します。Linuxなら`/dev/tty*`形式になります。Windowsの場合は`COM*`形式になります。
    * 3.5/Beta2よりシリアルポート通信をTCPでトンネルすることができるようになりました。これはUART-Wifiアダプタ, ELM327-WifiアダプタやSerial-TCPトンネルプログラムを使う場合に便利かもしれません.
        * このサーバープログラムをAndroid+Termux上で使用する際はTCPラッパの使用は必須になります。（TermuxがシリアルポートやBluetootheへのアクセスを禁止しているため）
    * TCPラッパ使用時には以下のように設定してください。
        ```jsonc
        "comport": "tcp:hostname.of.remote.com:xxxxx",
        ``` 
        or
        ```jsonc
        "comport": "tcp:192.168.xx.xx:xxxxx",
        ```
* `"baudrate"`:ELM327と通信する際のシリアル速度（ボーレート）を設定します。(TCP-Serialラッパ使用時には無視されます。)
* `"elm327ProtocolMode"` : ELM327-ECU通信プロトコルを設定します。デフォルトは自動(0)ですが、通信できない場合は手動で設定してください。
* `"elm327AdaptiveTimingControl"`: ELM327-ECU通信時のタイミング設定です。ECUからELM327へデータが転送される際の待ち時間を設定します。標準は1の”adaptive timing control”有効ですが、遅延が大きい場合は2の"aggressive adaptime timing control"を使用したり、0に設定してadaptive timing controlを無効にし、`"elm327Timeout"`で待ち時間を手動設定することが可能です。
* `"elm327HeaderBytes"` : ELM327 PIDリクエスト時のヘッダを設定します.
    * 詳細はELM327データシートの "AT SH" コマンドの項目を参照.
    * 空の場合("")この項目は無視されて、AT SHコマンドは発行されません.
    * これを設定することによって、通信先ECUのCAN ID of ECUを明示的に指定することができます。CAN BUSに複数のECU(ECU+TCUとか)が接続されている際に、ここを設定すると対象のECのみにPIDリクエストを送信できるようになり、他のECUが不要な応答をすることを防止できます。
       * (For the detail, see "Setting the Headers " in ELM327 data sheet.) 
* `"elm327BatchPIDQueryCount"`
* `"elm327PIDBatchQueryAvoidMultiFrameResponse"`
    * 1回のPIDリクエストで要求するPIDの数です。(Default is 1 Max is 6.)
    * SAE J1979 (ISO 15031-5)では、CANBUS(ISO 15765-4)経由での通信の際、一度のPIDリクエストで6つのPIDを同時リクエストできるとあります。 
    * この機能がECUとELM327によってサポートされているなら、本設定を適切に行うことでPIDリクエストを高速化することが可能となります。
        * 複数PID要求をする場合、ECUからの返答がCANメッセージの最大サイズである8バイトを超えるため、ISO-TP (ISO 15765-2)に従ってメッセージが分割送信されます。
        *  しかし、一部のELM327アダプタではこの分割送信されたPIDレスポンスを正しくハンドルすることができません。(特に中華クローンのELM327デバイス).
        * この場合 `"elm327PIDBatchQueryAvoidMultiFrameResponse"`を `enable`にすることで、ECUからの返信が分割されない程度にPID李衣クエスト数を自動的に制限することが可能です。
            * 例えばOBDLinx SXアダプタは6PIDリクエストをISO-TP サポート付きでハンドルできますが、 ( `"elm327BatchPIDQueryCount" : 6` `"elm327PIDBatchQueryAvoidMultiFrameResponse" : false` として通信を6倍高速化可能(ECUのサポートがあれば))
            * ノーブランドのbluetooth ELM327 アダプタ (ELM327チップは多分クローン品)はこの機能を完全にサポートできず, `"elm327BatchPIDQueryCount" : 2` および `"elm327PIDBatchQueryAvoidMultiFrameResponse" : true`　と設定する必要がありました(なので高速化も高々2倍).

## プログラムの起動
* 展開したディレクトリ内にある`WebSocketServer.exe`(Windows)または`WebSocketServer`(Linux)実行ファイルをエクスプローラまたはターミナル等から起動してください。

## 設定ページへのアクセス
* 起動したPCの2016番ポートにWebブラウザでアクセスします。
    * 起動したPCのWebブラウザを使用する場合は`http://localhost:2016/`
    * 他のPCやスマホ等からアクセスする場合は`http://(WebSocketServerを起動したPCのアドレス):2016/`
        * このプログラムは現在セキュリティ関係の機能を実装していないため、意図しない端末からのアクセスを避けるためにWifiのセキュリティやファイアウォールの設定をするように注意してください。
* 左側のメニューにおいて、設定ファイルにて仮想ECUを有効にしている場合は、VirtualELM327Controlタブから仮想ECUのセンサ値設定ページに移動できます。移動後はスライダでセンサ値を設定できます。

## サンプルメーター（クライアント）の起動
* メーターを表示したいPCないしはスマホ等で、上記設定ページをアクセスしたあと、Gauge clientメニューをクリックしてください。
* 以降、メーター選択メニューに入るため、表示させたいメーターサンプルをクリックしてください。
