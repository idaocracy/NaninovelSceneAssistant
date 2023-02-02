using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NaninovelSceneAssistant {

    public abstract class NaninovelObject : IEquatable<NaninovelObject>
    {
        protected NaninovelObject() => InitializeParams();
        public abstract string Id { get; }
        protected virtual List<Param> Params { get; private set; }
        public abstract GameObject GameObject { get; }
        protected abstract string GetCommandNameAndId();
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

        protected void InitializeParams() => Params = new List<Param>();
        protected abstract void AddParams();

        // todo get this to work
        public bool Equals(NaninovelObject other)
        {
            return this.Id == other.Id;
        }

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