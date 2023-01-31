using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public interface ISceneAssistantSpawnObject : ISceneAssistantObject
    {
        bool IsTransformable();
        string GetSpawnId();
    }

    public interface ISceneAssistantObject
    {
        string GetCommandId();
        List<Param> Params { get; }
        void InitializeParams();
    }

    public static class SceneAssistantHelpers
    {

        public static string GetAllCommands(List<NaninovelObject> objectList)
        {
            var allString = String.Empty;
            foreach (var o in objectList) allString = allString + o.GetCommandLine() + "\n";
            return allString;
        }

        public static T EnumField<T>(object value) where T : Enum
        {
            var options = Enum.GetNames(typeof(T));
            var valueIndex = Array.IndexOf(options, value.ToString());
            valueIndex = EditorGUILayout.Popup(valueIndex, options);
            return (T)Enum.Parse(typeof(T), options[valueIndex]);
        }

        //public static Vector3 PosField(string label, Vector3 position, bool paramIsButton = true)
        //{
        //    var cameraConfiguration = Engine.GetConfiguration<CameraConfiguration>();
        //    position = cameraConfiguration.WorldToSceneSpace(position);
        //    position.x *= 100;
        //    position.y *= 100;
        //    //position = Vector3Field(label, position, paramIsButton);
        //    position.x /= 100;
        //    position.y /= 100;
        //    return cameraConfiguration.SceneToWorldSpace(position);
        //}

        public static bool ShowButton(string label)
        {
            if (GUILayout.Button(label, GUILayout.Width(150))) return true;
            else return false;
        }

        public static void ShowValueOptions(Param param)
        {
            param.Selected = EditorGUILayout.Toggle(param.Selected, GUILayout.Width(20f));

            if (param.HasOptions)
            {
                if (ShowButton(param.Id)) Engine.GetService<SceneAssistantManager>().ClipboardString = FormatValue(param.Value);
            }
            else GUILayout.Label(param.Id.ToString());
        }

        public static string FormatValue(object value)
        {
            if (value is Vector2 || value is Vector3 || value is Vector4) return value.ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
            else if(value is bool) return value.ToString().ToLower();
            else if(value is Quaternion quaternion) return quaternion.eulerAngles.ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
            else return value.ToString();
        }

    }

}
