using Naninovel;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NaninovelSceneAssistant
{
    public abstract class ActorObject<TActor, TMetadata> : NaninovelObject where TActor : IActor where TMetadata : ActorMetadata  
    {
        public TActor Actor { get; }
        public TMetadata Metadata { get; }
        public override string Id { get; set; }

        public ActorObject(TActor actor, TMetadata metadata) : base() 
        {
            Actor = actor;
            Metadata = metadata;
            Id = actor.Id;
        }

        public override GameObject GetGameObject()
        {
            var monoActor = Actor as MonoBehaviourActor<TMetadata>;
            return monoActor.GameObject;
        }

        protected virtual void AddBaseProperties(List<Param> paramList, bool includeAppearance = true, bool includeColor = true, bool includeTransform = true)  
        {
   
            if(includeAppearance) paramList.Add(new Param { Id = "Appearance", Value = Actor.Appearance, OnEditor = () => Actor.Appearance = EditorGUILayout.DelayedTextField(Actor.Appearance) });
            if(includeColor) paramList.Add(new Param { Id = "Tint", Value = Actor.TintColor, OnEditor = () => Actor.TintColor = EditorGUILayout.ColorField(Actor.TintColor) });

            if (includeTransform)
            {
                paramList.Add(new Param { Id = "Position", Value = Actor.Position, OnEditor = () => Actor.Position = EditorGUILayout.Vector3Field("", Actor.Position) });
                paramList.Add(new Param { Id = "Rotation", Value = Actor.Rotation.eulerAngles, OnEditor = () => Actor.Rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("", Actor.Rotation.eulerAngles)) });
                paramList.Add(new Param { Id = "Scale", Value = Actor.Scale, OnEditor = () => Actor.Scale = EditorGUILayout.Vector3Field("", Actor.Scale) });
            }

        }

    }


    public class CharacterObject : ActorObject<ICharacterActor, CharacterMetadata>
    {
        public override List<Param> Params => paramList;

        private List<Param> paramList = new List<Param>();

        public CharacterObject(ICharacterActor actor, CharacterMetadata metadata) : base(actor, metadata)
        {
            InitializeParams();
        }

        public override string GetCommandNameAndId() => "char " + Id;

        protected override void InitializeParams()
        {
            AddBaseProperties(paramList);
            paramList.Add(new Param { Id = "Look Direction", Value = Actor.LookDirection, OnEditor = () => Actor.LookDirection = SceneAssistantHelpers.EnumField<CharacterLookDirection>(Actor.LookDirection) });
        }
    }

}