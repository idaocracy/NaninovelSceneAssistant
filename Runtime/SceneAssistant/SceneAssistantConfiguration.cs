using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEngine.Serialization;

namespace  NaninovelSceneAssistant
{
    [EditInProjectSettings]
    public class SceneAssistantConfiguration : Configuration
    {
        [Header("Custom Variables")]
        
        [Tooltip("Automatically create a menu for character-related variables. The character id specified in the variables should correspond to the character id in the editor resources (not Display name).")]
        public bool CreateCharactersVariableMenu;

        [Tooltip("{%CHARACTERID%} is a stand-in for the character id and should not be modified or removed.")]
        public string CharacterVariableTemplate = "{%CHARACTERID%}_";

        [Tooltip(("Variable name should start with a letter and can contain only latin characters, numbers and underscores."))]
        public List<string> CustomVariableMenus;
        
        [Header("Unlockables")]
        [Tooltip("Automatically create a menu for CG resources.")]
        public bool CreateCGMenu;
        
        [Tooltip(("Variable name should start with a letter and can contain only latin characters, numbers and underscores."))]
        public List<string> UnlockableMenus;
        
        [Header("Scripts")]
        [Tooltip("Automatically create a menu for chapters according to a template.")]
        public bool CreateChaptersMenu;

        [Tooltip("{%NUMBER%} is a stand-in for the character id and should not be modified or removed.")]
        public string ChapterVariableTemplate = "Chapter{%NUMBER%}";
        
        [Tooltip(("Variable name should start with a letter and can contain only latin characters, numbers and underscores."))]
        public List<string> ScriptsMenus;
    }
}



