# WiiUGamepadToMouse-MOD

A fork of WiiUGamepadToMouse by FIX94 with a few new features.

Usage
-----

To use this tool, you will need [MiisendU](https://github.com/Crayon2000/MiisendU-Wii-U) by Crayon2000 on your Wii U.

With the client running on your Wii U, enter the IP address of your computer and press A. On your computer, run WiiUGamepadToMouse.exe and configure the settings to the way you wish and click Start Server.

Controls: Press A, ZL, or ZR to left-click. Press B, L, or R to right-click. Press L3 or R3 to middle-click. Move L-stick and R-stick to scroll. Press X to toggle Trackpad mode. Press Y to toggle touch-to-click mode.

If a program is in the foreground which is running with administrator permissions, you will not be able to control the mouse using this tool unless it is also running with elevated permissions. You can do that by simply running the tool as an administrator. However, you will not be able to accept UAC dialogs using this tool for obvious reasons. Please don't open an issue because of it.

Modifications
-------------

This mod adds a trackpad mode, fixes the aspect ratio of the touchscreen, and allows for the scale to be adjusted independently.

Requirements
------------

You will need the .NET framework installed otherwise the app will not start. You can install it [here](https://dotnet.microsoft.com/en-us/download/dotnet-framework). Any version higher than 4.5 should work.

For compiling, I recommend Visual Studio Community 2019. You should be able to open either the .sln or .csproj file and compile it.
