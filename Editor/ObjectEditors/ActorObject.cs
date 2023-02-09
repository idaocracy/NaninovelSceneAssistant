using Naninovel;
using UnityEngine;
using UnityEditor;
using Naninovel.UI;
using System.Collections.Generic;
using System.Linq;

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


        async UniTask<List<string>> GetAppearanceList()
        {
            var resourceProviderManager = Engine.GetService<IResourceProviderManager>();
            var appearanceList = new List<string>();

            foreach (var provider in resourceProviderManager.GetProviders(Metadata.Loader.ProviderTypes))
            {
                var paths = await provider.LocateResourcesAsync<Object>(Metadata.Loader.PathPrefix + "/" + Id);
                foreach (var path in paths) appearanceList.Add(path.Split("/".ToCharArray()).Last());
            }

            return appearanceList;
        }

        protected GameObject GetGameObject()
        {
            var monoActor = Actor as MonoBehaviourActor<TMeta>;
            return monoActor.GameObject;
        }

        protected virtual void AddBaseParams(bool includeAppearance = true, bool includeColor = true, bool includeTransform = true, bool includeZPos = true)  
        {
            if (includeAppearance)
            {
                Params.Add(new CommandParam("Appearance", () => Actor.Appearance, v => Actor.Appearance = (string)v, (i, p) => i.StringListField(p, GetAppearanceList().Result.ToArray())));
                //Params.Add(new CommandParam("Appearance", () => Actor.Appearance, v => Actor.Appearance = (string)v, (i, p) => i.StringField(p)));
            }

            if (includeTransform)
            {
                Params.Add(new CommandParam ("Position", () => Actor.Position, v => Actor.Position = (Vector3)v, (i,p) => i.Vector3Field(p)));
                //not implemented pos
                Params.Add(new CommandParam("Pos", () => Actor.Position, v => Actor.Position = (Vector3)v, (i, p) => i.Vector3Field(p)));
                Params.Add(new CommandParam("Rotation", () => Actor.Rotation.eulerAngles, v => Actor.Rotation = Quaternion.Euler((Vector3)v), (i, p) => i.Vector3Field(p)));
                Params.Add(new CommandParam("Scale", () => Actor.Scale, v => Actor.Scale = (Vector3)v, (i, p) => i.Vector3Field(p)));
            }

            if (includeColor) Params.Add(new CommandParam("Tint", () => Actor.TintColor, v => Actor.TintColor = (Color)v, (i, p) => i.ColorField(p)));
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
            foreach(var choice in GameObject.GetComponentsInChildren<ChoiceHandlerButton>())
            {
                Params.Add(new CommandParam(choice.ChoiceState.Summary, () => (Vector2)choice.transform.localPosition, v => choice.transform.localPosition = (Vector2)v, (i, p) => i.Vector2Field(p)));
            }
        }
    }

}