## Introduction

EasyMorph Server Command Line Client (in further text – **ems-cmd**) allows you to run server commands via REST API.


### Requirements
1. OS Windows with .Net 4.5 or later
2. Morph.Server.Sdk.dll (deployed together with ems-cmd).  


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
This command will start specified task and wait until it is done. 

To start the task you need to know space name and the task ID. 
Make sure to check the task execution server log to determine task execution info.
```
ems-cmd run http://192.168.100.200:6330 -space Default -taskID 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-taskID` - task guid.

Task guid can be found in the browser location toolbar. E.g, if you have clicked on the edit task link, your browser location seems to be  `http://localhost:6330/default/tasks/edit/59b824f0-4b81-453f-b9e0-1c58b97c9fb9`, where `59b824f0-4b81-453f-b9e0-1c58b97c9fb9` - is a desired value

###### Output
```
Attempting to start task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
Project 'sample.morph' is running. Waiting until done.

Task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9 completed
```

#### Start task asynchronously (fire and forget)
This command return control immediately after task was enqueued 


To start the task you need to know task space name and task ID. 
Make sure to check the task execution log at server to determine task execution status.
```
ems-cmd runasync http://192.168.100.200:6330 -space Default -taskID 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-taskID` - task guid.

Task guid can be found in the browser location toolbar. E.g, if you have clicked on the edit task link, your browser location seems to be  `http://localhost:6330/default/tasks/edit/59b824f0-4b81-453f-b9e0-1c58b97c9fb9`, where `59b824f0-4b81-453f-b9e0-1c58b97c9fb9` - is a desired value

###### Output
```
Attempting to start task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
Project 'sample.morph' is running.
```



### Files Related

#### Browsing files
This command shows folder content

```
ems-cmd browse http://192.168.100.200:6330 -space Default -location "\folder 2"
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-location` - remote folder(relative path in the space `-space`).


###### Output
```
Browsing the folder 'folder 2' of the space Default
Space: Default
Free space: 7820345344 bytes
09/05/2017 09:39:22 PM        <DIR>            Folder 2.1
09/20/2017 09:45:33 AM        <DIR>            Folder 3
09/28/2017 03:44:16 PM                   8,123 project.morph
Listing done
```


#### Download file
This command will download one single file from server

If local file already exists, you will be prompted to overwrite it. 
Notice, that when you are using a redirected output (e.g. to the file) and local file already exists, download will fail. 
In any case, you may use parameter `/y` to overwrite existing file without any prompts.


Be careful with folders that contain spaces in their names. You should add quotation marks around such parameter values. 
Keep in mind, that sequence  `\"` will escape double quotes. So `"D:\"`, `"D:\another folder\"` are incorrect, use `D:\` and `"D:\another folder"` instead.

```
ems-cmd download http://192.168.100.200:6330 -space Default -destination D:\your\local\folder -source file.xml 
ems-cmd download http://192.168.100.200:6330 -space Default -destination D:\your\local\folder -source \file.xml
ems-cmd download http://192.168.100.200:6330 -space Default -destination D:\your\local\folder -source \server\folder\file.xml
ems-cmd download http://192.168.100.200:6330 -space Default -destination "D:\local\folder with spaces" -source "server\folder with spaces\file3.xml" 
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-destination` - destination folder (you local folder).
* `-source` - relative path to file in the space `-space`
* `/y` - overwrite existing file (silent agree)

###### Output
```
Downloading file 'server\folder\file.xml' from space 'Default' into 'D:\your\local\folder'...
Operation completed
```


#### Upload file
This command will upload one single to server

If remote file already exists, you will be prompted to overwrite it. 
Notice, that when you are using a redirected output (e.g. to the file) and remote file already exists, upload will fail. 
In any case, you may use parameter `/y` to overwrite existing file without any prompts.


Be careful with folders that contain spaces in their names. You should add quotation marks around such parameter values. 
Keep in mind, that sequence  `\"` will escape double quotes. So `"D:\"`, `"D:\another folder\"` are incorrect, use `D:\` and `"D:\another folder"` instead.


```
ems-cmd upload http://192.168.100.200:6330 -space Default -source D:\your\local\folder\file.xml -destination \
ems-cmd upload http://192.168.100.200:6330 -space Default -source D:\your\local\folder\file2.xml -destination "folder 2"
ems-cmd upload http://192.168.100.200:6330 -space Default -source D:\your\local\folder\file2.xml -destination "folder 2\sub folder"
ems-cmd upload http://192.168.100.200:6330 -space Default -source "D:\local\folder with spaces" -destination "folder 2\sub folder"
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-destination` - destination folder (remote folder, relative path in the space `-space`).
* `-source` - path to your local file
* `/y` - overwrite existing file (silent agree)

###### Output
```
Uploading file 'D:\your\local\folder\file.xml' to folder '\' of space 'Default'...
Operation completed
```


#### File deletion
This command will delete remote file

```
ems-cmd del http://192.168.100.200:6330 -space Default -file "folder 2\file.xml" 
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-file` - relative path in the space `-space` to the file

## License 

**ems-cmd** is licensed under the [MIT license](https://github.com/easymorph/server-cmd/blob/master/LICENSE).



