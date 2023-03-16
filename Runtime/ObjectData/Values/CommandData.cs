using System;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using System.Linq;
using Codice.CM.Common.Replication;
using UnityEditor;

namespace NaninovelSceneAssistant
{
    public interface ICommandData
    {
        string Name { get; }
        bool Selected { get; set; }
        Func<bool> HasCondition { get; }

        bool Changed { get; set; }
        string GetCommandValue(bool paramOnly = false);
        void GetLayout(ISceneAssistantLayout layout);
        void ResetDefault();
        void ResetState();
    }

    public abstract class CommandData
    {
        public CommandData(string name, Func<bool> getCondition = default)
        {
            Name = name;
            HasCondition = getCondition;
        }
        public string Name { get; }
        public bool Selected { get; set; } = true;
        public Func<bool> HasCondition { get; }
        public abstract string GetCommandValue(bool paramOnly = false);
        public abstract void GetLayout(ISceneAssistantLayout layout);
        public abstract void ResetDefault();
        public abstract void ResetState();

        public bool Changed { get; set; }
    }

    public interface ICommandData<T> : ICommandData
    {
        T Value { get; set; }
        T State { get; }
        T Default { get; }
    }

    public class CommandData<T> : CommandData, ICommandData<T>, IDisposable
    {
        public T Value { get => getValue();  set { if(CanUpdate) setValue(value); } }
        public T State { get; private set; }
        public T Default { get => defaultValue; }

        protected Action<ISceneAssistantLayout, ICommandData<T>> getLayout;
        protected Func<T> getValue;
        protected Action<T> setValue;
        protected T defaultValue;
        protected IScriptPlayer ScriptPlayer;
        protected IStateManager StateManager;
        protected static bool CanUpdate;

        public CommandData(string name, Func<T> getValue, Action<T> setValue, 
        Action<ISceneAssistantLayout, ICommandData<T>> getLayout, T defaultValue = default, Func<bool> getCondition = null) : base(name, getCondition) 
        {
            this.getValue = getValue;
            this.setValue = setValue;
            this.getLayout = getLayout;
            this.defaultValue = defaultValue;

            ScriptPlayer = Engine.GetService<IScriptPlayer>();
            StateManager = Engine.GetService<IStateManager>();
            ScriptPlayer.AddPreExecutionTask(HandleCommandStarted);
            ScriptPlayer.AddPostExecutionTask(HandleCommandFinished);
            StateManager.AddOnGameSerializeTask(HandleSerialization);
            State = getValue();
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
            State = getValue();
            CanUpdate = true;
            return UniTask.CompletedTask;
        }

        public override string GetCommandValue(bool paramOnly) => this.GetCommandValue<T>(paramOnly);
        public override void GetLayout(ISceneAssistantLayout layout) => getLayout(layout, this);
        public override void ResetDefault() => Value = Default;
        public override void ResetState() => Value = State;
        public void Dispose()
        {
            // todo set up disposing behaviour 
            if(Name.Equals("Offset")) Debug.Log("Disposed");
            ScriptPlayer.RemovePreExecutionTask(HandleCommandStarted);
            ScriptPlayer.RemovePostExecutionTask(HandleCommandFinished);
        }
    }

}