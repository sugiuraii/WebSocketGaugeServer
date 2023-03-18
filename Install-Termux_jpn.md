# Android + TermuxにWebSocketGaugeServerをインストールする
* 本サーバプログラムは、linux版をAndroid+Termux上で動作することを確認しています。
* 本ドキュメントは、本サーバプログラムをTermuxにインストールするための手順を説明するものです。

## 動作要件
* Androidスマートフォン/タブレット/PC等
    * arm64はテスト済みです。(arm32とx64のバイナリもありますが、テストしていません。)
* Google play storeからtermuxをインストールする許可が必要です。
    * termuxの最新バージョンはGoogle play storeで利用できないため、apkでインストールする必要があります。
    * Google play storeのtermuxは現在放置されており、使用することができません。公式も使用を推奨していません。
* UART-LAN(Wifiなど)ゲートウェイ、ELM327 wifi アダプタ、(USB/Bluetooth) serial -> TCP ラッパー android アプリ。
    * TermuxはBluetoothやUSB-serialアダプタとの通信を許可していないので、（WebSockeGaugeServerの）Serial->tcpトンネルモードが必要です。
        * [本サーバ] <-(tcpトンネル(ラッパー)モードによるTCP接続) -> [TCP->Serial wrapper android app] <-(BTまたはUSBシリアル) -> [ELM327 BT/USBアダプタ]。
        * [本サーバ] <-(tcpトンネル（ラッパー）モードによるTCP接続) <-(Wifi) -> [ELM327 wifiアダプタ]。

## termuxをインストールする
* [github release page](https://github.com/termux/termux-app/releases) または [F-driod のページ](https://f-droid.org/packages/com.termux/) から termux apk をダウンロードします。
* ダウンロードしたapkファイルをインストールします。ダウンロードしたapkファイルをインストールします(提供元不明のapkをインストールする許可が必要です)。
* termuxを起動し、`pkg update`と`pkg upgrade`を実行してパッケージを更新します。

## Proot-distro で termux に debian (または ubuntu) アセットをインストールします。
* termuxに `proot-distro` パッケージをインストールする。
    ```
    pkg install proot-distro
    ```
* `debian-wsgauge` と名前をつけて `debian` ディストリビューションをインストールします。
    ```
    proot-distro install --override-alias debian-wsgauge debian
    ```
    * `debian`の代わりに`ubuntu`をインストールすることもできます。
* インストールされた `debian` 環境にログインする。
    ```
    proot-distro login debian-wsgauge
    ```
* `debian` パッケージの更新
    ```
    apt update
    apt upgrade
    ```
* `libicu` を `debian` 環境にインストールする。(libicuはdotnetのランタイムを動かすのに必要です)
    ```
    apt install libicu-dev
    ```

## WebSocketGaugeServer のバイナリをインストール
* [WebSocketGaugeServer release](https://github.com/sugiuraii/WebSocketGaugeServer/releases)のページから、 `debian` 環境にログインした後、Linux バイナリをダウンロードします。
    ```
    curl -OL https://github.com/sugiuraii/WebSocketGaugeServer/releases/download/4.0%2FRC1/WebsocketGaugeServer-4.0-RC1-linux-arm64.tar.xz
    ```
    * バージョンやターゲットアーキテクチャに応じてURLを変更する。
* tarアーカイブの抽出
    ```
    xzcat WebsocketGaugeServer-4.0-RC1-linux-arm64.tar.xz | tar xvf -
    ```
* ディレクトリに移動する
    ```
    cd WebsocketGaugeServer-4.0-RC1-linux-arm64
    ```
* (オプション) 設定ファイルを編集します。
    * 設定を変更する場合は、`appsettings.json`を編集してください。
    * 編集する前に、`vim`のようなエディタをインストールしてください。
    ```
    apt install vim
    vi appsettings.json
    ```
* 最後に、`WebSocketServer`を起動します。
    ```
    ./WebSocketServer
    ```
* Androidの通知にて、termuxの`ACQUIRE LOCK`を取得する方がよいかもしれません (Androidが強制的にTermuxを終了させないように)
* 起動後は http://localhost:2016 にてフロントページにアクセス可能です。

## serial-TCP ラッパーアプリのインストール
* シリアル通信をTCPに変換するために、serial-TCP wrapperアプリが必要な場合があります。
* [Bluetooth Bridge (+TCP)](https://play.google.com/store/apps/details?id=masar.bb) は、Bluetooth elm327 アダプタで動作確認済みです。
    * 標準のポート番号は35000です。