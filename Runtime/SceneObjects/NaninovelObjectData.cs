using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Naninovel;

namespace NaninovelSceneAssistant {

    public interface INaninovelObjectData
    {
        string Id { get; }
        GameObject GameObject { get; }
        string GetCommandLine(bool inlined = false, bool paramsOnly = false);
        string GetAllCommands(Dictionary<string, INaninovelObjectData> objectList, Dictionary<Type, bool> objectTypeList, bool selected = false);
        List<ParameterValue> Params { get; }
        bool HasPosValues { get; }

        SortedList<string, VariableValue> CustomVars { get; }
    }

    public abstract class NaninovelObjectData<TEngineService> : INaninovelObjectData where TEngineService : class, IEngineService
    {
        protected virtual void Initialize()
        {
            AddParams();
            customVariableManager = Engine.GetService<ICustomVariableManager>();
        }

        protected static TEngineService EngineService { get => Engine.GetService<TEngineService>(); }
        public abstract string Id { get; }
        public virtual List<ParameterValue> Params { get; protected set; } = new List<ParameterValue>();
        public virtual SortedList<string, VariableValue> CustomVars { get; protected set; } = new SortedList<string, VariableValue>();
        public abstract GameObject GameObject { get; }
        protected abstract string CommandNameAndId { get; }
        protected abstract void AddParams();
        public bool HasPosValues => Params.Exists(p => p.Name == "Pos") && Params.Exists(p => p.Name == "Position");

        public virtual string GetCommandLine(bool inlined = false, bool paramsOnly = false)
        {
            if (Params == null)
            {
                Debug.LogWarning("No parameters found.");
                return null; 
            }

            var paramString = string.Join(" ", Params.Where(p => p.GetCommandValue() != null
                && p.Selected && p.IsParameter).Select(p => p.Name.ToLower() + ":" + p.GetCommandValue()));

            if (paramsOnly) return paramString;

            var commandString = CommandNameAndId + " " + paramString;

            return inlined ? "[" + commandString + "]" : "@" + commandString;
        }

        public string GetAllCommands(Dictionary<string, INaninovelObjectData> objectList, Dictionary<Type,bool> objectTypeList, bool selected = false)
        {
            var allString = String.Empty;

            foreach (var obj in objectList.Values)
            {
                if (selected && !objectTypeList[obj.GetType()]) continue;
                else allString = allString + obj.GetCommandLine() + "\n";
            }

            return allString;
        }

        protected ICustomVariableManager customVariableManager;


    }


    public class ParameterValue 
    {
        public string Name { get; }

        //todo find out why an exception is thrown on applying default values
        public object Value { get => getValue(); set => setValue(value); }
        public bool Selected { get; set; } = true;

        //True if a command parameter matches the name of this instance (e.g position, rotation, etc),
        //set to false if the parameter is part of a list (e.g all the values within params with the exception of the one called Params)
        public bool IsParameter { get; }
        private object DefaultValue { get; }
        public Action<ISceneAssistantLayout, ParameterValue> OnLayout { get; private set; }

        private Func<object> getValue;
        private Action<object> setValue;

        public ParameterValue(string id, Func<object> getValue, Action<object> setValue, Action<ISceneAssistantLayout, ParameterValue> onLayout, bool isParameter = true, object defaultValue = null)
        {
            this.Name = id;
            this.getValue = getValue;
            this.setValue = setValue;
            this.OnLayout = onLayout;
            this.IsParameter = isParameter;
            this.DefaultValue = defaultValue;
        }

        public string GetCommandValue() => FormatValue(Value);
        public object GetDefaultValue()
        {
            if (setValue == null || getValue == null) return default;
            else if (DefaultValue != null) return DefaultValue;
            else
            {
                var value = Value.GetType();
                if (value.IsValueType) return Activator.CreateInstance(value);
            }
            return default;
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
            else if (value is Texture texture && texture is null) return string.Empty;
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

    public class VariableValue
    {
        public string Name { get; }
        public string Value { get => getValue(); set => setValue(value); }

        private Func<string> getValue;
        private Action<string> setValue;

        public VariableValue(string name)
        {
            Name = name;
            getValue = () => Engine.GetService<ICustomVariableManager>().GetVariableValue(name);
            setValue = (value) => Engine.GetService<ICustomVariableManager>().SetVariableValue(name, value);
        }

        public void DisplayField(ISceneAssistantLayout layout)
        {
            layout.VariableField(this);
        }
    }

    public class UnlockableValue
    {
        public string Name { get; }
        public bool Value { get => getValue(); set => setValue(value); }
        public enum UnlockableState { Unlocked, Locked };
        public UnlockableState EnumValue { get; set; }

        private Func<bool> getValue;
        private Action<bool> setValue;
        private UnlockableState enumValue;

        public UnlockableValue(string name)
        {
            Name = name;
            getValue = () => Engine.GetService<IUnlockableManager>().ItemUnlocked(name);
            setValue = (value) => Engine.GetService<IUnlockableManager>().SetItemUnlocked(name, value);
            enumValue = Engine.GetService<IUnlockableManager>().ItemUnlocked(name) ? UnlockableState.Unlocked : UnlockableState.Locked;
        }

        public void DisplayField(ISceneAssistantLayout layout)
        {
            layout.UnlockableField(this);
        }
    }



}

