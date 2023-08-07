v2.4.0 August 2023
- Added option to load and save poses for characters and backgrounds
- Printer data will no longer have an appearance parameter if no appearances are specified within the prefab. Appearances will display in a list in the editor, similar to Sprite and Video actors.  

v2.3.2 August 2023
- Changed apppearance list parsing 

v2.3.1 July 2023
- Fixed missing reference exception caused by built-in camera spawn effects

v2.3.0 June 2023
- Added UI version of Scene Assistant.
- Script Player is now visible on all tabs.
- Minor bug-fixes and refactoring.

v2.2.1 May 2023
- Fixed 1.19 incompatibility issue.

v2.2.0 May 2023
- Scene Assistant will now clear itself on command/load/rollback/reset start operation to prevent object nullrefs when an object is destroyed before the operation is finished (like choices).
- Refactoring and cleanup.

v2.1.0 April 2023
- Added a script player and other tweaks to Scene Assistant to better communicate and control the availability of Scene Assistant. 
- Replaced "Pause Script Player In Hover" with "Disable Rollback On Hover" in Scene Assistant options. 

v2.0.0 March 2023
- You can now insert lines directly into the visual editor without having to copy the command to the clipboard buffer first.
- The object list will now update according to what's visible as opposed to what's (pre)loaded, eliminating some clutter.
- The original parameter states are now saved and these are restored when the parameters are deselected. You can also choose to ignore unchanged values when checking the "Exclude State Values" option.
- Added three new tabs to the Scene Assistant windows: Variables, Unlockables, Scripts. You can now track their states and debug them, similar to the built-in Custom Variable GUI and Script Navigator.
- Added an option to pause the Script Player on hover, which causes the script player to pause after the current command has completed its transition.
- Hovering on the window will no longer cause any ongoing transitions to complete instantly. It is also not possible to change the values as long as there is an ongoing transition to prevent unwanted changes.
- Improved stability. The extension is designed to remain active and functional throughout the engine's lifetime.
- Scene Assistant is now managed by a custom engine service, and it should be possible to override its behaviour. Instructions coming soon.
- Added SceneAssistantSpawnObject script which can be used to add parameters to spawn objects. A working example can be found in my newly updated NaninovelPostProcess V2: https://github.com/idaocracy/NaninovelPostProcess