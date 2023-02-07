using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Naninovel;
using Unity.Plastic.Newtonsoft.Json.Linq;
using Codice.CM.Common;

namespace NaninovelSceneAssistant {

    public interface INaninovelObject
    {
        string Id { get; set; }
        GameObject GameObject { get; }
        string GetCommandLine();
        List<CommandParam> Params { get; }
        SortedList<string, CustomVar> CustomVars { get; }
        bool HasPosValues(out int posParamIndex, out int positionParam);
    }

    public abstract class NaninovelObject<TEngineService> : INaninovelObject where TEngineService : class, IEngineService
    {
        protected virtual void Initialize()
        {
            AddParams();
            customVariableManager = Engine.GetService<ICustomVariableManager>();
        }

        protected TEngineService EngineService { get => Engine.GetService<TEngineService>(); }
        public abstract string Id { get; set; }
        public virtual List<CommandParam> Params { get; protected set; } = new List<CommandParam>();
        public virtual SortedList<string, CustomVar> CustomVars { get; protected set; } = new SortedList<string, CustomVar>();
        public abstract GameObject GameObject { get; }
        protected abstract string CommandNameAndId { get; }
        public virtual string GetCommandLine() => Params != null ? CommandNameAndId + " " + string.Join(" ", Params.Where(p => p.GetValue != null && p.Selected).Select(p => p.Id.ToLower() + ":" + p.GetValue)) : null;
        protected abstract void AddParams();
        public virtual bool HasPosValues(out int posParamIndex, out int positionParamIndex)
        {
            var hasPosValues = Params.Exists(p => p.Id == "Pos") && Params.Exists(p => p.Id == "Position");
            posParamIndex = Params.FindIndex(p => p.Id == "Position");
            positionParamIndex = Params.FindIndex(p => p.Id == "Pos");

            return hasPosValues;
        }
        
        protected ICustomVariableManager customVariableManager;

        public static string GetAllCommands(List<INaninovelObject> objectList)
        {
            var allString = String.Empty;
            foreach (var o in objectList) allString = allString + o.GetCommandLine() + "\n";
            return allString;
        }
    }

    public class CommandParam
    {
        public string Id { get; private set; }
        public bool Selected { get; set; } = true;
        public bool HasOptions { get; set; } = true;
        public object DefaultValue { get; set; }


        private Func<object> getValue;
        private Action<object> setValue;
        public Action OnEditor;

        public CommandParam(string id, Func<object> getValue, Action<object> setValue, Action onEditor)
        {
            Id = id;
            this.getValue = getValue;
            this.setValue = setValue;
            OnEditor = onEditor;
        }

        public void DisplayField() => OnEditor();
        public string GetValue => FormatValue(getValue());
        public void GetDefaultValue()
        {
            if (DefaultValue != null) setValue(DefaultValue);
            else
            {
                var value = getValue().GetType();
                if (value.IsValueType) setValue(Activator.CreateInstance(value));
            }

        }

        private static string FormatValue(object value)
        {
            if (value is Vector2 || value is Vector3 || value is Vector4) return value.ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
            else if (value is bool) return value.ToString().ToLower();
            else if (value is Quaternion quaternion) return quaternion.eulerAngles.ToString("0.##").Replace(" ", "").Replace("(", "").Replace(")", "");
            else if (value is Color color) return "#" + ColorUtility.ToHtmlStringRGBA(color);
            else return value?.ToString();
        }
    }

    public class CustomVar
    {
        public string Name;
        public string Value;

        public CustomVar(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public void DisplayField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Name);
            Value = EditorGUILayout.DelayedTextField(Value);
            Engine.GetService<ICustomVariableManager>().SetVariableValue(Name, Value);
            EditorGUILayout.EndHorizontal();
        }
    }

    public class Unlockable
    {
        private string Name;
        public bool Value;
        private string[] States = new string[] { "Locked", "Unlocked" };
        private int stateIndex;

        public Unlockable(string name, bool value)
        {
            Name = name;
            Value = value;
            stateIndex = value ? 1 : 0;
        }

        public void DisplayField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Name);
            stateIndex = EditorGUILayout.Popup(stateIndex, States);
            Value = stateIndex == 1 ? true : false;
            Engine.GetService<IUnlockableManager>().SetItemUnlocked(Name, Value);
            EditorGUILayout.EndHorizontal();
        }
    }



}

