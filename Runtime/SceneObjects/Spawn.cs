using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using System.Linq;

namespace NaninovelSceneAssistant
{
    public class Spawn : NaninovelObject<SpawnManager>, INaninovelObject  
    {
        public Spawn(string id)
        {
            this.id = id;
            Initialize();
        }

        protected SpawnedObject Spawned { get => Engine.GetService<SpawnManager>().GetSpawned(Id); }
        public override string Id => id;
        protected Transform Transform => Spawned.Transform; 
        public override GameObject GameObject => Spawned.GameObject; 
        protected ISceneAssistantSpawnObject spawnSceneAssistant => GameObject.GetComponent<ISceneAssistantSpawnObject>() ?? null; 
        protected override string CommandNameAndId => "spawn " + Id;
        protected bool IsTransformable => spawnSceneAssistant?.IsTransformable ?? true;

        private string id;

        public override string GetCommandLine()
        {
            if (Params == null) return null;

            var tempParams = Params.ToList();
            var tempString = CommandNameAndId + " ";

            if (IsTransformable)
            {
                for (int i = 0; i <= 2; i++) tempString = tempString + tempParams[i].Name.ToLower() + ":" + tempParams[i].Value + " ";
                tempParams.RemoveRange(0, 3);
            }

            return tempString = tempString + " params:"+ string.Join(",", tempParams.Select(p => p.GetCommandValue().ToString()));
        }

        protected void AddTransformParams()
        {
            Params.Add(new CommandParam("Position", () => Transform.localPosition, v => Transform.localPosition = (Vector3)v, (i,p) => i.Vector3Field(p)));
            Params.Add(new CommandParam("Rotation", () => Transform.localRotation.eulerAngles, v => Transform.localRotation = Quaternion.Euler((Vector3)v), (i,p) => i.Vector3Field(p)));
            Params.Add(new CommandParam("Scale", () => Transform.localScale, v => Transform.localScale = (Vector3)v, (i, p) => i.Vector3Field(p)));
        }

        protected override void AddParams()
        {
            if(spawnSceneAssistant == null || spawnSceneAssistant.IsTransformable) AddTransformParams();

            if (spawnSceneAssistant?.GetParams() != null)
            {
                Params.Add(new CommandParam("Params", () => string.Join(",", spawnSceneAssistant.GetParams().Select(p => p.GetCommandValue()).ToList()), v => { }, (i,p) => { }));
                Params = Params.Concat(spawnSceneAssistant?.GetParams()).ToList();
            }
        }
    }

    public interface ISceneAssistantSpawnObject : ISceneAssistantObject
    {
        bool IsTransformable { get; }
    }

    public interface ISceneAssistantObject
    {
        string CommandId { get; }
        List<CommandParam> GetParams();
    }
}