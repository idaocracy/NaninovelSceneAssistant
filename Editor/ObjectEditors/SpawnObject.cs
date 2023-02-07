using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;
using System.Linq;
using Codice.Utils;

namespace NaninovelSceneAssistant
{
    public class SpawnObject : NaninovelObject<SpawnManager>, INaninovelObject  
    {
        public SpawnObject(string id)
        {
            Id = id;
            Initialize();
        }

        protected SpawnedObject Spawn { get => Engine.GetService<SpawnManager>().GetSpawned(Id); }
        public override string Id { get; set; }
        protected Transform Transform { get => Spawn.Transform; }
        public override GameObject GameObject { get => Spawn.GameObject; }
        protected ISceneAssistantSpawnObject spawnSceneAssistant { get => GameObject.GetComponent<ISceneAssistantSpawnObject>() ?? null; }
        protected override string CommandNameAndId => "spawn " + Id;
        protected bool IsTransformable { get => spawnSceneAssistant?.IsTransformable ?? true; }

        public override string GetCommandLine()
        {
            if (Params == null) return null;

            var tempParams = Params.ToList();
            var tempString = CommandNameAndId + " ";

            if (IsTransformable)
            {
                for (int i = 0; i <= 2; i++) tempString = tempString + tempParams[i].Id.ToLower() + ":" + tempParams[i].GetValue + " ";
                tempParams.RemoveRange(0, 3);
            }

            return tempString = tempString + " params:"+ string.Join(",", tempParams.Select(p => p.GetValue.ToString()));
        }

        protected void AddTransformParams()
        {
            //Params.Add(new CommandParam("Position", Transform.localPosition, () => Transform.localPosition = EditorGUILayout.Vector3Field("", Transform.localPosition)));
            //Params.Add(new CommandParam ("Rotation", Transform.localRotation, () => Transform.localRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("", Transform.localRotation.eulerAngles))));
            //Params.Add(new CommandParam("Scale", Transform.localScale, () => Transform.localScale = EditorGUILayout.Vector3Field("", Transform.localScale)));
        }

        protected override void AddParams()
        {
            if(spawnSceneAssistant == null || spawnSceneAssistant.IsTransformable) AddTransformParams();
            if(spawnSceneAssistant?.GetParams() != null)Params = Params.Concat(spawnSceneAssistant?.GetParams()).ToList();
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