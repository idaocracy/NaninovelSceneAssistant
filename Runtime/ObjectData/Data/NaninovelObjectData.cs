﻿using System;
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
        string GetAllCommands(Dictionary<string, INaninovelObjectData> objectList, Dictionary<Type, bool> objectTypeList, bool selected = false);
        List<ParameterValue> Params { get; }
        SortedList<string, VariableValue> CustomVars { get; }
    }

    public abstract class NaninovelObjectData<TEngineService> : INaninovelObjectData where TEngineService : class, IEngineService
    {
        protected virtual void Initialize()
        {
            AddParams();
        }

        protected static TEngineService EngineService { get => Engine.GetService<TEngineService>(); }
        public abstract string Id { get; }
        public abstract GameObject GameObject { get; }
        public virtual List<ParameterValue> Params { get; protected set; } = new List<ParameterValue>();
        public virtual SortedList<string, VariableValue> CustomVars { get; protected set; } = new SortedList<string, VariableValue>();
        protected abstract string CommandNameAndId { get; }
        protected abstract void AddParams();

        public virtual string GetCommandLine(bool inlined = false, bool paramsOnly = false)
        {
            if (Params == null)
            {
                Debug.LogWarning("No parameters found.");
                return null; 
            }

            var paramString = string.Join(" ", Params.Where(p => p.GetCommandValue() != null).Select(p => p.Name.ToLower() + ":" + p.GetCommandValue()));
            if (paramsOnly) return paramString;
            var commandString = CommandNameAndId + " " + paramString;

            return inlined ? "[" + commandString + "]" : "@" + commandString;
        }

        public string GetAllCommands(Dictionary<string, INaninovelObjectData> objectList, Dictionary<Type,bool> objectTypeList, bool selectedOnly = false)
        {
            var allString = String.Empty;

            foreach (var obj in objectList.Values)
            {
                if (selectedOnly && !objectTypeList[obj.GetType()]) continue;
                else allString = allString + obj.GetCommandLine() + "\n";
            }

            return allString;
        }
    }

}
