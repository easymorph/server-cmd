### Introduction

EasyMorph Server Command Line Client (in further text – **ems-cmd**) allows you to run server command via REST API.


### Requirements
1. OS Windows with installed .Net 4.5 or later
2. Morph.Server.Sdk.dll (deployed with ems-cmd).  


General command format:

```
ems-cmd <commad> <host> -param1 value1 -param2 "value two"
```


where

+ **```<command>```** - command to execute. See [Commands](#commands) section for details.
+  **```<host>```** - Server host like `http://192.168.100.200:6330`.
  


### Commands

#### Server Related

##### Retrieve server status

```bash
ems-cmd status http://192.168.100.200:6330
```
###### Parameters
This command has no additional parameters

###### Output
```
Retrieving server status...
STATUS:
StatusCode: OK
StatusMessage: Server is OK
ServerVersion:1.2.0.0
```

#### Tasks Related
##### Start the task 
To start the task you need to know task space name and task ID. 
```
ems-cmd run http://192.168.100.200:6330 -space Default -taskID 8de5b50a-2d65-44f4-9e86-660c2408fb06
```




