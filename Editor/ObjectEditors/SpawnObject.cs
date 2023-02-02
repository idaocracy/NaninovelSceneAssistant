using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;
using System.Linq;

namespace NaninovelSceneAssistant
{
    public class SpawnObject<TSpawn> : NaninovelObject where TSpawn : SpawnedObject
    {
        public SpawnObject(TSpawn spawn)
        {
            Spawn = spawn;
            AddParams();
        }

        protected TSpawn Spawn { get; private set; }
        public override string Id { get => Spawn.Path; }
        protected Transform Transform { get => Spawn.Transform; }
        public override GameObject GameObject { get => Spawn.GameObject; }
        protected ISceneAssistantSpawnObject spawnSceneAssistant { get => GameObject.GetComponent<ISceneAssistantSpawnObject>(); }
        protected override string GetCommandNameAndId() => "spawn " + spawnSceneAssistant?.GetSpawnId();

        public void AddTransformParams()
        {
            Params.Add(new Param { Id = "Position", Value = Transform.localPosition, OnEditor = () => Transform.localPosition = EditorGUILayout.Vector3Field("", Transform.localPosition) });
            Params.Add(new Param { Id = "Rotation", Value = Transform.localRotation, OnEditor = () => Transform.localRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("", Transform.localRotation.eulerAngles)) });
            Params.Add(new Param { Id = "Scale", Value = Transform.localScale, OnEditor = () => Transform.localScale = EditorGUILayout.Vector3Field("", Transform.localScale) });
        }

        protected override void AddParams()
        {
            if (spawnSceneAssistant.IsTransformable()) AddTransformParams();
            Params.Concat(spawnSceneAssistant.GetParams()).ToList();
        }
    }
}