using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NaninovelSceneAssistant {

    public abstract class NaninovelObject : INaninovelObject, IEquatable<NaninovelObject>
    {
        public abstract string Id { get; set; }
        public abstract List<Param> Params { get; }
        public virtual string GetCommandLine() => GetCommandNameAndId() + Params != null ? " " + string.Join(" ", Params.Where(p => p.GetValue() != null && p.Selected).Select(p => p.Id.ToLower() + ":" + p.GetValue())) : null;
        public virtual void ShowParamValues()
        {
            foreach (Param param in Params)
            {
                EditorGUILayout.BeginHorizontal();
                SceneAssistantHelpers.ShowValueOptions(param);
                param.DisplayField();
                EditorGUILayout.EndHorizontal();
            }
        }

        protected abstract void InitializeParams();
        public abstract string GetCommandNameAndId();
        public abstract GameObject GetGameObject();

        public bool Equals(NaninovelObject other)
        {
            return this.GetType() == other.GetType() && this.Id == other.Id;
        }

    }


    public interface INaninovelObject
    {
        string Id { get; set; }

        List<Param> Params { get; }

        GameObject GetGameObject();

        string GetCommandNameAndId();
    }


    public class Param
    {
        public string Id;
        public object Value;
        public Func<object> OnEditor;
        public bool Selected = true;
        public bool HasOptions = true;

        public void DisplayField() => Value = OnEditor(); 
        public string GetValue() => SceneAssistantHelpers.FormatValue(Value);
    }

}