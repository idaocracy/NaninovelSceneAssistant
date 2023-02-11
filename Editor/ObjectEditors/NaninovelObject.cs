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

        protected static TEngineService EngineService { get => Engine.GetService<TEngineService>(); }

        public abstract string Id { get; set; }
        public virtual List<CommandParam> Params { get; protected set; } = new List<CommandParam>();
        public virtual SortedList<string, CustomVar> CustomVars { get; protected set; } = new SortedList<string, CustomVar>();
        public abstract GameObject GameObject { get; }
        protected abstract string CommandNameAndId { get; }
        public virtual string GetCommandLine() => Params != null ? CommandNameAndId + " " + string.Join(" ", Params.Where(p => p.GetCommandValue() != null && p.Selected && p.HasCommandOptions).Select(p => p.Id.ToLower() + ":" + p.GetCommandValue())) : null;
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

    public interface ICommandParam
    {
        string Id { get; }
        bool Selected { get; set; }
        bool HasCommandOptions { get; set; }
    }


    public class CommandParam : ICommandParam
    {
        public string Id { get; }
        public bool Selected { get; set; } = true;
        public bool HasCommandOptions { get; set; } = true;
        public object DefaultValue { get; set; }

        public Func<object> GetValue { get; private set; }
        public Action<object> SetValue { get; private set; }
        public Action<ISceneAssistantLayout, CommandParam> OnLayout { get; private set; }

        public CommandParam(string id, Func<object> getValue, Action<object> setValue, Action<ISceneAssistantLayout, CommandParam> onLayout)
        {
            Id = id;
            this.GetValue = getValue;
            this.SetValue = setValue;
            OnLayout = onLayout;
        }

        public void DisplayField(ISceneAssistantLayout layout) => OnLayout(layout, this);
        public string GetCommandValue() => FormatValue(GetValue());
        public void GetDefaultValue()
        {
            if (DefaultValue != null) SetValue(DefaultValue);
            else
            {
                var value = GetValue().GetType();
                if (value.IsValueType) SetValue(Activator.CreateInstance(value));
            }

        }

        private static string FormatValue(object value)
        {
            if (value is Vector2 || value is Vector3 || value is Vector4) return value.ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
            else if (value is bool) return value.ToString().ToLower();
            else if (value is Quaternion quaternion) return quaternion.eulerAngles.ToString("0.##").Replace(" ", "").Replace("(", "").Replace(")", "");
            else if (value is Color color) return "#" + ColorUtility.ToHtmlStringRGBA(color);
            else if (ValueIsDictionary(value, out var namedList)) return namedList;
            else return value?.ToString();
        }

        private static bool ValueIsDictionary(object value, out string namedList)
        {
            namedList = string.Empty;

            if(value is Dictionary<string, string> namedValues){
                var namedStrings = new List<string>();
                foreach (var item in namedValues) namedStrings.Add(item.Key + "." + item.Value);
                namedList = string.Join(",", namedStrings);
            }

            return !string.IsNullOrEmpty(namedList);
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

