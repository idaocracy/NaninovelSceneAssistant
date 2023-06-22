using Naninovel;
using NaninovelSceneAssistant;
using System;

public class VariableData
{
    public string Name { get; }
    public string Value { get => getValue(); set => setValue(value); }

    private Func<string> getValue;
    private Action<string> setValue;
    private ICustomVariableManager customVariableManager;

    public VariableData(string name)
    {
        Name = name;
        customVariableManager = Engine.GetService<ICustomVariableManager>();
        getValue = () => customVariableManager.GetVariableValue(Name);
        setValue = (value) => customVariableManager.SetVariableValue(Name, value);
    }

    public void DisplayField(ICustomVariableLayout layout) => layout.VariableField(this);
}