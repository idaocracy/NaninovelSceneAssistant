using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Naninovel;

namespace NaninovelSceneAssistant
{


    public static class SceneAssistantHelpers
    {

        public static T EnumField<T>(object value) where T : Enum
        {
            var options = Enum.GetNames(typeof(T));
            var valueIndex = Array.IndexOf(options, value.ToString());
            valueIndex = EditorGUILayout.Popup(valueIndex, options);
            return (T)Enum.Parse(typeof(T), options[valueIndex]);
        }

        public static Vector3 PosField(string label, Vector3 position, bool paramIsButton = true)
        {
            var cameraConfiguration = Engine.GetConfiguration<CameraConfiguration>();
            position = cameraConfiguration.WorldToSceneSpace(position);
            position.x *= 100;
            position.y *= 100;
            //position = Vector3Field(label, position, paramIsButton);
            position.x /= 100;
            position.y /= 100;
            return cameraConfiguration.SceneToWorldSpace(position);
        }





    }

}
