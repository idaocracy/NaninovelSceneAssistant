using System;
using Naninovel;
using UnityEngine;

namespace NaninovelSceneAssistant
{
	public interface ICommandParameterData
	{
		string Name { get; }
		bool Selected { get; set; }
		Func<bool>[] Conditions { get; }
		string GetCommandValue(bool paramOnly = false);
		void DrawLayout(ISceneAssistantLayout layout);
		void ResetDefault();
		void ResetState();
	}

	public abstract class CommandParameterData 
	{
		public CommandParameterData(string name, params Func<bool>[] getCondition)
		{
			Name = name;
			Conditions = getCondition;
			StateManager = Engine.GetService<IStateManager>();
		}

		public string Name { get; }
		public virtual bool Selected { get; set; } = true;
		public Func<bool>[] Conditions { get; }
		public abstract string GetCommandValue(bool paramOnly = false);
		public abstract void DrawLayout(ISceneAssistantLayout layout);
		public abstract void ResetDefault();
		public abstract void ResetState();
		public static bool ExcludeState;
		public static bool ExcludeDefault;

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
			set => setValue(value);
		}
		private readonly Func<T> getValue;
		private readonly Action<T> setValue;

		public T State { get; private set; }
		public T Default { get; }

		private readonly Action<ISceneAssistantLayout, ICommandParameterData<T>> getLayout;

		public CommandParameterData(string name, Func<T> getValue, Action<T> setValue, Action<ISceneAssistantLayout, ICommandParameterData<T>> getLayout, 
			T defaultValue = default, params Func<bool>[] conditions) : base(name, conditions) 
		{
			this.getValue = getValue;
			this.setValue = setValue;
			this.getLayout = getLayout;
			this.Default = defaultValue;

			State = Value;
			StateManager.AddOnGameSerializeTask(HandleSerialization);
		}

		private void HandleSerialization(GameStateMap stateMap)
		{
			ResetState();
		}

		public override string GetCommandValue(bool paramOnly) => this.GetCommandValue<T>(paramOnly);
		public override void DrawLayout(ISceneAssistantLayout layout) => getLayout(layout, this);
		public override void ResetDefault() => Value = Default;
		public override void ResetState() 
		{
			if(State != null && !Value.Equals(State)) Value = State;
		}

		public void Dispose() 
		{
			StateManager.RemoveOnGameSerializeTask(HandleSerialization);
		} 
	}
}