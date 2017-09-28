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

### Server Related

#### Retrieve server status

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

### Tasks Related
#### Start the task 
This command will wait until task is done. 

To start the task you need to know task space name and task ID. 
Make sure to check the task execution log at server to determine task execution status.
```
ems-cmd run http://192.168.100.200:6330 -space Default -taskID 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-taskID` - task guid.

Task guid can be found in the browser location toolbar. E.g, if you have clicked on the edit task link, your browser location seems to be  `http://localhost:6330/default/tasks/edit/59b824f0-4b81-453f-b9e0-1c58b97c9fb9`, where `59b824f0-4b81-453f-b9e0-1c58b97c9fb9` - is a task guid

###### Output
```
Attempting to start task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
Project 'sample.morph' is running. Waiting until done.

Task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9 completed
```

#### Start the task asynchronously (fire and forget)
This command return control immediately after task was enqueued 


To start the task you need to know task space name and task ID. 
Make sure to check the task execution log at server to determine task execution status.
```
ems-cmd runasync http://192.168.100.200:6330 -space Default -taskID 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-taskID` - task guid.

Task guid can be found in the browser location toolbar. E.g, if you have clicked on the edit task link, your browser location seems to be  `http://localhost:6330/default/tasks/edit/59b824f0-4b81-453f-b9e0-1c58b97c9fb9`, where `59b824f0-4b81-453f-b9e0-1c58b97c9fb9` - is a task guid

###### Output
```
Attempting to start task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
Project 'sample.morph' is running.
```



### Files Related
#### Download the file
This command will download one single file from server

If local file already exists, you will be prompted to overwrite it. 
Notice, that when you are using a redirected output (e.g. to the file) and local file already exists, download will fail. 
In any case, you may use parameter '/y' to overwrite existing file without any prompts.

```
ems-cmd download http://192.168.100.200:6330 -space Default -to "D:\your\local\folder" -from "server\folder\file.xml" 
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-to` - destination folder (you local folder).
* `-from` - relative path to file in the space `-space`
* '/y' - overwrite existing file (silent agree)

###### Output
```
Downloading file 'server\folder\file.xml' from space 'Default' into 'D:\your\local\folder'...
Operation completed
```


#### Upload the file
This command will upload one single to server

If remote file already exists, you will be prompted to overwrite it. 
Notice, that when you are using a redirected output (e.g. to the file) and remote file already exists, upload will fail. 
In any case, you may use parameter '/y' to overwrite existing file without any prompts.

```
ems-cmd upload http://192.168.100.200:6330 -space Default -from "D:\your\local\folder\file.xml" -to "\" 
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-to` - destination folder (remote folder, relative path in the space `-space`).
* `-from` - path to your local file
* '/y' - overwrite existing file (silent agree)

###### Output
```
Uploading file 'D:\your\local\folder\file.xml' to folder '\' of space 'Default'...
Operation completed
```




