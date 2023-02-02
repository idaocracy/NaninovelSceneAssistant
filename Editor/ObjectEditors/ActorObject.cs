using Naninovel;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace NaninovelSceneAssistant
{
    public abstract class ActorObject<TActor, TMetadata> : NaninovelObject where TActor : IActor where TMetadata : ActorMetadata
    {
        public ActorObject(TActor actor, TMetadata metadata)
        {
            Actor = actor;
            Metadata = metadata;
            AddParams();
        }

        protected TActor Actor { get; private set; }
        protected TMetadata Metadata { get; private set; }
        public override string Id { get => Actor.Id; }
        public override GameObject GameObject => GetGameObject();

        protected GameObject GetGameObject()
        {
            var monoActor = Actor as MonoBehaviourActor<TMetadata>;
            return monoActor.GameObject;
        }

        protected virtual void AddBaseParams(bool includeAppearance = true, bool includeColor = true, bool includeTransform = true)  
        {
   
            if(includeAppearance) Params.Add(new Param { Id = "Appearance", Value = Actor.Appearance, OnEditor = () => Actor.Appearance = EditorGUILayout.DelayedTextField(Actor.Appearance) });
            if(includeColor) Params.Add(new Param { Id = "Tint", Value = Actor.TintColor, OnEditor = () => Actor.TintColor = EditorGUILayout.ColorField(Actor.TintColor) });

            if (includeTransform)
            {
                Params.Add(new Param { Id = "Position", Value = Actor.Position, OnEditor = () => Actor.Position = EditorGUILayout.Vector3Field("", Actor.Position) });
                Params.Add(new Param { Id = "Rotation", Value = Actor.Rotation, OnEditor = () => Actor.Rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("", Actor.Rotation.eulerAngles)) });
                Params.Add(new Param { Id = "Scale", Value = Actor.Scale, OnEditor = () => Actor.Scale = EditorGUILayout.Vector3Field("", Actor.Scale) });
            }
        }
    }


    public class CharacterObject : ActorObject<ICharacterActor, CharacterMetadata>
    {
        public CharacterObject(ICharacterActor actor, CharacterMetadata metadata) : base(actor, metadata) { }
        protected override string GetCommandNameAndId() => "char " + Id;
        protected override void AddParams()
        {
            AddBaseParams();
            Params.Add(new Param { Id = "Look Direction", Value = Actor.LookDirection, OnEditor = () => Actor.LookDirection = SceneAssistantHelpers.EnumField<CharacterLookDirection>(Actor.LookDirection) });
        }
    }

    public class BackgroundObject : ActorObject<IBackgroundActor, BackgroundMetadata>
    {
        public BackgroundObject(IBackgroundActor actor, BackgroundMetadata metadata) : base(actor, metadata) { }
        protected override string GetCommandNameAndId() => "back " + "id:" + Id;
        protected override void AddParams()
        {
            AddBaseParams();
        }
    }

    public class TextPrinterObject : ActorObject<ITextPrinterActor, TextPrinterMetadata>
    {
        public TextPrinterObject(ITextPrinterActor actor, TextPrinterMetadata metadata) : base(actor, metadata) { }
        protected override string GetCommandNameAndId() => "printer " + Id;
        protected override void AddParams()
        {
            AddBaseParams();
        }
    }

    public class ChoiceHandlerObject : ActorObject<IChoiceHandlerActor, ChoiceHandlerMetadata>
    {
        public ChoiceHandlerObject(IChoiceHandlerActor actor, ChoiceHandlerMetadata metadata) : base(actor, metadata) { }
        protected override string GetCommandNameAndId() => "choice ";
        protected override void AddParams()
        {

        }
    }

}