## Introduction
The EasyMorph Server Command Line Client (further – **ems-cmd**) is an open-source cross-platform utility that allows performing  from the command line various operations with tasks, files, and other features of EasyMorph Server (further - just Server). Under the hood, the utility uses the open-source [EasyMorph Server .NET SDK](https://github.com/easymorph/server-sdk) to communicate with EasyMorph Server via the REST API and can be used as a self-explanatory example of using the SDK.

### Requirements
**Compatible OS**

OS Windows x86/x64  with .Net 4.7.2 or later.

OS Windows/ Linux / MacOS with net 6.0 or higher (net6.0).

**EasyMorh Server**

Morph.Server.Sdk.dll (deployed together with ems-cmd). Also [hosted on github](https://github.com/easymorph/server-sdk)  

[EasyMorph Server 5.0](http://easymorph.com/server.html) or higher.


### Download
ems-cmd comes together with EasyMorph Server. Also, it can be [downloaded](https://github.com/easymorph/server-cmd/releases) separately.
 

### General syntax

```
ems-cmd <command> <host> -param1 value1 -param2 "value two"
```

where

+ **```<command>```** - a command to execute. See the [Commands](#commands) section for details.
+  **```<host>```** - the Server host, for instance: `http://192.168.100.200:6330`.

#### Exit codes
ems-cmd  may return one of the following exit codes:
* `0` ems-cmd was successfully run to completion.
* `1` A fatal error occurred during command parsing or execution.

#### Authorization  
For password-protected spaces, you should pass the password via the command parameter `-password`.
```
ems-cmd upload http://192.168.100.200:6330  -space Default -password your_password -source D:\your\local\folder\file.xml -target \
```
In the example above, a session will be opened for the specified space Default, assuming the password is correct. In case of an incorrect password or if the space doesn't require a password, an error will be thrown.
Some hash computations are applied to the password before it is sent to the Server. 


## Commands

### Status and metadata

#### Retrieve server status

```bash
ems-cmd status http://192.168.100.200:6330
```
###### Parameters
This command has no additional parameters.

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
This command has no additional parameters.

###### Output
```
Available spaces:
* closed one
  Default
Listing done
```
Asterisk `*` means that the space requires authorization.


#### Space status
Returns the status of the specified space. This command may require authorization if the space is password-protected.

```bash
ems-cmd spacestatus http://192.168.100.200:6330 -space "closed one" -password some_password
```
###### Parameters

* `-space` - the space name.
* `-password` - the password (if required).

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
This command lists all tasks in the specified space.
```
ems-cmd listtasks http://192.168.100.200:6330 -space Default
```
###### Parameters
* `-space` - space name, e.g. `Default`.

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
* `-space` - space name, e.g. `Default`.
* `-taskID` - task's GUID.

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
This command starts the specified task and waits until it is finished.

To start the task, you need to know the space name and the task ID. The task ID can be seen in the URL of the task settings page. For instance, if the URL of the task setting page is  `http://localhost:6330/default/tasks/edit/59b824f0-4b81-453f-b9e0-1c58b97c9fb9`, then `59b824f0-4b81-453f-b9e0-1c58b97c9fb9` - is the GUID of the task.

```
ems-cmd run http://192.168.100.200:6330 -space Default -taskID 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
```
###### Parameters
* `-space` - space name, e.g. `Default`.
* `-taskID` - task's GUID.
* `-param:XXX ZZ` - assign task parameter `XXX` with value `ZZ`.

If you want to pass (or override) parameters that were defined in the task settings, add `-param:XXX ZZ` to the ems-cmd execution line. Here, `XXX` is a parameter name, and `ZZ` is the parameter value. At least one space between a parameter name and the parameter value is required.

For instance, if you have a parameter named `Timeout` in your EasyMorph project and want to set it to 73, use `-param:Timeout 73`. Note that parameter names are case-sensitive.

Examples:


Set parameter `Rounds` to `10`:   `-param:Rounds 10`

Set parameter `Full name` to `John Smith`:   `-param:"Full name" "John Smith"`

Set parameter `From Date` to December, 10th 2000:   `-param:"From Date" "2000-12-10"`   (the ISO 8601 date format)


###### Output
```
Attempting to start task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
Project 'sample.morph' is running. Waiting until done.

Task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9 completed
```

#### Start a task asynchronously (fire and forget)
This command returns control immediately after the task is enqueued for execution by the Server.

To start the task, you need to know the space name and the task ID. The task ID can be seen in the URL of the task settings page. For instance, if the URL of the task setting page is  `http://localhost:6330/default/tasks/edit/59b824f0-4b81-453f-b9e0-1c58b97c9fb9`, then `59b824f0-4b81-453f-b9e0-1c58b97c9fb9` - is the GUID of the task.

To see whether the task finished successfully or not, see the Server journal.

```
ems-cmd runasync http://192.168.100.200:6330 -space Default -taskID 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
```
###### Parameters
* `-space` - space name, e.g. `Default`.
* `-taskID` - task GUID.
* `-param:XXX ZZ` - assign task parameter `XXX` with value `ZZ`.

If you want to pass (or override) parameters that were defined in the task settings, add `-param:XXX ZZ` to the ems-cmd execution line. Here, `XXX` is a parameter name, and `ZZ` is the parameter value. At least one space between a parameter name and the parameter value is required.

For instance, if you have a parameter named `Timeout` in your EasyMorph project and want to set it to 73, use `-param:Timeout 73`. Note that parameter names are case-sensitive.

Examples:


Set parameter `Rounds` to `10`:   `-param:Rounds 10`

Set parameter `Full name` to `John Smith`:   `-param:"Full name" "John Smith"`

Set parameter `From Date` to December, 10th 2000:   `-param:"From Date" "2000-12-10"`   (the ISO 8601 date format)

###### Output
```
Attempting to start task 59b824f0-4b81-453f-b9e0-1c58b97c9fb9
Project 'sample.morph' is running.
```

### Files

#### Browse files
This command shows the contents of the specified folder.

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
This command will download one single file from Server.

If a local file with the same name already exists, you will be prompted to overwrite it. Notice, that when you are using a redirected output (e.g. to a file) and a local file already exists, downloading will fail.
Use the parameter `/y` to always overwrite existing files without prompting for a user action.

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
* `-target` - destination folder (your local folder).
* `-source` - relative path to file in the space `-space`
* `/y` - overwrite an existing file (silent confirmation)

###### Output
```
Downloading file 'server\folder\file.xml' from space 'Default' into 'D:\your\local\folder'...
Operation completed
```

#### Upload a file
This command uploads one single to Server.

If a remote file with the same name already exists, you will be prompted to overwrite it. 
Notice, that when you are using a redirected output (e.g. to a file) and a remote file already exists, uploading will fail. 
Use parameter `/y` to always overwrite existing files without prompting for a user action.

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
This command deletes a remote file (i.e. a file stored on Server).

```
ems-cmd del http://192.168.100.200:6330 -space Default -target "folder 2\file.xml" 
```
###### Parameters
* `-space` - space name, e.g. `Default`
* `-target` - relative path in the space `-space` to file 

### Shared Memory

#### 'Recall' (read shared memory value)
This command retrieves the value of a shared memory record as a string.

```
ems-cmd recall http://10.20.30.40:6330 -space Default -key path1\path2\abc
```
###### Parameters
* `-space`: The name of the space, e.g., `Default`.
* `-key`: The key for the shared memory, specified as an arbitrary string.

#### 'Remember' (write shared memory value)
This command writes a value to a shared memory record.

```
ems-cmd remember http://10.20.30.40:6330 -space Default -key path1\path2\abc -value XYZ
```
###### Parameters
* `-space`: The name of the space, e.g., `Default`.
* `-key`: The key for the shared memory record.
* `-value`: The value to be stored in the shared memory record.

#### 'Forget' (delete shared memory value)
This command removes a shared memory record.

```
ems-cmd forget http://10.20.30.40:6330 -space Default -key path1\path2\abc
```
###### Parameters
* `-space`: The name of the space, e.g., `Default`.
* `-key`: The key for the shared memory record.

### SSL errors
If you want to suppress SSL errors,  use the additional parameter `/suppress-ssl-errors`.
```
ems-cmd del http://192.168.100.200:6330 -space Default -target "folder 2\file.xml" /suppress-ssl-errors 
```

## License 

**ems-cmd** is licensed under the [MIT license](https://github.com/easymorph/server-cmd/blob/master/LICENSE).

