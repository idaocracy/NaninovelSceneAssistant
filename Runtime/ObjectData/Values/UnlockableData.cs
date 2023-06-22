using Naninovel;
using NaninovelSceneAssistant;
using System;

public class UnlockableData
{
    public string Name { get; }
    public bool Value { get => getValue(); set => setValue(value); }
    public enum UnlockableState { Unlocked, Locked }
    public UnlockableState EnumValue { get => enumValue; set => enumValue = value; }

    private Func<bool> getValue;
    private Action<bool> setValue;
    private UnlockableState enumValue;
    private IUnlockableManager unlockableManager;

    public UnlockableData(string name)
    {
        Name = name;
        unlockableManager = Engine.GetService<IUnlockableManager>();
        getValue = () => unlockableManager.ItemUnlocked(Name);
        setValue = (value) => unlockableManager.SetItemUnlocked(Name, value);
        enumValue = getValue() ? UnlockableState.Unlocked : UnlockableState.Locked;
    }
    public void DisplayField(IUnlockableLayout layout) => layout.UnlockableField(this);
}