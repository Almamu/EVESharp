evedec
======
Reads and decrypts Eve Online python files and passes them to uncompyle2 to decompile.
This is a modified version of https://github.com/wibiti/evedec to work with Apocrypha
* Doesn't manipulate Eve process. Can be run with or without Eve running.
* Searches for decryption key in the blue.dll file.
* Requires uncompyle2 for actual decompilation.
* Uses multiple processes to speed up decompilation.

The uncompyle2 version required is included in the uncompyle2 folder. No modification should be needed, it should load the custom version right away. 

Expects a evedec.ini file to specify Eve install location and output directory, e.g.:
```
[main]
eve_path = C:\Program Files (x86)\CCP\EVE\
store_path = output\
```

It only works on Windows
