
using Naninovel;
using NaninovelSceneAssistant;
using System;

public class VariableValue
{
    public string Name { get; }
    public string Value { get => getValue(); set => setValue(value); }

    private Func<string> getValue;
    private Action<string> setValue;

    private ICustomVariableManager customVariableManager;

    public VariableValue(string name)
    {
        Name = name;
        customVariableManager = Engine.GetService<ICustomVariableManager>();
        getValue = () => customVariableManager.GetVariableValue(Name);
        setValue = (value) => customVariableManager.SetVariableValue(Name, value);
    }

    public void DisplayField(ISceneAssistantLayout layout) => layout.VariableField(this);
}