﻿using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using System.Linq;

namespace NaninovelSceneAssistant
{
    public class SpawnData : NaninovelObjectData<ISpawnManager, SpawnConfiguration>, INaninovelObjectData
    {
        public SpawnData(string id)
        {
            this.id = id;
            Initialize();
        }

        protected SpawnedObject Spawned { get => Engine.GetService<SpawnManager>().GetSpawned(Id); }
        public override string Id => id;
        public static string TypeId => "Spawn";
        protected Transform Transform => Spawned.Transform;
        public override GameObject GameObject => Spawned.GameObject;
        protected ISceneAssistantSpawn SpawnSceneAssistant => GameObject.GetComponent<ISceneAssistantSpawn>() ?? null;
        protected override string CommandNameAndId => "spawn " + Id;
        protected bool IsTransformable => SpawnSceneAssistant?.IsTransformable ?? true;

        private string id;
        public override string GetCommandLine(bool inlined = false, bool paramsOnly = false)
        {
            if (CommandParameters == null)
            {
                Debug.LogWarning("No parameters found.");
                return null;
            }

            var tempParams = CommandParameters.ToList();
            var transformString = string.Empty;

            if (IsTransformable)
            {
                foreach (var parameter in tempParams.ToList())
                {
                    if (parameter.Name == "Position" || parameter.Name == "Pos" || parameter.Name == "Rotation" || parameter.Name == "Scale")
                    {
                        if (parameter.Selected & parameter.GetCommandValue() != null) transformString = transformString + parameter.GetCommandValue() + " ";
                        tempParams.Remove(parameter);
                    }
                }
            }

            var paramsString = string.Join(",", tempParams.Where(p => p.GetCommandValue() != null).Select(p => p.GetCommandValue(paramOnly: true) ?? string.Empty));
            if ((CommandParameterData.ExcludeState || CommandParameterData.ExcludeDefault) && paramsString.Length == 0 && transformString.Length == 0) return null;
            if (paramsOnly) return paramsString;
            var commandString = CommandNameAndId + " " + transformString + (paramsString.Length != 0 ? "params:" + paramsString : string.Empty);
            return inlined ? $"[{commandString}]" : $"@{commandString}";
        }


        public virtual string GetSpawnEffectLine(bool inlined = false, bool paramsOnly = false)
        {
            if (CommandParameters == null)
            {
                Debug.LogWarning("No parameters found.");
                return null;
            }

            var paramString = string.Join(" ", CommandParameters.Where(p => p.Selected && p.Name != "Params").Select(p => p.GetCommandValue()));
            if (paramsOnly) return paramString;
            var commandString = $"{SpawnSceneAssistant.CommandId} {paramString}";
            return inlined ? $"[{commandString}]" : $"@{commandString}";
        }

        protected void AddTransformParams()
        {
            ICommandParameterData pos = null;
            ICommandParameterData position = null;

            CommandParameters.Add(position = new CommandParameterData<Vector3>("Position", () => Transform.localPosition, v => Transform.localPosition = v, (i, p) => i.Vector3Field(p, toggleGroup: pos), defaultValue: new Vector3(0, 0, 99)));
            CommandParameters.Add(pos = new CommandParameterData<Vector3>("Pos", () => Transform.localPosition, v => Transform.localPosition = v, (i, p) => i.PosField(p, CameraConfiguration, toggleGroup: position), defaultValue: new Vector3(0, 0, 99)));
            CommandParameters.Add(new CommandParameterData<Vector3>("Rotation", () => Transform.localRotation.eulerAngles, v => Transform.localRotation = Quaternion.Euler(v), (i, p) => i.Vector3Field(p)));
            CommandParameters.Add(new CommandParameterData<Vector3>("Scale", () => Transform.localScale, v => Transform.localScale = v, (i, p) => i.Vector3Field(p), defaultValue: Vector3.one));
        }

        protected override void AddCommandParameters()
        {
            if (IsTransformable) AddTransformParams();
            if (SpawnSceneAssistant?.GetParams() != null) CommandParameters.Add(new ListCommandData("Params", SpawnSceneAssistant.GetParams(), (i, p) => i.ListField(p)));
        }
    }

    public interface ISceneAssistantSpawn : ISceneAssistantObject
    {
        bool IsTransformable { get; }
        string SpawnId { get; }
    }

    public interface ISceneAssistantObject
    {
        string CommandId { get; }
        List<ICommandParameterData> GetParams();
    }
}