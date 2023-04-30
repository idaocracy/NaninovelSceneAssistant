﻿using System;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public interface ICommandParameterData
    {
        string Name { get; }
        bool Selected { get; set; }
        Func<bool> HasCondition { get; }

        string GetCommandValue(bool paramOnly = false);
        void GetLayout(ISceneAssistantLayout layout);
        void ResetDefault();
        void ResetState();
    }

    public abstract class CommandParameterData
    {
        public CommandParameterData(string name, Func<bool> getCondition = default)
        {
            Name = name;
            HasCondition = getCondition;

            ScriptPlayer = Engine.GetService<IScriptPlayer>();
            StateManager = Engine.GetService<IStateManager>();
        }
        public string Name { get; }
        public virtual bool Selected { get; set; } = true;
        public Func<bool> HasCondition { get; }
        public abstract string GetCommandValue(bool paramOnly = false);
        public abstract void GetLayout(ISceneAssistantLayout layout);
        public abstract void ResetDefault();
        public abstract void ResetState();

        public static bool ExcludeState;

        protected IScriptPlayer ScriptPlayer;
        protected IStateManager StateManager;
    }

    public interface ICommandParameterData<T> : ICommandParameterData
    {
        T Value { get; set; }
        T State { get; }
        T Default { get; }
    }

    public class CommandParameterData<T> : CommandParameterData, ICommandParameterData<T>, IDisposable
    {
        public T Value { 
            get => getValue();  
            set 
            { 
                if((CanUpdate && ScriptPlayer.Playing) || !ScriptPlayer.Playing) setValue(value); 
            } 
        }
        public T State { get; private set; }
        public T Default { get; }

        protected Func<T> getValue;
        protected Action<T> setValue;
        protected Action<ISceneAssistantLayout, ICommandParameterData<T>> getLayout;

        protected static bool CanUpdate;

        public CommandParameterData(string name, Func<T> getValue, Action<T> setValue, Action<ISceneAssistantLayout, ICommandParameterData<T>> getLayout, 
            T defaultValue = default, Func<bool> getCondition = null) : base(name, getCondition) 
        {
            this.getValue = getValue;
            this.setValue = setValue;
            this.getLayout = getLayout;
            this.Default = defaultValue;

            ScriptPlayer.AddPreExecutionTask(HandleCommandStarted);
            ScriptPlayer.AddPostExecutionTask(HandleCommandFinished);
            StateManager.AddOnGameSerializeTask(HandleSerialization);
            State = Value;
        }

        private void HandleSerialization(GameStateMap stateMap)
        {
            ResetState();
        }

        private UniTask HandleCommandStarted(Command command)
        {
            CanUpdate = false;
            return UniTask.CompletedTask;
        }

        private UniTask HandleCommandFinished(Command command)
        {
            State = Value;
            CanUpdate = true;
            return UniTask.CompletedTask;
        }

        public override string GetCommandValue(bool paramOnly) => this.GetCommandValue<T>(paramOnly);
        public override void GetLayout(ISceneAssistantLayout layout) => getLayout(layout, this);
        public override void ResetDefault() => Value = Default;
        public override void ResetState() => Value = State;
        public void Dispose()
        {
            ScriptPlayer.RemovePreExecutionTask(HandleCommandStarted);
            ScriptPlayer.RemovePostExecutionTask(HandleCommandFinished);
            StateManager.RemoveOnGameSerializeTask(HandleSerialization);
        }
    }
}