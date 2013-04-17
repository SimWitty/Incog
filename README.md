incog
======

Steganography and Covert Channels Library written in C#. The library is delivered as a PowerShell 3.0 module.

Download release files from:
https://github.com/SimWitty/Incog/tree/master/Release

What it is
==========

Incog provides a means to test for detecting and preventing covert channels. It can be used as a fire drill to evaluate an infrastructure's controls and a team's preparedness, without introducing malicious software into the environment.

What it is not
==============

Incog is not designed to be used for a penetration test or for facilitating a security breach. It does not evade anti-virus software, or otherwise conceal its actions. #incog is strictly meant for opening covert channels and testing defenses against said channels.

Features
========

// Channel commands -- Covert Channels and Communications
- IncogLookup - DNS
- IncogNamedPipe - Windows DCE/RPC
- IncogPing - ICMP
- IncogWebServer - HTTP
- Netcat - Any TCP or UDP port

// Media commands -- Steganography and Files
- IncogFileSystem - Alternate Data Streams
- IncogImage - Bitmap with Least Significant Bit (LSB)
- IncogMutex - Memory Mapped Files / Mutex
- IncogWebPage - HTML

[![githalytics.com alpha](https://cruel-carlota.pagodabox.com/299f7cdbf2cc7f8abcee60a8bca8a270 "githalytics.com")](http://githalytics.com/SimWitty/Incog)
