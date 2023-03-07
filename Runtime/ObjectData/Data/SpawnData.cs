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
    public class SpawnData : NaninovelObjectData<ISpawnManager, SpawnManager.GameState, SpawnConfiguration>, INaninovelObjectData
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
        protected SpawnedObjectState SpawnedState => State.SpawnedObjects.FirstOrDefault(s => s.Path == Id);

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
            var commandString = ParameterValue.GetFormattedName(SpawnSceneAssistant.CommandId) + " " + paramString;

            return inlined ? "[" + commandString + "]" : "@" + commandString;
        }


        protected void AddTransformParams()
        {
            ParameterValue pos = null;
            ParameterValue position = null;

            Params.Add(position = new ParameterValue("Position", () => Transform.localPosition, v => Transform.localPosition = (Vector3)v, () => SpawnedState.Position, (i, p) =>  i.Vector3Field(p, toggleWith: pos)));
            Params.Add(pos = new ParameterValue("Pos", () => Transform.localPosition, v => Transform.localPosition = (Vector3)v, () => SpawnedState.Position, (i, p) => i.PosField(p, CameraConfiguration, toggleWith: position)));
            Params.Add(new ParameterValue("Rotation", () => Transform.localRotation.eulerAngles, v => Transform.localRotation = Quaternion.Euler((Vector3)v), () => SpawnedState.Rotation.eulerAngles, (i,p) => i.Vector3Field(p)));
            Params.Add(new ParameterValue("Scale", () => Transform.localScale, v => Transform.localScale = (Vector3)v, () => SpawnedState.Scale, (i, p) => i.Vector3Field(p)));
        }

        protected override void AddParams()
        {
            if(SpawnSceneAssistant == null || SpawnSceneAssistant.IsTransformable) AddTransformParams();

            if (SpawnSceneAssistant?.GetParams() != null)
            {
                Params.Add(new ParameterValue("Params", () => string.Join(",", SpawnSceneAssistant.GetParams().Where(p => p.Condition == null || p.Condition()).Select(p => p.GetFormattedValue()).ToList()), null, null, (i,p) => i.EmptyField(p)));
                Params = Params.Concat(SpawnSceneAssistant.GetParams()).ToList();
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