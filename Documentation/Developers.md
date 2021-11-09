In this page we'll document different techniques and debug options for the game and the server.

# Decompiling EVE Online Source Code
For info on this check [evedec](/Tools/evedec)

# Recomended roles for developer accounts
The following roles are ideal for developers as these allow for running code on the client, using commands and some other tools embedded into the client:
```
ROLE_PLAYER, ROLE_LOGIN, ROLE_ADMIN, ROLE_QA, ROLE_SPAWN, ROLE_GML, ROLE_GDL, ROLE_GDH, ROLE_HOSTING, ROLE_PROGRAMMER
```
These can be used as is in the autoaccount configuration.

# Debugging network traffic
Both Proxy and Node allow to setup network logging in the settings file. Check the "logging" on both project's documentation.

Another way of debugging network traffic is using [EVEmu Live Packet Editor](https://github.com/Almamu/EVEmu-live-packet-editor), which gives some advanced filtering options for the data being sent and received.