## Introduction

EasyMorph Server Command Line Client (in further text – **ems-cmd**) allows you to run server commands via REST API.


#### Requirements
1. OS Windows x86/x64  with .Net 4.5 or later
2. Morph.Server.Sdk.dll (deployed together with ems-cmd). Also [hosted on github](https://github.com/easymorph/server-sdk)  
3. [EasyMorph Server 1.2](http://easymorph.com/server.html) or higher (installed on a separate machine)


#### Download
ems-cmd comes together with EasyMorph Server. Also it can be [downloaded](https://github.com/easymorph/server-cmd/releases) separately 
 

#### General command format:

```
ems-cmd <commad> <host> -param1 value1 -param2 "value two"
```


where

+ **```<command>```** - command to execute. See [Commands](#commands) section for details.
+  **```<host>```** - Server host like `http://192.168.100.200:6330`.
  

#### exit codes
ems-cmd  may return one of the following exit codes:
* `0` ems-cmd was successfully run to completion.
* `1` A fatal error occurred during command parsing or execution.

#### Authorization  
For password protected spaces you should pass password via command parameter `-password`.
```
ems-cmd upload http://192.168.100.200:6330  -space Default -password your_password -source D:\your\local\folder\file.xml -target \
```
In the example above, a session will be opened for the specified space Default, of course if password was correct. In case of incorrect password or public spaces, error will be thrown.
Some hash computations are applied to the password before it is sent to the server. 


### Commands

### Status and metadata

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
ServerVersion:1.3.0.0
```


#### List spaces
A list of all spaces will be displayed. This command doesn't require authorization.

```bash
ems-cmd listspaces http://192.168.100.200:6330 
```
###### Parameters
This command has no additional parameters

###### Output
```
Available spaces:
* closed one
  Default
Listing done
```
Asterisk `*` means that the space requires an authorization.


#### Space status
Returns specified space status. This command may require authorization if space is password protected.

```bash
ems-cmd spacestatus http://192.168.100.200:6330 -space "closed one" -password some_password
```
###### Parameters

* `-space` - space name.
* `-password` - if password is required.

###### Output
```
Checking space default status...
Space: Default
IsPublic: True
Permissions: FilesList, FileDownload
done
```



### Tasks
#### List tasks
This command will list all tasks in the space.
```
ems-cmd listtasks http://192.168.100.200:6330 -space Default
```
###### Parameters
* `-space` - space name, e.g. `Default`

###### Output
```
Listing tasks in the space default
0c73012f-0704-44a2-a3f5-ae54df3edd05: wait
235cc252-2eb2-4b6c-b6b4-7b92eabf5494: f
f69efa25-b649-47d5-a5ff-3772358a8aec: wait
Listing done
```

#### Get task metadata
Allows obtaining metadata about a task and its settings.

```
ems-cmd gettask  http://192.168.100.200:6330 -space Default -taskID 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-taskID` - task guid.


###### Output
```
Attempting to get task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9 in space 'default'
Info about task:
Id:'59b824f0-4b81-453f-b9e0-1c58b97c9fb9'
Name:'f'
IsRunning:'False'
Enabled:'True'
Note:''
ProjectPath:'C:\Users\Public\Documents\Morphs\f.morph'
StatusText:'Idle'
TaskState:'Idle'
Task Parameters:
Parameter 'ZText' = 'some text' [Text] (Note: ZText annotation)
Parameter 'ZDate' = '2000-01-15' [Date] (Note: ZDate annotation)
Parameter 'ZFile' = 'C:\Users\Public\Documents\Morphs\file.csv'
[FilePath] (Note: ZFile annotation)
Done
```



#### Start a task synchronously
This command will start the specified task and wait until it is finished. 

To start the task you need to know the space name and the task ID. 
See the task execution server log to determine task execution info.


```
ems-cmd run http://192.168.100.200:6330 -space Default -taskID 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-taskID` - task guid.
* `-param:XXX ZZ` - set task parameter `XXX` with value `ZZ`.

Task guid can be found in the browser location toolbar. E.g, if you have clicked on the edit task link, your browser location seems to be  `http://localhost:6330/default/tasks/edit/59b824f0-4b81-453f-b9e0-1c58b97c9fb9`, where `59b824f0-4b81-453f-b9e0-1c58b97c9fb9` - is a desired value

If you want to pass (or override) parameters that were defined in  morph project, add `-param:XXX ZZ` to ems-cmd execution line. 
Where `XXX`  is a parameter name and `ZZ` is a parameter value. 
At least one space between  parameter name and parameter value is required.


E.g. If you've defined parameter `Timeout` in your morph project, and want to set it to 73 use `-param:Timeout 73`. Pay attention, that parameters are case sensitive.


Examples:


Set parameter `Rounds` to `10` :   `-param:Timeout 73`

Set parameter `Full name` to `John Smith` :   `-param:"Full name" "John Smith"`

Set parameter `From Date` to  the `10th of December 2000` :   `-param:"From Date" "2000-12-10"`   (ISO 8601 date format)


###### Output
```
Attempting to start task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
Project 'sample.morph' is running. Waiting until done.

Task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9 completed
```

#### Start a task asynchronously (fire and forget)
This command return control immediately after task was enqueued 


To start the task you need to know task space name and task ID. 
Make sure to check the task execution log at server to determine task execution status.
```
ems-cmd runasync http://192.168.100.200:6330 -space Default -taskID 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-taskID` - task guid.
* `-param:XXX ZZ` - set task parameter `XXX` with value `ZZ`.

Task guid can be found in the browser location toolbar. E.g, if you have clicked on the edit task link, your browser location seems to be  `http://localhost:6330/default/tasks/edit/59b824f0-4b81-453f-b9e0-1c58b97c9fb9`, where `59b824f0-4b81-453f-b9e0-1c58b97c9fb9` - is a desired value

If you want to pass (or override) parameters that were defined in  morph project, add `-param:XXX ZZ` to ems-cmd execution line. 
Where `XXX`  is a parameter name and `ZZ` is a parameter value. 
At least one space between  parameter name and parameter value is required.


E.g. If you've defined parameter `Timeout` in your morph project, and want to set it to 73 use `-param:Timeout 73`. Pay attention, that parameters are case sensitive.


Examples:


Set parameter `Rounds` to `10` :   `-param:Timeout 73`

Set parameter `Full name` to `John Smith` :   `-param:"Full name" "John Smith"`

Set parameter `From Date` to  the `10th of December 2000` :   `-param:"From Date" "2000-12-10"`   (ISO 8601 date format)


###### Output
```
Attempting to start task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
Project 'sample.morph' is running.
```



### Files

#### Browse files
This command shows the contents of a folder.

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


#### Download a file
This command will download one single file from server.

If local file already exists, you will be prompted to overwrite it. 
Notice, that when you are using a redirected output (e.g. to the file) and local file already exists, download will fail. 
In any case, you may use parameter `/y` to overwrite existing file without any prompts.


Be careful with folders that contain spaces in their names. You should add quotation marks around such parameter values. 
Keep in mind, that sequence  `\"` will escape double quotes. So don't use it at the end of the parameter value.

```
ems-cmd download http://192.168.100.200:6330 -space Default -target D:\your\local\folder -source file.xml 
ems-cmd download http://192.168.100.200:6330 -space Default -target D:\your\local\folder -source \file.xml
ems-cmd download http://192.168.100.200:6330 -space Default -target D:\your\local\folder -source \server\folder\file.xml
ems-cmd download http://192.168.100.200:6330 -space Default -target "D:\local\folder with spaces" -source "server\folder with spaces\file3.xml" 
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-target` - destination folder (you local folder).
* `-source` - relative path to file in the space `-space`
* `/y` - overwrite existing file (silent agree)

###### Output
```
Downloading file 'server\folder\file.xml' from space 'Default' into 'D:\your\local\folder'...
Operation completed
```


#### Upload a file
This command will upload one single to Server.

If remote file already exists, you will be prompted to overwrite it. 
Notice, that when you are using a redirected output (e.g. to the file) and remote file already exists, upload will fail. 
In any case, you may use parameter `/y` to overwrite existing file without any prompts.


Be careful with folders that contain spaces in their names. You should add quotation marks around such parameter values. 
Keep in mind, that sequence  `\"` will escape double quotes. So don't use it at the end of the parameter value.


```
ems-cmd upload http://192.168.100.200:6330 -space Default -source D:\your\local\folder\file.xml -target \
ems-cmd upload http://192.168.100.200:6330 -space Default -source D:\your\local\folder\file2.xml -target "folder 2"
ems-cmd upload http://192.168.100.200:6330 -space Default -source D:\your\local\folder\file2.xml -target "folder 2\sub folder"
ems-cmd upload http://192.168.100.200:6330 -space Default -source "D:\local\folder with spaces" -target "folder 2\sub folder"
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-target` - destination folder (remote folder, relative path in the space `-space`).
* `-source` - path to your local file
* `/y` - overwrite existing file (silent agree)

###### Output
```
Uploading file 'D:\your\local\folder\file.xml' to folder '\' of space 'Default'...
Operation completed
```


#### Delete file
This command will delete a remote file.

```
ems-cmd del http://192.168.100.200:6330 -space Default -target "folder 2\file.xml" 
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-target` - relative path in the space `-space` to file 


### SSL errors
In case if you want to suppress ssl errors,  use additional parameter `/suppress-ssl-errors`.
```
ems-cmd del http://192.168.100.200:6330 -space Default -target "folder 2\file.xml" /suppress-ssl-errors 
```

## License 

**ems-cmd** is licensed under the [MIT license](https://github.com/easymorph/server-cmd/blob/master/LICENSE).



