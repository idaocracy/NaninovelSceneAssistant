using System.Collections.Generic;
using System;
using UnityEditor;
using Naninovel;


namespace NaninovelSceneAssistant
{
    public class SceneAssistantSettings : ConfigurationSettings<SceneAssistantConfiguration>
    {
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(SceneAssistantConfiguration.CharacterVariableFilterTemplate)] = p => DrawWhen(Configuration.CreateCharactersVariableFilterMenu, p);
            drawers[nameof(SceneAssistantConfiguration.ChapterVariableFilterTemplate)] = p => DrawWhen(Configuration.CreateChapterFilterMenu, p);

            return drawers;
        }
    }
}
