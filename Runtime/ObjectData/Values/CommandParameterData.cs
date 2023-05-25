using System;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public interface ICommandParameterData
    {
        string Name { get; }
        bool Selected { get; set; }
        Func<bool> HasCondition { get; }
        string GetCommandValue(bool paramOnly = false);
        void DrawLayout(ISceneAssistantLayout layout);
        void ResetDefault();
        void ResetState();
    }

    public abstract class CommandParameterData
    {
        public CommandParameterData(string name, Func<bool> getCondition = default)
        {
            Name = name;
            HasCondition = getCondition;
        }

        public string Name { get; }
        public virtual bool Selected { get; set; } = true;
        public Func<bool> HasCondition { get; }
        public abstract string GetCommandValue(bool paramOnly = false);
        public abstract void DrawLayout(ISceneAssistantLayout layout);
        public abstract void ResetDefault();
        public abstract void ResetState();

        public static bool ExcludeState;

    }

    public interface ICommandParameterData<T> : ICommandParameterData
    {
        T Value { get; set; }
        Func<T> GetValue { get; }
        Action<T> SetValue { get; }
        T State { get; }
        T Default { get; }
    }

    public class CommandParameterData<T> : CommandParameterData, ICommandParameterData<T>, IDisposable
    {

        public T Value { 
            get => GetValue();  
            set => SetValue(value);
        }
        public Func<T> GetValue { get; protected set; }
        public Action<T> SetValue { get; protected set; }

        public T State { get; private set; }
        public T Default { get; }


        private Action<ISceneAssistantLayout, ICommandParameterData<T>> GetLayout;

        public CommandParameterData(string name, Func<T> getValue, Action<T> setValue, Action<ISceneAssistantLayout, ICommandParameterData<T>> getLayout, 
            T defaultValue = default, Func<bool> getCondition = null) : base(name, getCondition) 
        {
            this.GetValue = getValue;
            this.SetValue = setValue;
            this.GetLayout = getLayout;
            this.Default = defaultValue;
            State = Value;
        }

        public override string GetCommandValue(bool paramOnly) => this.GetCommandValue<T>(paramOnly);
        public override void DrawLayout(ISceneAssistantLayout layout) => GetLayout(layout, this);
        public override void ResetDefault() => Value = Default;
        public override void ResetState() => Value = State;
        public void Dispose()
        {
            ResetState();
        }
    }
}