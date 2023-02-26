using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using System.Linq;

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
        protected Transform Transform => Spawned.Transform; 
        public override GameObject GameObject => Spawned.GameObject; 
        protected ISceneAssistantSpawn spawnSceneAssistant => GameObject.GetComponent<ISceneAssistantSpawn>() ?? null; 
        protected override string CommandNameAndId => "spawn " + Id;
        protected bool IsTransformable => spawnSceneAssistant?.IsTransformable ?? true;

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
                for (int i = 0; i <= 2; i++) tempString = tempString + tempParams[i].Name.ToLower() + ":" + tempParams[i].Value + " ";
                tempParams.RemoveRange(0, 3);
            }

            var paramsString = string.Join(",", tempParams.Select(p => p.GetCommandValue().ToString() ?? string.Empty));

            if (paramsOnly) return paramsString;
            var commandString = CommandNameAndId + " params:" + paramsString; 

            return inlined ? "[" + commandString + "]" : "@" + commandString;
        }

        public virtual string GetSpawnEffectLine(bool inlined = false, bool paramsOnly = false)
        {
            if (Params == null)
            {
                Debug.LogWarning("No parameters found.");
                return null;
            }

            var paramString = string.Join(" ", Params.Where(p => p.GetCommandValue() != null
                && p.Selected && p.IsParameter).Select(p => p.Name.ToLower() + ":" + p.GetCommandValue()));

            if (paramsOnly) return paramString;

            var commandString = spawnSceneAssistant.CommandId + " " + paramString;

            return inlined ? "[" + commandString + "]" : "@" + commandString;
        }


        protected void AddTransformParams()
        {
            Params.Add(new ParameterValue("Position", () => Transform.localPosition, v => Transform.localPosition = (Vector3)v, (i,p) => i.Vector3Field(p)));
            Params.Add(new ParameterValue("Rotation", () => Transform.localRotation.eulerAngles, v => Transform.localRotation = Quaternion.Euler((Vector3)v), (i,p) => i.Vector3Field(p)));
            Params.Add(new ParameterValue("Scale", () => Transform.localScale, v => Transform.localScale = (Vector3)v, (i, p) => i.Vector3Field(p)));
        }

        protected override void AddParams()
        {
            if(spawnSceneAssistant == null || spawnSceneAssistant.IsTransformable) AddTransformParams();

            if (spawnSceneAssistant?.GetParams() != null)
            {
                Params.Add(new ParameterValue("Params", () => string.Join(",", spawnSceneAssistant.GetParams().Select(p => p.GetCommandValue()).ToList()), v => { }, (i,p) => i.EmptyField(p)));
                Params = Params.Concat(spawnSceneAssistant.GetParams()).ToList();
            }
        }
    }

    public interface ISceneAssistantSpawn : ISceneAssistantObject
    {
        bool IsTransformable { get; }
    }

    public interface ISceneAssistantObject
    {
        string CommandId { get; }
        List<ParameterValue> GetParams();
    }
}