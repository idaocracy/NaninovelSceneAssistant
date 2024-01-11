using Naninovel;
using NaninovelSceneAssistant;
using System;

public class UnlockableData
{
    public string Name { get; }
    public bool Value { get => getValue(); set { if(SceneAssistantManager.IsAvailable) setValue(value); } }
    public enum UnlockableState { Unlocked, Locked }
    public UnlockableState EnumValue { get => enumValue; set => enumValue = value; }

    private readonly Func<bool> getValue;
    private readonly Action<bool> setValue;
    private UnlockableState enumValue;

    protected SceneAssistantManager SceneAssistantManager => Engine.GetService<SceneAssistantManager>();
    
    public UnlockableData(string name)
    {
        Name = name;
        
        var unlockableManager = Engine.GetService<IUnlockableManager>();
        getValue = () => unlockableManager.ItemUnlocked(Name);
        setValue = (value) => unlockableManager.SetItemUnlocked(Name, value);
        enumValue = getValue() ? UnlockableState.Unlocked : UnlockableState.Locked;
    }
    public void DisplayField(IUnlockableLayout layout) => layout.UnlockableField(this);
}