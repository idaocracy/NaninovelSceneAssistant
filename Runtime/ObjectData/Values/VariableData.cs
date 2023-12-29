using Naninovel;
using NaninovelSceneAssistant;
using System;

public class VariableData
{
    public string Name { get; }
    public string Value { get => getValue(); set => setValue(value); }

    private readonly Func<string> getValue;
    private readonly Action<string> setValue;

    public VariableData(string name)
    {
        Name = name;
        
        var customVariableManager = Engine.GetService<ICustomVariableManager>();
        getValue = () => customVariableManager.GetVariableValue(Name);
        setValue = (value) => customVariableManager.SetVariableValue(Name, value);
    }

    public void DisplayField(ICustomVariableLayout layout) => layout.VariableField(this);
}