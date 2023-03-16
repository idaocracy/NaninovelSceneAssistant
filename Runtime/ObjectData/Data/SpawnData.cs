using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using System.Linq;
using Naninovel.Metadata;
using UnityEngine.UIElements;
using Naninovel.Commands;
using Naninovel.Parsing;

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
        protected CameraConfiguration CameraConfiguration { get => Engine.GetConfiguration<CameraConfiguration>(); }

        private string id;

        public override string GetCommandLine(bool inlined = false, bool paramsOnly = false)
        {
            if (Params == null)
            {
                Debug.LogWarning("No parameters found.");
                return null;
            }

            var tempParams = Params.ToList();
            var tempString = string.Empty;

            if (IsTransformable)
            {
                foreach (var parameter in tempParams)
                {
                    if(parameter.Name == "Position" || parameter.Name == "Pos" || parameter.Name == "Rotation" || parameter.Name == "Scale")
                    {
                        if (!parameter.Selected) continue;
                        tempString = tempString + parameter.GetCommandValue() + " ";
                        tempParams.Remove(parameter);
                    }
                }
            }

            var paramsString = string.Join(",", tempParams.Where(p => p.GetCommandValue() != null).Select(p => p.GetCommandValue(paramOnly:true) ?? string.Empty));

            if (paramsOnly) return paramsString;

            var commandString = $"{CommandNameAndId} {tempString} params:{paramsString}";
            return inlined ? $"[{commandString}]" : $"@{commandString}";
        }

        public virtual string GetSpawnEffectLine(bool inlined = false, bool paramsOnly = false)
        {
            if (Params == null)
            {
                Debug.LogWarning("No parameters found.");
                return null;
            }

            var paramString = string.Join(" ", Params.Where(p => p.Selected && p.Name != "Params").Select(p => p.GetCommandValue()));
            if (paramsOnly) return paramString;
            var commandString = $"{SpawnSceneAssistant.CommandId} {paramString}";

            return inlined ? $"[{commandString}]" : $"@{commandString}";
        }

        protected void AddTransformParams()
        {
            ICommandData pos = null;
            ICommandData position = null;

            Params.Add(position = new CommandData<Vector3>("Position", () => Transform.localPosition, v => Transform.localPosition = v, (i, p) =>  i.Vector3Field(p, toggleWith: pos)));
            Params.Add(pos = new CommandData<Vector3>("Pos", () => Transform.localPosition, v => Transform.localPosition = v,  (i, p) => i.PosField(p, CameraConfiguration, toggleWith: position)));
            Params.Add(new CommandData<Vector3>("Rotation", () => Transform.localRotation.eulerAngles, v => Transform.localRotation = Quaternion.Euler(v), (i,p) => i.Vector3Field(p)));
            Params.Add(new CommandData<Vector3>("Scale", () => Transform.localScale, v => Transform.localScale = v, (i, p) => i.Vector3Field(p)));
        }

        protected override void AddParams()
        {
            if (SpawnSceneAssistant == null || SpawnSceneAssistant.IsTransformable) AddTransformParams();

            if (SpawnSceneAssistant?.GetParams() != null)
            {
                Params.Add(new ListCommandData("Params", SpawnSceneAssistant.GetParams(), (i, p) => i.ListButtonField(p)));
            }
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
        List<ICommandData> GetParams();
    }
}