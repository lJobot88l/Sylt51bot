# Sylt51bot
Sylt51bot is a small bot, made originally just for the [Sylt51 discord server](https://discord.gg/Sylt51 "Sylt51 discord invite") with .NET Core 3.1.

It implements a discord version of r/ich_iel's "Känguru_Rechenknecht" and also has a fairly modular levelling system.

The bot uses the [DSharpPlus and DSharpPlus.CommandsNext](https://github.com/DSharpPlus/DSharpPlus "GitHub D#+") libraries for interaction with the Discord API.
***
## Installation
### 1. Building it yourself
- Download or Clone the repository
+ Create a file `mconfig.json` in `config/`
- Enter the necessary information in the newly created .json as specified in the readme.txt in that folder
	- (The only required information is your applications token, the channel to send error messages and heartbeats in, and one or more prefixes)
---
### 2. Using the release
+ Go to the [latest version](https://github.com/lJobot88l/Sylt51bot/releases/latest "latest release") in the "Releases" list
- Select and Download the version for your runtime (e.G `Sylt51-[version]-linux_arm.zip`, `Sylt51-[version]-win_x86.zip`)
+ Unzip the downloaded file
- Create a file `mconfig.json` in `./config/`
+ Enter the necessary information in the newly created .json as specified in the readme.txt in that folder
	+ (The only required information is your applications token, the channel to send error messages and heartbeats in, and one or more prefixes)