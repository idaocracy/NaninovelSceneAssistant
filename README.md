## Introduction & Installation

Naninovel Scene Assistant is an extension that lets you easily modify Naninovel objects in real time and copy their corresponding command and parameters directly into the visual editor or clipboard. Naninovel is a visual novel extension for Unity: https://naninovel.com/

Found this extension useful? This extension was made during my spare time so any donations are appreciated. 
https://ko-fi.com/idaot

## Demo 
Try the WebGL demo of the UI version of Scene Assistant here: https://idaocracy.github.io/NaninovelSceneAssistantWebGLDemo/ <br/>
The Vignette and ColorGradingLDR objects displayed in the demo can be obtained from my NaninovelPostProcess extension: https://github.com/idaocracy/NaninovelPostProcess 

### Installation (v2.0+)
1. In Unity's Package Manager, click the plus sign and navigate to *Add package from git URL...*. If you don't have git installed, install it and restart the computer.
2. Type in https://github.com/idaocracy/NaninovelSceneAssistant.git and it should install automatically. **For 1.20 users, type in https://github.com/idaocracy/NaninovelSceneAssistant.git#1.20 instead (you can skip the other steps).**
3. In Naninovel's Engine configuration, unfold the Type Assemblies dropdown and add these two assembly definitions:
- **Idaocracy.NaninovelSceneAssistant.Runtime**
- **Idaocracy.NaninovelSceneAssistant.Editor**
4. Reimport the Naninovel folder for Naninovel to recognise the extension (right click on the Naninovel folder -> Reimport)
3. You are done! You can now access the scene assistant from the Naninovel menu.

### UI setup
Naninovel Scene Assistant has a UI version which is aimed at devs who don't have access to Unity or modders. Please note that the UI is designed for use with mouse and keyboard.

Setup instructions: 
1. Search **SceneAssistantUI** in the Project search field. Set the Search setting to **All**.
2. Add the UI to the UI resources **(Naninovel -> Resources -> UI)**. 
3. Development console setup: In Naninovel's **Engine** configuration, make sure **Enable Development Console** is checked. Enter Play Mode and while a script is playing, call the development console by using the **Toggle Console Key** in the Engine configuration and type in **scn**. The UI should appear. 
4. Naniscript command setup: Use the **@sceneAssistant** command to call it in naniscript.    

Generated commands will be copied to clipboard, similar to the Editor version (except WebGL, see info below). Additionally you can save the string to a file generated at the application's **StreamingAssets** folder (except Android, which uses **persistentDataPath** instead). 

#### WebGL limitations
In WebGL, it's not currently possible to save a generated string to a text file nor is it possible to copy the value to the clipboard automatically. However, you can use **Ctrl+C/CMD+C** to copy the value yourself, however please be aware that this won't work while in fullscreen mode.   

### Video walkthrough 
Check this video for a quick guide on installation and usage. Please note that the installation instructions are for V1 which is no longer being maintained. 

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/Qc5XYE-ojx8/0.jpg)](https://www.youtube.com/watch?v=Qc5XYE-ojx8)  

## Scene Assistant Overview

<img width="500" alt="image" src="https://github.com/idaocracy/NaninovelSceneAssistant/assets/77254066/9de32a61-ed85-4707-8d92-63604f8b10e4">

**From top to bottom:**

**Script player:** Play will resume playback, Pause will activate wait for input mode, Stop will stop the player. Skip and Rewind will skip or rewind once and activate wait for input mode immediately after.

**Insert/Copy commands (@ or []):** Will insert the command of the current object directly into the visual editor (if currently visible) or alternatively, copy the command to the clipboard buffer. Results will be visible in the text field below, and the results can also be logged when Log Results is checked. 

**Insert/Copy all or Copy Selected:** Insert/Copy all or selected type of objects listed under the buttons. The list of object types are updated according to the currently present objects. 
____
**Id**: The object ID. Click on the object Id to get the list of objects you can modify.  

**Inspect object**: Inspect the scene object directly (Editor only)

**Parameter options:** Each object has a list of parameters you can modify in real time. These options are available out of the box:
- Appearance (character, background and printer)
- Look Direction (character)
- Tint Color (character, background and printer)* 
- Pos/Position/Offset (all)
- Rotation (all except choice buttons)
- Scale (all except camera and choice buttons)
- Orthographic (camera only)
- Camera Components (camera only)
- Roll (camera only)
- Spawn parameters**
- Custom parameters

\* Please note that you need to set up the tint behaviour for the printer prefab yourself, by hooking a method to the public **On Tint Color Changed** event

\** Spawn parameters (apart from transform values) have to be set up manually. See my NaninovelPostProcess V2 extension for reference. 

**Parameter/reset options:**  
- Select/Deselect All: Will check/uncheck all the parameter options for the current object.
- Default: Will attempt to retrieve the default value for the parameter (some which are set in the object's configuration menu)
- Reset: Will reset all values of the current object. 
- Rollback: Will perform a faux rollback that resets all values found in the scene.

**Pose Options (Editor only, available for Backgrounds and Characters):**
- Load Pose: Use this to load one of the available poses.
- Save Pose: Name a pose and once saved, it will be copied alongside the object's current values directly to the actor or shared configuration.  

## Debug menus  

In addition, Naninovel Scene Assistant has debug menus for the following items: custom variables, unlockables and naniscripts.<br /><br />
On top of searching for specific items, it's possible to create filter menus as well as generate a menu based on a regex template (available for characters and chapters). All of these can be configured in the Scene Assistant configuration (available in 2.5.0+)
<br /><br />
<img width="783" alt="image" src="https://github.com/idaocracy/NaninovelSceneAssistant/assets/77254066/8cbe30f8-f710-492f-99ec-ef5bc49e221d"><br />
The above screenshot demonstrates how a Characters filter menu is generated as well as two other filter menus specified in the filter menu options list.
<br /><br />
Please be aware that the filter menus are only available in the editor version, they will be added to the UI version at a later point.

## Current limitations
- The float parameter fields in the editor version are fugly as heck. Unfortunately due to the limitations of FloatField, I couldn't make them any prettier.

## Contact
If you need help with the extension, you can contact me on here or on Discord. I am the tech support person (only yellow username) on the Naninovel discord.  
