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
    public class SpawnData : NaninovelObjectData<SpawnManager>, INaninovelObjectData  
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
        protected ISceneAssistantSpawn spawnSceneAssistant => GameObject.GetComponent<ISceneAssistantSpawn>() ?? null; 
        protected override string CommandNameAndId => "spawn " + Id;
        protected bool IsTransformable => spawnSceneAssistant?.IsTransformable ?? true;
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
                        tempString = tempString + ParameterValue.GetFormattedName(parameter.Name) + ":" + parameter.GetCommandValue() + " ";
                        tempParams.Remove(parameter);
                    }
                }
            }

            var paramsString = string.Join(",", tempParams.Where(p => p.GetCommandValue() != null).Select(p => p.GetCommandValue() ?? string.Empty));

            if (paramsOnly) return paramsString;

            var commandString = CommandNameAndId + " " + tempString + "params:" + paramsString;
            return inlined ? "[" + commandString + "]" : "@" + commandString;
        }

        public virtual string GetSpawnEffectLine(bool inlined = false, bool paramsOnly = false)
        {
            if (Params == null)
            {
                Debug.LogWarning("No parameters found.");
                return null;
            }

            var paramString = string.Join(" ", Params.Where(p => p.Selected && p.Name != "Params").Select(p => ParameterValue.GetFormattedName(p.Name) + ":" + p.GetFormattedValue()));
            if (paramsOnly) return paramString;
            var commandString = ParameterValue.GetFormattedName(spawnSceneAssistant.CommandId) + " " + paramString;

            return inlined ? "[" + commandString + "]" : "@" + commandString;
        }


        protected void AddTransformParams()
        {
            ParameterValue pos = null;
            ParameterValue position = null;

            Params.Add(position = new ParameterValue("Position", () => Transform.localPosition, v => Transform.localPosition = (Vector3)v, (i, p) =>  i.Vector3Field(p, toggleWith: pos)));
            Params.Add(pos = new ParameterValue("Pos", () => Transform.localPosition, v => Transform.localPosition = (Vector3)v, (i, p) => i.PosField(p, CameraConfiguration, toggleWith: position)));
            Params.Add(new ParameterValue("Rotation", () => Transform.localRotation.eulerAngles, v => Transform.localRotation = Quaternion.Euler((Vector3)v), (i,p) => i.Vector3Field(p)));
            Params.Add(new ParameterValue("Scale", () => Transform.localScale, v => Transform.localScale = (Vector3)v, (i, p) => i.Vector3Field(p)));
        }

        protected override void AddParams()
        {
            if(spawnSceneAssistant == null || spawnSceneAssistant.IsTransformable) AddTransformParams();

            if (spawnSceneAssistant?.GetParams() != null)
            {
                Params.Add(new ParameterValue("Params", () => string.Join(",", spawnSceneAssistant.GetParams().Where(p => p.Condition == null || p.Condition()).Select(p => p.GetFormattedValue()).ToList()), v => { }, (i,p) => i.EmptyField(p)));
                Params = Params.Concat(spawnSceneAssistant.GetParams()).ToList();
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
        List<ParameterValue> GetParams();
    }
}