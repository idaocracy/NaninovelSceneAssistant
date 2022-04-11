# NaninovelSceneAssistant

## Introduction & Installation

Naninovel Scene Assistant is an extension that lets you easily modify Naninovel objects in real time and copy their corresponding command and parameters to a clipboard. 

Check this video for a quick guide on installation and usage:
[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/Qc5XYE-ojx8/0.jpg)](https://www.youtube.com/watch?v=Qc5XYE-ojx8)  
TLDW: Add the script to an Editor folder (If you don't have one for your own use, create one under Assets). The window can be accessed via the Naninovel menu. Enter Play Mode and tada.  

## Overview

![image](https://user-images.githubusercontent.com/77254066/162417149-db622e5f-1f01-4861-8fe1-7d10deca85ff.png)

**From top to bottom:**  
**Copy commands (@ or []):** Copy a string generated from selected parameter options to the clipboard buffer as well as the Clipboard field. Will always include the command corresponding to the object type.   
**Copy all or Copy Selected:** Copy all or select type of objects listed under ID. Please note that you cannot omit parameter options from these strings.  
____
**Id**: The object ID. Click on the object Id to get the list of objects you can modify.  
**Parameter options:** Each object has a list of parameters you can modify in real time. To exclude the parameter from string generation, uncheck the box in front of the parameter name. In case of transforms, you can omit x,y,z values specifically by unchecking the boxes on the far right.  
- Appearance (character or background)
- Look Direction (character)
- Tint Color (character, background and printer)* 
- Pos/Position/Offset (all)
- Rotation (all except choice buttons)
- Scale (all except camera and choice buttons)
- Orthographic (camera only)
- Camera Components (camera only)**

/* Please note that you need to set up the tint behaviour for the printer prefab yourself, by hooking a method to the public **On Tint Color Changed** event

**Parameter/reset options:**  
- Rollback:  Will attempt to roll back to the newest state snapshot. Please note that this option is only available when you have State Rollback enabled in Naninovel's State Configuration.
- Nullify Transforms: Will reset to default or visibility-friendly values. 
- Select/Deselect All: Will check/uncheck all the parameter options for the current object.

### Current limitations

- When the object is going through a timed transition, hovering over the Scene Assistant window will instantly complete the transition. This is due to Naninovel registering the new values before any tweens are completed, I do not intend to work around this.   
- Spawn effect parameters (params) cannot be manipulated. I intend to release my own pack of spawn effects soon that will be compatible with the scene assistant. 
- This is my first time working on Editor scripting and unfortunately the documentation can be insufficient so there will be bugs. If you can identify a bug and reproduce it, please let me know!

### Contact

If you need help with the extension, you can contact me here or on Discord. I am the tech support person (only yellow username) on the Naninovel discord.  
