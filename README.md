# TFTP Server

## Features

Managed TFTP server implementation with the following features:

* Open source
* works both in IPv4 and IPv6 mode.
* correct retry behavior
* supports the following TFTP options: block size, transfer size, and timeout.
* it's possible to run in single port mode: this ensures that only port 69 will be used for TFTP transfers, simplifying firewall and router configuration.
* supports many concurrent transfers. 
* as of 1.2 beta : supports windowed mode, enabling high speed downloads.
* Runs as windows service.
* Supports multiple TFTP servers on different endpoints, each serving its own root directory.
* Advanced MSI based installer.
* Logs to the windows event log.
* permissive MIT license so it can be used in commercial projects.

## Screenshots

See [Screenshots](docs/Screenshots.md).

## Tested clients

The server has been successfully tested with the following clients:
* my multiplatform graphical desktop TFTP client: [Avalonia-TFTPClient](https://github.com/jpmikkers/Avalonia-TFTPClient)
* my TFTP client library [TFTPClient](https://github.com/jpmikkers/TFTPClient)
* windows 7 TFTP
* Win32: [TFTPD32](http://tftpd32.jounin.net/) by Philippe Jounin
* ubuntu/linux Advanced TFTP client, [ATFTP](http://www.ubuntugeek.com/howto-setup-advanced-tftp-server-in-ubuntu.html)
* ubuntu/linux TFTP
* ubuntu/linux TFTP-hpa
* [B&R](http://www.br-automation.com/) PLC bootloader

## Tested platforms (so far)

* 64 bit Windows 11
* 64 bit Windows 10
* 64 bit Windows 7
* 32 bit Windows XP
* 64 bit Windows Vista
* Linux! The main TFTP library compiles and runs without modifications in [Mono](http://www.mono-project.com/) . All I had to do was create a small console app that instantiates the TFTPServer class.
