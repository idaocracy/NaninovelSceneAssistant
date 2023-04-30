## Introduction & Installation

Naninovel Scene Assistant is an extension that lets you easily modify Naninovel objects in real time and copy their corresponding command and parameters directly into the visual editor or clipboard.

### Installation (v2.0+)
1. In Unity's Package Manager, click the plus sign and navigate to *Add package from git URL...*. If you don't have git installed, install it and restart the computer.
2. Type in https://github.com/idaocracy/NaninovelSceneAssistant.git and it should install automatically. 
3. In Naninovel's Engine configuration, unfold the Type Assemblies dropdown and add these two assembly definitions:
- **Idaocracy.NaninovelSceneAssistant.Runtime**
- **Idaocracy.NaninovelSceneAssistant.Editor**
4. Reimport the Naninovel folder for Naninovel to recognise the extension (right click on the Naninovel folder -> Reimport)
3. You are done! You can now access the scene assistant from the Naninovel menu.

### Installation (V1) and basic walkthrough 
Check this video for a quick guide on installation and usage:

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/Qc5XYE-ojx8/0.jpg)](https://www.youtube.com/watch?v=Qc5XYE-ojx8)  
TLDW: Add the script to an Editor folder (If you don't have one for your own use, create one under Assets). The window can be accessed via the Naninovel menu. Enter Play Mode and tada.  

## Overview

<img width="500" alt="image" src="https://user-images.githubusercontent.com/77254066/235355459-a1e9484b-39f7-4e83-8da4-d8ff9e95c21c.png">


**From top to bottom:**

**Script player:** Play will resume playback, Pause will activate wait for input mode, Stop will stop the player. Skip and Rewind will skip or rewind once and activate wait for input mode immediately after.

**Insert/Copy commands (@ or []):** Will insert the command of the current object directly into the visual editor (if currently visible) or alternatively, copy the command to the clipboard buffer. Results will be visible in the text field below, and the results can also be logged when Log Results is checked. 

**Insert/Copy all or Copy Selected:** Insert/Copy all or selected type of objects listed under the buttons. The list of object types are updated according to the currently present objects. 
____
**Id**: The object ID. Click on the object Id to get the list of objects you can modify.  
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

## Current limitations
- The float parameter fields are fugly as heck. Unfortunately due to the limitations of FloatField, I couldn't make them any prettier.

## Future features
- A UI version of this extension is in the works, hence the Runtime scripts in the extension. For now the extension won't be included in builds as the assembly definitions target editor only.  

## Contact
If you need help with the extension, you can contact me on here or on Discord. I am the tech support person (only yellow username) on the Naninovel discord.  
