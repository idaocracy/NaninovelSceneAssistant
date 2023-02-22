using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Naninovel;

namespace NaninovelSceneAssistant {

    public interface INaninovelObject
    {
        string Id { get; }
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
        public abstract string Id { get; }
        public virtual List<CommandParam> Params { get; protected set; } = new List<CommandParam>();
        public virtual SortedList<string, CustomVar> CustomVars { get; protected set; } = new SortedList<string, CustomVar>();
        public abstract GameObject GameObject { get; }
        protected abstract string CommandNameAndId { get; }
        public virtual string GetCommandLine() => Params != null ? CommandNameAndId + " " + string.Join(" ", Params.Where(p => p.GetCommandValue() != null && p.Selected && p.HasCommandOptions).Select(p => p.Name.ToLower() + ":" + p.GetCommandValue())) : null;
        protected abstract void AddParams();
        public virtual bool HasPosValues(out int posParamIndex, out int positionParamIndex)
        {
            var hasPosValues = Params.Exists(p => p.Name == "Pos") && Params.Exists(p => p.Name == "Position");
            posParamIndex = Params.FindIndex(p => p.Name == "Position");
            positionParamIndex = Params.FindIndex(p => p.Name == "Pos");

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
        public string Name { get; }
        public object Value { get => getValue(); set => setValue(value); }
        public bool Selected { get; set; } = true;
        public bool HasCommandOptions { get; set; } = true;
        public object DefaultValue { get; set; }
        public Action<ISceneAssistantLayout, CommandParam> OnLayout { get; private set; }
        private Func<object> getValue;
        private Action<object> setValue;

        public CommandParam(string id, Func<object> getValue, Action<object> setValue, Action<ISceneAssistantLayout, CommandParam> onLayout)
        {
            Name = id;
            this.getValue = getValue;
            this.setValue = setValue;
            OnLayout = onLayout;
        }

        public string GetCommandValue() => FormatValue(Value);
        public void GetDefaultValue()
        {
            if (DefaultValue != null) Value = DefaultValue;
            else
            {
                var value = Value.GetType();
                if (value.IsValueType) Value = Activator.CreateInstance(value);
            }
        }

        public void DisplayField(ISceneAssistantLayout layout)
        {
            if (layout == null) return;

            OnLayout(layout, this);
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
        public string Name { get; }
        public string Value { get => getValue(); set => setValue(value); }

        private Func<string> getValue;
        private Action<string> setValue;

        public CustomVar(string name)
        {
            Name = name;
            getValue = () => Engine.GetService<ICustomVariableManager>().GetVariableValue(name);
            setValue = (value) => Engine.GetService<ICustomVariableManager>().SetVariableValue(name, value);
        }

        public void DisplayField(ISceneAssistantLayout layout)
        {
            layout.CustomVarField(this);
        }
    }

    public class Unlockable
    {
        public string Name { get; }
        public bool Value { get => getValue(); set => setValue(value); }
        public int stateIndex { get => getValue() ? 1 : 0; set => setValue(value == 1 ? true : false); }

        private string[] States = new string[] { "Locked", "Unlocked" };

        private Func<bool> getValue;
        private Action<bool> setValue;

        public Unlockable(string name)
        {
            Name = name;
            getValue = () => Engine.GetService<IUnlockableManager>().ItemUnlocked(name);
            setValue = (value) => Engine.GetService<IUnlockableManager>().SetItemUnlocked(name, value);
        }

        public void DisplayField(ISceneAssistantLayout layout)
        {
            layout.UnlockableField(this, stateIndex, States);
        }
    }



}

