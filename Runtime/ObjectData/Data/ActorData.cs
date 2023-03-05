using Naninovel;
using UnityEngine;
using Naninovel.UI;
using System.Collections.Generic;
using System.Linq;

namespace NaninovelSceneAssistant
{
    public abstract class ActorData<TService, TActor, TMeta, TConfig> : NaninovelObjectData<TService>, INaninovelObjectData
        where TService : class, IActorManager
        where TActor : IActor
        where TMeta : ActorMetadata
        where TConfig : ActorManagerConfiguration<TMeta>
    {
        public ActorData(string id) : base()
        {
            this.id = id;
            Initialize();
        }
        public override string Id => id;
        protected TActor Actor => (TActor)EngineService.GetActor(Id); 
        protected TMeta Metadata => Config.GetMetadataOrDefault(Id); 
        protected TConfig Config  => Engine.GetConfiguration<TConfig>(); 
        public override GameObject GameObject => GetGameObject();
        protected CameraConfiguration CameraConfiguration { get => Engine.GetConfiguration<CameraConfiguration>(); }

        private string id;

        private async UniTask<List<string>> GetAppearanceList()
        {
            var resourceProviderManager = Engine.GetService<IResourceProviderManager>();
            var appearanceList = new List<string>();

            foreach (var provider in resourceProviderManager.GetProviders(Metadata.Loader.ProviderTypes))
            {
                var paths = await provider.LocateResourcesAsync<UnityEngine.Object>(Metadata.Loader.PathPrefix + "/" + Id);
                foreach (var path in paths) appearanceList.Add(path.Split("/".ToCharArray()).Last());
            }
            return appearanceList;
        }

        protected GameObject GetGameObject()
        {
            var monoActor = Actor as MonoBehaviourActor<TMeta>;
            return monoActor.GameObject;
        }

        protected string GetDefaultAppearance()
        {
            var appearancePaths = GetAppearanceList().Result;

            if (appearancePaths != null && appearancePaths.Count > 0)
            {
                if (appearancePaths.Any(t => t.EqualsFast(Id))) return appearancePaths.First(t => t.EqualsFast(Id));
                if (appearancePaths.Any(t => t.EqualsFast("Default"))) return appearancePaths.First(t => t.EqualsFast("Default"));
            }
            return appearancePaths.FirstOrDefault();
        }

        protected virtual void AddBaseParameters(bool includeAppearance = true, bool includeTint = true, bool includeTransform = true, bool includeZPos = true)  
        {
            if (includeAppearance)
            {
                var appearances = GetAppearanceList().Result;
                if(appearances.Count > 0) Params.Add(new ParameterValue("Appearance", () => Actor.Appearance ?? GetDefaultAppearance(), v => Actor.Appearance = (string)v, (i, p) => i.StringListField(p, appearances.ToArray())));
                else Params.Add(new ParameterValue("Appearance", () => Actor.Appearance, v => Actor.Appearance = (string)v, (i, p) => i.StringField(p)));
            }

            if (includeTransform)
            {
                ParameterValue pos = null;
                ParameterValue position = null;

                //todo use Vector 2 values for printers
                Params.Add(position = new ParameterValue("Position", () => Actor.Position, v => Actor.Position = (Vector3)v, (i, p) => i.Vector3Field(p, includeZPos, toggleWith: pos)));
                Params.Add(pos = new ParameterValue("Pos", () => Actor.Position, v => Actor.Position = (Vector3)v, (i, p) => i.PosField(p, CameraConfiguration, includeZPos, toggleWith: position)));
                Params.Add(new ParameterValue("Rotation", () => Actor.Rotation.eulerAngles, v => Actor.Rotation = Quaternion.Euler((Vector3)v), (i, p) => i.Vector3Field(p)));
                Params.Add(new ParameterValue("Scale", () => Actor.Scale, v => Actor.Scale = (Vector3)v, (i, p) => i.Vector3Field(p), defaultValue: Vector3.one));
            }

            if (includeTint) Params.Add(new ParameterValue("Tint", () => Actor.TintColor, v => Actor.TintColor = (Color)v, (i, p) => i.ColorField(p), defaultValue: Color.white));
        }
    }

    public class CharacterData : ActorData<CharacterManager, ICharacterActor, CharacterMetadata, CharactersConfiguration>
    {
        public CharacterData(string id) : base(id) { }
        public static string TypeId => "Character";
        protected override string CommandNameAndId => "char " + Id;
        protected override void AddParams()
        {
            AddBaseParameters();
            Params.Add(new ParameterValue("Look", () => Actor.LookDirection, v => Actor.LookDirection = (CharacterLookDirection)v, (i, p) => i.EnumField(p)));
        }
    }

    public class BackgroundData : ActorData<BackgroundManager, IBackgroundActor, BackgroundMetadata, BackgroundsConfiguration>
    {
        public BackgroundData(string id) : base(id) { }
        public static string TypeId => "Background";
        protected override string CommandNameAndId => "back " + "id:" + Id;
        protected override void AddParams()
        {
            AddBaseParameters();
        }
    }

    public class TextPrinterData : ActorData<TextPrinterManager, ITextPrinterActor, TextPrinterMetadata, TextPrintersConfiguration>
    {
        public TextPrinterData(string id) : base(id) { }
        public static string TypeId => "Text Printer";
        protected override string CommandNameAndId => "printer " + Id;
        protected override void AddParams()
        {
            AddBaseParameters(includeZPos:false);
        }
    }

    public class ChoiceHandlerData : ActorData<ChoiceHandlerManager, IChoiceHandlerActor, ChoiceHandlerMetadata, ChoiceHandlersConfiguration>
    {
        public ChoiceHandlerData(string id) : base(id) { }
        public static string TypeId => "ChoiceHandler";
        protected override string CommandNameAndId => "choice";
        protected List<ChoiceHandlerButton> ChoiceHandlerButtons => GameObject.GetComponentsInChildren<ChoiceHandlerButton>().ToList();

        protected override void AddParams()
        {
            foreach(var choice in ChoiceHandlerButtons)
            {
                Params.Add(new ParameterValue(choice.ChoiceState.Summary + " pos", () => (Vector2)choice.transform.localPosition, v => choice.transform.localPosition = (Vector2)v, (i, p) => i.Vector2Field(p)));
            }
        }
    }

}