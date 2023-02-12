#if UNITY_POST_PROCESSING_STACK_V2

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public class SceneAssistantSettings : ConfigurationSettings<SceneAssistantConfiguration>
    {
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers()
        {
            var drawers = base.OverrideConfigurationDrawers();
            return drawers;
        }

    }
}
#endif