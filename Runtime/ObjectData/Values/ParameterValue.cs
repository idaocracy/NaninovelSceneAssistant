using NaninovelSceneAssistant;
using System.Collections.Generic;
using System;
using UnityEngine;
using Naninovel;

public class ParameterValue
{
    public string Name { get; }
    public object Value { get => getValue() ?? null; set { if (getValue != null && setValue != null) setValue(value); } }
    public bool Selected { get; set; } = true;

    //True if a command parameter matches the name of this instance (e.g position, rotation, etc),
    //set to false if the parameter is part of a list (e.g all the values within params with the exception of the one called Params)
    public bool IsParameter { get; }
    public Func<bool> Condition { get; }
    public Action<ISceneAssistantLayout, ParameterValue> OnLayout { get; private set; }

    private Func<object> getValue;
    private Action<object> setValue;
    private object defaultValue;

    public ParameterValue(string id, Func<object> getValue, Action<object> setValue, Action<ISceneAssistantLayout, ParameterValue> onLayout, bool isParameter = true, object defaultValue = null, Func<bool> condition = null)
    {
        this.Name = id;
        this.getValue = getValue;
        this.setValue = setValue;
        this.OnLayout = onLayout;
        this.IsParameter = isParameter;
        this.defaultValue = defaultValue;
        this.Condition = condition;
    }

    public string GetCommandValue()
    {
        if (Selected && IsParameter && (Condition == null || Condition())) return GetFormattedValue();
        else return null;
    }

    public object GetDefaultValue()
    {
        if (defaultValue != null) return defaultValue;
        else
        {
            var value = Value.GetType();
            if (value.IsValueType) return Activator.CreateInstance(value);
        }
        return default;
    }

    public void DisplayField(ISceneAssistantLayout layout) => OnLayout(layout, this);

    public static string GetFormattedName(string name) => char.ToLower(name[0]) + name.Substring(1);

    public string GetFormattedValue()
    {
        if (Name == "Pos") return GetPosValue();
        else if (Value is Vector2 || Value is Vector3 || Value is Vector4) return Value.ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
        else if (Value is Quaternion quaternion) return quaternion.eulerAngles.ToString("0.##").Replace(" ", "").Replace("(", "").Replace(")", "");
        else if (Value is bool) return Value.ToString().ToLower();
        else if (Value is Color color) return "#" + ColorUtility.ToHtmlStringRGBA(color);
        else if (ValueIsDictionary(Value, out var namedList)) return namedList;
        else if (Value is Texture texture && texture is null) return string.Empty;
        else return Value?.ToString();
    }

    private string GetPosValue()
    {
        var config = Engine.GetConfiguration<CameraConfiguration>();
        if (Value is Vector3 vector) 
            return new Vector3(config.WorldToSceneSpace(vector).x * 100, config.WorldToSceneSpace(vector).y * 100, vector.z).ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
        else return null;
    }

    private bool ValueIsDictionary(object value, out string result)
    {
        if (IsDictionary(out result)) return true;
        else if (IsList(out result)) return true;
        else return false;

        bool IsDictionary(out string namedValues)
        {
            namedValues = string.Empty;

            if (value != null && value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var namedList = value as Dictionary<string, string>;
                var namedStrings = new List<string>();
                foreach (var item in namedList) namedStrings.Add(item.Key.ToString() + "." + GetFormattedValue());
                namedValues = string.Join(",", namedStrings);
            }

            return !string.IsNullOrEmpty(namedValues);
        }

        bool IsList(out string values)
        {
            values = string.Empty;

            if (value != null && value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                values = string.Join(",", value as List<string>);
            }

            return !string.IsNullOrEmpty(values);
        }

    }
}
