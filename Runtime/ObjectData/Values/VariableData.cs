using Naninovel;
using NaninovelSceneAssistant;
using System;

public class VariableData : IDisposable
{
    public string Name { get; }
    public string Value { get => getValue(); set => setValue(value); }
    public bool Changed { get; set; }
    //public string State { get; set; }

    private readonly Func<string> getValue;
    private readonly Action<string> setValue;

    protected SceneAssistantManager SceneAssistantManager;

    public VariableData(string name)
    {
        Name = name;
        
        var customVariableManager = Engine.GetService<ICustomVariableManager>();
        getValue = () => customVariableManager.GetVariableValue(Name);
        setValue = (value) => customVariableManager.SetVariableValue(Name, value);

        //State = Value;

        SceneAssistantManager = Engine.GetService<SceneAssistantManager>();
        SceneAssistantManager.OnSceneAssistantReset += HandleReset;
    }

    private void HandleReset()
    {
        //todo get the previous state, not initial value
        Changed = false;
    }
    
    public void DisplayField(ICustomVariableLayout layout) => layout.VariableField(this);
    public void Dispose()
    {
        SceneAssistantManager.OnSceneAssistantReset -= HandleReset;
    }
}