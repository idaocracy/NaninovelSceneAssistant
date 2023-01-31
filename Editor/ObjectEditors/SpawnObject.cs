using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public class SpawnObject<TSpawn> : NaninovelObject, INaninovelObject where TSpawn : SpawnedObject
    {
        private SpawnedObject spawn;
        private Transform spawnTransform;
        private GameObject spawnObject;
        private ISceneAssistantSpawnObject spawnSceneAssistant;

        public SpawnObject(SpawnedObject spawn) : base()
        {
            Id = spawn.Path;
            this.spawn = spawn;
            spawnTransform = spawn.Transform;
            spawnObject = spawn.GameObject;
            spawnSceneAssistant = spawnObject.GetComponent<ISceneAssistantSpawnObject>() ?? null;
        }

        public override string Id { get; set; }
        public override List<Param> Params => spawnSceneAssistant.Params;

        public override string GetCommandNameAndId() => "spawn " + spawnSceneAssistant?.GetSpawnId();
        public override GameObject GetGameObject() => spawnObject;

        protected override void InitializeParams() => spawnSceneAssistant?.InitializeParams();

    }

}