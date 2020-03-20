# Configuration options for Node
The configuration file for Node has to be named "configuration.conf" and must always be stored in the same directory as the Node.

This file follows the commonly-used ini format which separates the configuration in named sections and keys. [For more information on the .ini format click here](https://en.wikipedia.org/wiki/INI_file)

## proxy
Configuration of the proxy server to connect the node to. This should point to the ClusterController running.

```
[proxy]
hostname=localhost
port=26000
```

### hostname
The host where the ClusterController is running
### port
The port to connect to, 26000 by default

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
logfile=Node.log
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

## authentication
This section controls various authentication details.

```
[authentication]
loginMessageType=MESSAGE
loginMessage=Welcome to EVESharp, the EVE Online server emulator written in C#
```

### loginMessageType
Indicates whether the user will receive a message or not upon login. Possible values are: ```MESSAGE``` and ```NONE```
### loginMessage
Indicates the message to be displayed to the user when logged in. The value can be any kind of HTML or plaintext text, but be wary, no new lines are supported