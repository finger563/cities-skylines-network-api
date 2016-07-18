# cities-skylines-network-api

Mod for Cities: Sylines which exposes its API through the network for
other programs and computers to interact with and control the
simulation.

## Requirements

You must have Cities:Skylines installed to use this mod. Will flesh
out this section later

## Setup

Will flesh out this section later.

## Building

Within Visual Studio 2015, you should make sure the Cities:Skylines /
Unity references point to the correct (installed) locations (within
`Steam/steamapps/common/Cities_Skylines/Cities_Data/Managed`).

## Installation

Once the dll has been built, VS will automatically (configured by the
project settings) make the relevant folder in `%LOCALAPPDATA%/Colossal
Order/Cities_Skylines/Addons/Mods/NetworkAPI` and copy the
NetworkAPI.dll there.  Other DLLs will need to be copied to there
manually.  Those dlls will be listed later (and perhaps automated).

## Running

Make sure to enable the mod within Cities: Skylines.

By default there is a UDP server listening on `localhost:11000`.

It accepts JSON-formatted data (example in [udp.js](./Client/udp.js)).