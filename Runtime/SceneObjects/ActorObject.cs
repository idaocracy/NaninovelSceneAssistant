using Naninovel;
using UnityEngine;
using UnityEditor;
using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.SEIDInfo;

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
            this.id = id;
            Initialize();
            AddVars();
        }

        public override string Id => id;
        protected TActor Actor => (TActor)EngineService.GetActor(Id); 
        protected TMeta Metadata => Config.GetMetadataOrDefault(Id); 
        protected TConfig Config  => Engine.GetConfiguration<TConfig>(); 
        public override GameObject GameObject => GetGameObject();

        private string id;

        async UniTask<List<string>> GetAppearanceList()
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
            var texturePaths = GetAppearanceList().Result;

            if (texturePaths != null && texturePaths.Count > 0)
            {
                if (texturePaths.Any(t => t.EqualsFast(Id))) return texturePaths.First(t => t.EqualsFast(Id));
                if (texturePaths.Any(t => t.EqualsFast("Default"))) return texturePaths.First(t => t.EqualsFast("Default"));
            }
            return texturePaths.FirstOrDefault();
        }

        protected virtual void AddBaseParams(bool includeAppearance = true, bool includeColor = true, bool includeTransform = true)  
        {
            if (includeAppearance)
            {
                var appearances = GetAppearanceList().Result;
                if(appearances.Count > 0) Params.Add(new CommandParam("Appearance", () => Actor.Appearance ?? GetDefaultAppearance(), v => Actor.Appearance = (string)v, (i, p) => i.StringListField(p, appearances.ToArray())));
                else Params.Add(new CommandParam("Appearance", () => Actor.Appearance, v => Actor.Appearance = (string)v, (i, p) => i.StringField(p)));
            }

            if (includeTransform)
            {
                CommandParam pos = null;
                CommandParam position = null;

                Params.Add(position = new CommandParam("Position", () => Actor.Position, v => Actor.Position = (Vector3)v, (i,p) => i.Vector3Field(p, toggleWith:pos)));
                Params.Add(pos = new CommandParam("Pos", () => Actor.Position, v => Actor.Position = (Vector3)v, (i, p) => i.PosField(p, toggleWith: position)));
                Params.Add(new CommandParam("Rotation", () => Actor.Rotation.eulerAngles, v => Actor.Rotation = Quaternion.Euler((Vector3)v), (i, p) => i.Vector3Field(p)));
                Params.Add(new CommandParam("Scale", () => Actor.Scale, v => Actor.Scale = (Vector3)v, (i, p) => i.Vector3Field(p), defaultValue: Vector3.one));
            }

            if (includeColor) Params.Add(new CommandParam("Tint", () => Actor.TintColor, v => Actor.TintColor = (Color)v, (i, p) => i.ColorField(p), defaultValue: Color.white));
        }

        protected virtual void AddVars()
        {
            foreach(var variable in customVariableManager.GetAllVariables())
            {
                if (variable.Name.Contains(Id))
                {
                    CustomVars.Add(variable.Name, new CustomVar(variable.Name));
                }
            }
        }
    }


    public class Character : ActorObject<CharacterManager, ICharacterActor, CharacterMetadata, CharactersConfiguration>
    {
        public Character(string id) : base(id) { }
        protected override string CommandNameAndId => "char " + Id;
        protected override void AddParams()
        {
            AddBaseParams();
            Params.Add(new CommandParam("Look", () => Actor.LookDirection, v => Actor.LookDirection = (CharacterLookDirection)v, (i, p) => i.EnumField(p)));
        }
    }

    public class Background : ActorObject<BackgroundManager, IBackgroundActor, BackgroundMetadata, BackgroundsConfiguration>
    {
        public Background(string id) : base(id) { }
        protected override string CommandNameAndId => "back " + "id:" + Id;
        protected override void AddParams()
        {
            AddBaseParams();
        }
    }

    public class TextPrinter : ActorObject<TextPrinterManager, ITextPrinterActor, TextPrinterMetadata, TextPrintersConfiguration>
    {
        public TextPrinter(string id) : base(id) { }
        protected override string CommandNameAndId => "printer " + Id;
        protected override void AddParams()
        {
            AddBaseParams();
        }
    }

    public class ChoiceHandler : ActorObject<ChoiceHandlerManager, IChoiceHandlerActor, ChoiceHandlerMetadata, ChoiceHandlersConfiguration>
    {
        public ChoiceHandler(string id) : base(id) { }
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