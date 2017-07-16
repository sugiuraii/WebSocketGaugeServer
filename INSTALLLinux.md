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

## <a name="setupSupervisor"> Setup supervisor </a>

