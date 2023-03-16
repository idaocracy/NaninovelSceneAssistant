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
        string GetCommandLine(bool inlined = false, bool paramsOnly = false);
        List<string> GetAllCommands(Dictionary<string, INaninovelObjectData> objectList, Dictionary<Type, bool> objectTypeList, bool inlined = false, bool selected = false);
        List<ICommandData> Params { get; }
        SortedList<string, VariableValue> CustomVars { get; }
    }

    public abstract class NaninovelObjectData<TService, TConfig> : INaninovelObjectData, IDisposable 
        where TService : class, IEngineService
        where TConfig : Configuration
    {
        protected virtual void Initialize()
        {
            stateManager = Engine.GetService<IStateManager>();
            AddParams();
        }

        protected TService Service { get => Engine.GetService<TService>(); }
        protected TConfig Config { get => Engine.GetConfiguration<TConfig>(); }
        public abstract string Id { get; }
        public abstract GameObject GameObject { get; }
        public virtual List<ICommandData> Params { get; protected set; } = new List<ICommandData>();
        public virtual SortedList<string, VariableValue> CustomVars { get; protected set; } = new SortedList<string, VariableValue>();
        protected abstract string CommandNameAndId { get; }
        protected abstract void AddParams();

        protected IStateManager stateManager;

        public virtual string GetCommandLine(bool inlined = false, bool paramsOnly = false)
        {
            if (Params == null)
            {
                Debug.LogWarning("No parameters found.");
                return null; 
            }

            var paramString = string.Join(" ", Params.Where(p => p.GetCommandValue() != null).Select(p => p.GetCommandValue()));
            if (paramsOnly) return paramString;
            var commandString = CommandNameAndId + " " + paramString;

            return inlined ? "[" + commandString + "]" : "@" + commandString;
        }

        public List<string> GetAllCommands(Dictionary<string, INaninovelObjectData> objectList, Dictionary<Type,bool> objectTypeList, bool inlined = false, bool selectedOnly = false)
        {
            var list = new List<string>();

            foreach (var obj in objectList.Values)
            {
                if (selectedOnly && !objectTypeList[obj.GetType()]) continue;
                list.Add(obj.GetCommandLine(inlined));
            }

            return list;
        }

        public void Dispose()
        {
            foreach(var data in Params)
            {
                if (data is IDisposable disposable) disposable.Dispose();
            }
        }
    }

}

