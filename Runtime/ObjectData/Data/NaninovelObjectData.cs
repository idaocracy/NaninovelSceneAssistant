using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Naninovel;

namespace NaninovelSceneAssistant {
	public interface INaninovelObjectData
	{
		string Id { get; }
		GameObject GameObject { get; }
		string GetCommandLine( bool inlined = false, bool paramsOnly = false);
		List<string> GetAllCommands(Dictionary<string, INaninovelObjectData> objectList, Dictionary<Type, bool> objectTypeList,  bool inlined = false, bool selected = false);
		List<ICommandParameterData> CommandParameters { get; }
		SortedList<string, VariableData> CustomVariables { get; }
	}

	//This interface is designed for objects which has more data added throughout its lifetime (like choice handler)
	public interface IDynamicCommandParameter
	{
		void UpdateCommandParameters();
	}

	public abstract class NaninovelObjectData<TService, TConfig> : INaninovelObjectData, IDisposable 
		where TService : class, IEngineService
		where TConfig : Configuration
	{
		protected virtual void Initialize()
		{
			AddCommandParameters();
		}

		protected TService Service { get => Engine.GetService<TService>(); }
		protected TConfig Config { get => Engine.GetConfiguration<TConfig>(); }
		public abstract string Id { get; }
		public abstract GameObject GameObject { get; }
		public virtual List<ICommandParameterData> CommandParameters { get; protected set; } = new List<ICommandParameterData>();
		public virtual SortedList<string, VariableData> CustomVariables { get; protected set; } = new SortedList<string, VariableData>();
		protected abstract string CommandNameAndId { get; }
		protected abstract void AddCommandParameters();
		protected CameraConfiguration CameraConfiguration => Engine.GetConfiguration<CameraConfiguration>();

		public virtual string GetCommandLine(bool inlined = false, bool paramsOnly = false)
		{
			if (CommandParameters == null)
			{
				Debug.LogWarning("No parameters found.");
				return null; 
			}

			var paramString = string.Join(" ", CommandParameters.Where(p => p.GetCommandValue() != null).Select(p => p.GetCommandValue()));
			
			if ((CommandParameterData.ExcludeState || CommandParameterData.ExcludeDefault) && paramString.Length == 0) return null;
			if (paramsOnly) return paramString;
			var commandString = CommandNameAndId + " " + paramString;

			return inlined ? "[" + commandString + "]" : "@" + commandString;
		}

		public List<string> GetAllCommands(Dictionary<string, INaninovelObjectData> objectList, Dictionary<Type,bool> objectTypeList,bool inlined = false, bool selectedOnly = false)
		{
			var list = new List<string>();

			foreach (var obj in objectList.Values)
			{
				if (selectedOnly && !objectTypeList[obj.GetType()]) continue;
				list.Add(obj.GetCommandLine( inlined));
			}

			return list;
		}

		public void Dispose()
		{
			foreach(var data in CommandParameters)
			{
				if (data is IDisposable disposable) disposable.Dispose();
			}
		}
	}
}