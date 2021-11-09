# Configuration options for Proxy
The configuration file for Proxy has to be named "configuration.conf" and must always be stored in the same directory as the Proxy.

This file follows the commonly-used ini format which separates the configuration in named sections and keys. [For more information on the .ini format click here](https://en.wikipedia.org/wiki/INI_file)

## database
Configuration of the database connection. This section is required for the server to startup.

```
[database]
username=evesharp
password=passwordhere
hostname=localhost
name=evedb
```

### username
The user to use when connecting to the database
### password
The password to use when connection to the database
### database
The name of the database to use for storing the data. This database has to be setup manually following the installation steps.
### hostname
The server where the MySQL instance is running.

## logging
### force
Indicates the log channels that should be enabled regardless of the supression state they are in (for example for debugging network packets). The list is separated by commas. This is intended for developers only.

```
[logging]
force=NetworkDebug,Client
```

Right now only NetworkDebug is suppressed by default, but more might come

## logfile
Configuration for the file log output. If this section is not present there will be no log file created for the session.

```
[logfile]
directory=logs
logfile=Proxy.log
```

### directory
The directory where to save the log files.

### logfile
The name of the log file to write the loggin information to.

## loglite
LogLite is an official CCP tool that allows external logging. The protocol is implemented to allow for an easier log inspection and can be run on any machine as long as the server is configured properly to connect to it. This section contains the server information the log should be sent to.

```
[loglite]
hostname=localhost
port=3273
```

### hostname
The hostname of the loglite server is running
### port
The port in which the loglite server is running

## autoaccount
Configuration for the autoaccount mechanism. If a non-existant user tries to login the account will be automatically created for the user and login performed. This section controls this behaviour.

```
[autoaccount]
enabled=true
role=ROLE_PLAYER, ROLE_LOGIN, ROLE_ADMIN, ROLE_QA, ROLE_SPAWN, 
```
### enabled
Indicates if the auto-account mechanism should be enabled or not. Possible values are: ```yes```, ```1```, ```true```.
### role
Indicates the role for the accounts created by the autoaccount system. This key must be specified if the autoaccount is enabled. Possible values are:
```
ROLE_LOGIN
ROLE_PLAYER
ROLE_GDNPC
ROLE_GML
ROLE_GMH
ROLE_ADMIN
ROLE_SERVICE
ROLE_HTTP
ROLE_PETITIONEE
ROLE_GDL
ROLE_GDH
ROLE_CENTURION
ROLE_WORLDMOD
ROLE_QA
ROLE_EBS
ROLE_ROLEADMIN
ROLE_PROGRAMMER
ROLE_REMOTESERVICE
ROLE_LEGIONEER
ROLE_TRANSLATION
ROLE_CHTINVISIBLE
ROLE_CHTADMINISTRATOR
ROLE_HEALSELF
ROLE_HEALOTHERS
ROLE_NEWSREPORTER
ROLE_HOSTING
ROLE_BROADCAST
ROLE_TRANSLATIONADMIN
ROLE_N00BIE
ROLE_ACCOUNTMANAGEMENT
ROLE_DUNGEONMASTER
ROLE_IGB
ROLE_TRANSLATIONEDITOR
ROLE_SPAWN
ROLE_VIPLOGIN
ROLE_TRANSLATIONTESTER
ROLE_REACTIVATIONCAMPAIGN
ROLE_TRANSFER
ROLE_GMS
ROLE_EVEONLINE
ROLE_CR
ROLE_CM
ROLE_MARKET
ROLE_MARKETH
ROLE_ANY
ROLEMASK_ELEVATEDPLAYER
ROLEMASK_VIEW
```

Multiple roles can be specified separating them by commas.

## listening
Configuration for how the Proxy listens for new node and/or client connections.

### port
Sets the port the Proxy will listen on for new clients and nodes connections. By default this will be setup to port 26000 (the default port). This is specially useful for using [EVEmu Live Packet Editor](https://github.com/Almamu/EVEmu-live-packet-editor) to debug client <-> server communications.

```
[listening]
port=26000
```