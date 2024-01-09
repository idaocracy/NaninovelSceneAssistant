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
            drawers[nameof(SceneAssistantConfiguration.CharacterVariableTemplate)] = p => DrawWhen(Configuration.CreateCharactersVariableMenu, p);
            drawers[nameof(SceneAssistantConfiguration.ChapterVariableTemplate)] = p => DrawWhen(Configuration.CreateChaptersMenu, p);

            return drawers;
        }
    }
}
