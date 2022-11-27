# WebSocketGaugeServer - クイックスタート
* このファイルではとりあえず動作を試したいときのために、OBDII+ELM327でECUと通信するモードの起動、設定方法を紹介します。
* それ以外のECU通信を使う手順は現在作成中です。ソースコードを参照するか、メール等にて質問ください。
    * 尚、本プログラムは実車での動作確認をあまり行ってなく（数年前に実家のプリウスで動作確認したのみ）、開発上のテストはECUシミュレータでのみ行っておりますので、実車動作においては不具合が出るかもしれませんのでご容赦ください。

## バイナリファイルの取得
* [GitHubのReleaseページ](https://github.com/sugiuraii/WebSocketGaugeServer/releases/)より対応するOSのバイナリアーカイブファイルをダウンロードの後、適当なディレクトリに展開する。

## 設定ファイルの編集
* 展開したディレクトリ内にある`appsettings.json`を編集する。下記の部分を目的に応じて編集ください。
```js
"ServiceConfig": {
    "ELM327": {
      "enabled": true,
      "urlpath" : "/elm327",
      "usevirtual" : false,
      "comport": "/dev/ttyUSB0",
      "baudrate": 115200,
      "elm327ProtocolMode": "0",
      "elm327AdaptiveTimingControl": 1,
      "elm327Timeout" : 50
```
上記部分のうち
* `"enabled"`:ELM327+OBDII通信機能を有効化します。
* `"urlpath"`:変更不要（WebSocket通信を開始する際のPath)
* `"usevirtual"`: 仮想ECUの設定をします。ELM327を実際に接続する際は`false`にしてください。
    * `true`にした場合、ELM327への接続はせずに、代わりにプログラム内部で仮想ECUを作成します。実車がない場合でテストをするときやメーターGUI（メーターパネル）のデザイン、デバッグ時に使用してください。
* `"comport"`: ELM327を接続するシリアルポート名を設定します。Linuxなら`/dev/tty*`形式になります。Windowsの場合は`COM*`形式になります。
* `"baudrate"`:ELM327と通信する際のシリアル速度（ボーレート）を設定します。
* `"elm327ProtocolMode"` : ELM327-ECU通信プロトコルを設定します。デフォルトは自動(0)ですが、通信できない場合は手動で設定してください。
* `"elm327AdaptiveTimingControl"`: ELM327-ECU通信時のタイミング設定です。ECUからELM327へデータが転送される際の待ち時間を設定します。標準は1の”adaptive timing control”有効ですが、遅延が大きい場合は2の"aggressive adaptime timing control"を使用したり、0に設定してadaptive timing controlを無効にし、`"elm327Timeout"`で待ち時間を手動設定することが可能です。

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
