using Naninovel;
using System;

namespace NaninovelSceneAssistant
{
    public interface IVariableData
    {
        string Name { get; }

        void DisplayField(ICustomVariableLayout layout);
    }

    public abstract class VariableData<T> : IDisposable, IVariableData where T : IConvertible
    {
        public string Name { get; }
        public abstract T Value { get; set; }
        public bool Changed { get; set; }

        protected SceneAssistantManager SceneAssistantManager => Engine.GetService<SceneAssistantManager>();
        protected ICustomVariableManager CustomVariableManager => Engine.GetService<ICustomVariableManager>();


        public VariableData(string name)
        {
            Name = name;
            SceneAssistantManager.OnSceneAssistantReset += HandleReset;
        }

        private void HandleReset()
        {
            //todo get the previous state, not initial value
            Changed = false;
        }

        public abstract void DisplayField(ICustomVariableLayout layout);
        public void Dispose()
        {
            SceneAssistantManager.OnSceneAssistantReset -= HandleReset;
        }
    }

    public class NumericVariableData : VariableData<float>
    {
        public NumericVariableData(string name) : base(name)
        {
            Value = CustomVariableManager.GetVariableValue(name).Number;
        }
        public override float Value { get => CustomVariableManager.GetVariableValue(Name).Number; set => CustomVariableManager.SetVariableValue(Name, new CustomVariableValue(value)); }
        public override void DisplayField(ICustomVariableLayout layout) => layout.NumericVariableField(this);

    }

    public class BooleanVariableData : VariableData<bool>
    {
        public BooleanVariableData(string name) : base(name)
        {
            Value = CustomVariableManager.GetVariableValue(name).Boolean;
        }

        public override bool Value { get => CustomVariableManager.GetVariableValue(Name).Boolean; set => CustomVariableManager.SetVariableValue(Name, new CustomVariableValue(value)); }
        public override void DisplayField(ICustomVariableLayout layout) => layout.BooleanVariableField(this);

    }

    public class StringVariableData : VariableData<string>
    {
        public StringVariableData(string name) : base(name)
        {
            Value = CustomVariableManager.GetVariableValue(name).String;
        }

        public override string Value { get => CustomVariableManager.GetVariableValue(Name).String; set => CustomVariableManager.SetVariableValue(Name, new CustomVariableValue(value)); }
        public override void DisplayField(ICustomVariableLayout layout) => layout.StringVariableField(this);

    }
}
