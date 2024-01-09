using System;
using System.Collections;
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
        
        public bool CreateCharactersVariableMenu;

        public string CharacterVariableTemplate = "{%CHARACTERID%}_";

        public List<string> CustomVariableMenus;
        
        [Header("Scripts")]
        
        public bool CreateChaptersMenu;

        [FormerlySerializedAs("ChapterTemplate")] public string ChapterVariableTemplate = "Chapter{%NUMBER%}";
        public List<string> ScriptsMenus;
        
        [Header("Unlockables")]
        
        public bool CreateCGMenu;

        public List<string> UnlockableMenus;
    }
}



