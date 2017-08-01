DefiSSMCOM_WebsocketServer linux install manual
---

## Description
The DefiSSMCOM_WebsocketServer is developed on .net. This program can run on linux thanks to [mono](http://www.mono-project.com/). 

To use this program in carputer (computer in the car), this program may preffered to run as daemon.
Currently, this program itself is not desighed to run as daemon. However, it is possible to use this program as a daemon with using [Supervisor](http://supervisord.org/).

This document explains how to install DefiSSMCOM_WebsocketServer into linux(debian-based) PC, and how to setup with supervisor.

## Table of contents
* [Add new user to run the program](#addNewUser)
* [Install mono-devel](#installMono)
* [Install supervisor](#installSupervisor)
* [Install(copy) this program](#installThisProgram)
* [Setup this program](#setupThisProgram)
* [Setup supervisor](#setupSupervisor)

## <a name="addNewUser">Add new user to run the program</a>
Add new user to run the program. 

Since this program access to serial ports, this new user have to belong to "dialout' group (otherwise, access to the serial port will be denied).

In this example, new user of "wscomm" is added. The adduser command may ask you Full Name, phone number etc.., you don't have to input anything.

```
> sudo adduser --ingroup dialout --disabled-login wscomm
```

## <a name="installMono">Install mono-devel </a>

This program needs mono-devel package. (This program cannot run on mono-runtime. mono-devel package is required.)
```
> sudo apt-get install mono-devel
```

## <a name="installSupervisor">Install supervisor </a>

Install supervisor to run this program as daemon.
```
> sudo apt-get install supervisor
```

## <a name="installThisProgram">Install(copy) this program </a>
Install this program. Currently this program have no installer, so please unzip your favorite directory.

In this example, unzip the program into wscomm's (the user you added above) home directory.

(Before doing this, copy the binary archive to /tmp)
Change current user to wscomm
```
> sudo su wscomm
> cd /home/wscomm
```
And unzip the program from archive.
```
> zcat /tmp/DefiSSMCOM_Websocket_1.0.tar.gz | tar xvf -
> exit
```
After this, new directory of "WebsocketServer" is created.

## <a name=setupThisProgram>Setup this program</a>
The settings of the programs are written on xml files such as,
* Defi/defiserver_settings.xml
* SSM/ssmserver_settings.xml
* Arduino/arduinoserver_settings.xml
* ELM327/elm327server_settings.xml

At least, <comport> tag of these files need to be modified. In linux, you can set the comport name such as,
```
<comport>/dev/ttyUSB0</comport>
```
If you need, you can change the websocket portname by `<websocket_port>` tag (Nomally, you don't have to).

As for the Arduino and ELM327, you can change the baudrate by `<baudrate>` tag. (Especially for ELM327,) you have to match the baudrate of your ELM327/arduino adaptor and the `<baudrate>` tag.

## <a name="setupSupervisor"> Setup supervisor </a>
Add configuration file for supervisor. In the case of Debian based distribution, config file directory is `/etc/supervisor/conf.d/`.

Move to `/etc/supervisor/conf.d`, and create arduino_websocket.conf (in the case of Arduino).
```
cd /etc/supervisor/conf.d
sudo vi arduino_websocket.conf
```

And make [arduino_websocket.conf](./LinuxInstall.SettingFiles/arduino_websocket.conf) as follow,
```
[program:arduino_websocket]
command=mono ArduinoCOM_WebSocket_Server.exe
user=wscomm
directory=/home/wscomm/WebsocketServer/Arduino
autorestart=true
stdout_logfile=/var/log/supervisor/arduino_websocket.log
stdout_logfile_maxbytes=1MB
stdout_logfile_backups=10
stdout_capture_maxbytes=1MB
redirect_stderr=true
```
Please note that `wscomm` is the username you created on for the program, the `directory` is the directory where you extracted the program binary exe file.

And create [defi_websocket.conf](./LinuxInstall.SettingFiles/defi_websocket.conf), [ssm_websocket.conf](./LinuxInstall.SettingFiles/ssm_websocket.conf), [elm327_websocket.conf](./LinuxInstall.SettingFiles/elm327_websocket.conf) in the similar way as this.
Of cause, you can skip making these files if you do not have plan to use these programs.

`defi_websocket.conf`
```
[program:defi_websocket]
command=mono DefiCOM_WebSocket_Server.exe
user=wscomm
directory=/home/wscomm/WebsocketServer/Defi
autorestart=true
stdout_logfile=/var/log/supervisor/defi_websocket.log
stdout_logfile_maxbytes=1MB
stdout_logfile_backups=10
stdout_capture_maxbytes=1MB
redirect_stderr=true
```
`ssm_websocket.conf`
```
[program:ssm_websocket]
command=mono SSMCOM_WebSocket_Server.exe
user=wscomm
directory=/home/wscomm/WebsocketServer/SSM
autorestart=true
stdout_logfile=/var/log/supervisor/ssm_websocket.log
stdout_logfile_maxbytes=1MB
stdout_logfile_backups=10
stdout_capture_maxbytes=1MB
redirect_stderr=true
```
`elm327_websocket.conf`
```
[program:elm327_websocket]
command=mono ELM327COM_WebSocket_Server.exe
user=wscomm
directory=/home/wscomm/WebsocketServer/ELM327
autorestart=true
stdout_logfile=/var/log/supervisor/elm327_websocket.log
stdout_logfile_maxbytes=1MB
stdout_logfile_backups=10
stdout_capture_maxbytes=1MB
redirect_stderr=true
```

After making these conf files, you can start these websocket server program by rebooting linux or restating supervisor.
```
> sudo service supervisor restart
```
You can check the program output (errors or warning messages) by log file in `/var/log/supervisor/`.
