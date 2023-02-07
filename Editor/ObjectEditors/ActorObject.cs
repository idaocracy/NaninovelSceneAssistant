using Naninovel;
using UnityEngine;
using UnityEditor;

namespace NaninovelSceneAssistant
{

    public abstract class ActorObject<TService, TActor, TMeta, TConfig> : NaninovelObject<TService>, INaninovelObject
        where TService : class, IActorManager
        where TActor : IActor
        where TMeta : ActorMetadata
        where TConfig : ActorManagerConfiguration<TMeta>
    {
        public ActorObject(string id) : base()
        {
            Id = id;
            Initialize();
            AddVars();
        }

        public override string Id { get; set; } 
        protected TActor Actor { get => (TActor)EngineService.GetActor(Id); }
        protected TMeta Metadata { get => Config.GetMetadataOrDefault(Id); }
        protected TConfig Config { get => Engine.GetConfiguration<TConfig>(); }
        public override GameObject GameObject => GetGameObject();

        protected GameObject GetGameObject()
        {
            var monoActor = Actor as MonoBehaviourActor<TMeta>;
            return monoActor.GameObject;
        }

        protected virtual void AddBaseParams(bool includeAppearance = true, bool includeColor = true, bool includeTransform = true, bool includeZPos = true)  
        {
            //if(includeAppearance) Params.Add(new CommandParam("Appearance", Actor.Appearance, () => Actor.Appearance = EditorGUILayout.DelayedTextField(Actor.Appearance)));
            //if(includeColor) Params.Add(new CommandParam("Tint", Actor.TintColor, () => Actor.TintColor = EditorGUILayout.ColorField(Actor.TintColor)));

            //if (includeTransform)
            //{
            //    Params.Add(new CommandParam ("Position", Actor.Position, () => Actor.Position = includeZPos ? EditorGUILayout.Vector3Field("", Actor.Position) : (Vector3)EditorGUILayout.Vector2Field("", Actor.Position)));
            //    Params.Add(new CommandParam ("Pos", Actor.Position, () => Actor.Position = includeZPos ? EditorGUILayout.Vector3Field("", Actor.Position) : (Vector3)EditorGUILayout.Vector2Field("", Actor.Position)));
            //    Params.Add(new CommandParam ("Rotation", Actor.Rotation, () => Actor.Rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("", Actor.Rotation.eulerAngles))));
            //    Params.Add(new CommandParam ("Scale", Actor.Scale, () => Actor.Scale = EditorGUILayout.Vector3Field("", Actor.Scale)));
            //}
        }

        protected virtual void AddVars()
        {
            foreach(var variable in customVariableManager.GetAllVariables())
            {
                if (variable.Name.Contains(Id))
                {
                    CustomVars.Add(variable.Name, new CustomVar(variable.Name, variable.Value));
                }
            }
        }
    }


    public class CharacterObject : ActorObject<CharacterManager, ICharacterActor, CharacterMetadata, CharactersConfiguration>
    {
        public CharacterObject(string id) : base(id) { }
        protected override string CommandNameAndId => "char " + Id;
        protected override void AddParams()
        {
            AddBaseParams();
            //Params.Add(new CommandParam ("Look", Actor.LookDirection, () => Actor.LookDirection = SceneAssistantHelpers.EnumField<CharacterLookDirection>(Actor.LookDirection)));
        }
    }

    public class BackgroundObject : ActorObject<BackgroundManager, IBackgroundActor, BackgroundMetadata, BackgroundsConfiguration>
    {
        public BackgroundObject(string id) : base(id) { }

        protected override string CommandNameAndId => "back " + "id:" + Id;
        protected override void AddParams()
        {
            AddBaseParams();
        }
    }

    public class TextPrinterObject : ActorObject<TextPrinterManager, ITextPrinterActor, TextPrinterMetadata, TextPrintersConfiguration>
    {
        public TextPrinterObject(string id) : base(id) { }

        protected override string CommandNameAndId => "printer " + Id;
        protected override void AddParams()
        {
            AddBaseParams(includeZPos:false);
        }
    }

    public class ChoiceHandlerObject : ActorObject<ChoiceHandlerManager, IChoiceHandlerActor, ChoiceHandlerMetadata, ChoiceHandlersConfiguration>
    {
        public ChoiceHandlerObject(string id) : base(id) { }
        protected override string CommandNameAndId => "choice ";
        protected override void AddParams()
        {

        }
    }

}