using Naninovel;
using Naninovel.UI;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace NaninovelSceneAssistant
{
	public abstract class ActorData<TService, TActor, TMeta, TConfig> : NaninovelObjectData<TService, TConfig>, INaninovelObjectData
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
		public override GameObject GameObject => GetGameObject();
		protected TActor Actor => (TActor)Service.GetActor(Id);
		protected TMeta Metadata => Config.GetMetadataOrDefault(Id);
		protected virtual float? DefaultZOffset { get; }
		private string id;

		protected virtual async UniTask<IReadOnlyCollection<string>> GetAppearanceList()
		{
			var resourceProviderManager = Engine.GetService<IResourceProviderManager>();
			
			// In case you are extending the data class and want to add a dropdown list for appearances, you'll need to be explicit with typing, 
			// as otherwise the dropdown list won't appear in build. await LocateResourcesAtPathAsync<UnityEngine.Object>() will work for all types in the editor.
			var providers = resourceProviderManager.GetProviders(Metadata.Loader.ProviderTypes);
			if(Actor is SpriteCharacter || Actor is SpriteBackground)  return await LocateResourcesAtPathAsync<Texture2D>();
			else if(Actor is VideoCharacter || Actor is VideoBackground)  return await LocateResourcesAtPathAsync<VideoClip>();
			else return await LocateResourcesAtPathAsync<UnityEngine.Object>();
			
			async UniTask <IReadOnlyCollection<string>> LocateResourcesAtPathAsync<T>() where T: UnityEngine.Object
			{
				var actorPath = "${Metadata.Loader.PathPrefix}/{Id}";
				var resourcePaths = await providers.LocateResourcesAsync<T>(actorPath);
				var appearances = new List<string>();
				
				foreach (var path in resourcePaths) 
				{
					var appearance = path.Remove("${actorPath}/");
					appearances.Add(appearance);
				} 
				return appearances;
			} 
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
				
				if(appearances.Count > 0) 
				{
					CommandParameters.Add(new CommandParameterData<string>("Appearance", () => Actor.Appearance ?? GetDefaultAppearance(), v => Actor.Appearance = (string)v, (i, p) => i.StringDropdownField(p, appearances.ToArray()), defaultValue: GetDefaultAppearance()));
				}
				else CommandParameters.Add(new CommandParameterData<string>("Appearance", () => Actor.Appearance, v => Actor.Appearance = (string)v, (i, p) => i.StringField(p)));
			}

			if (includeTransform)
			{
				ICommandParameterData pos = null;
				ICommandParameterData position = null;

				CommandParameters.Add(position = new CommandParameterData<Vector3>("Position", () => Actor.Position, v => Actor.Position = v, (i, p) => i.Vector3Field(p, toggleGroup:pos), defaultValue: new Vector3(0,-5.4f, DefaultZOffset ?? 0)));
				CommandParameters.Add(pos = new CommandParameterData<Vector3>("Pos", () => Actor.Position, v => Actor.Position = v, (i, p) => i.PosField(p, CameraConfiguration, toggleGroup:position), defaultValue: new Vector3(0, -5.4f, DefaultZOffset ?? 0)));
				CommandParameters.Add(new CommandParameterData<Vector3>("Rotation", () => Actor.Rotation.eulerAngles, v => Actor.Rotation = Quaternion.Euler(v), (i, p) => i.Vector3Field(p)));
				CommandParameters.Add(new CommandParameterData<Vector3>("Scale", () => Actor.Scale, v => Actor.Scale = v, (i, p) => i.Vector3Field(p), defaultValue: Vector3.one));
			}

			if (includeTint) CommandParameters.Add(new CommandParameterData<Color>("Tint", () => Actor.TintColor, v => Actor.TintColor = v, (i, p) => i.ColorField(p), defaultValue: Color.white));
		}
	}

	public class CharacterData : ActorData<CharacterManager, ICharacterActor, CharacterMetadata, CharactersConfiguration> 
	{
		public CharacterData(string id) : base(id) { }
		public static string TypeId => "Character";
		protected override string CommandNameAndId => "char " + Id;
		protected override float? DefaultZOffset => Config.ZOffset;
		protected override void AddCommandParameters()
		{
			AddBaseParameters();
			CommandParameters.Add(new CommandParameterData<Enum>("Look", () => Actor.LookDirection, v => Actor.LookDirection = (CharacterLookDirection)v, (i,p) => i.EnumDropdownField(p), defaultValue: Metadata.BakedLookDirection));
		}
	}

	public class BackgroundData : ActorData<BackgroundManager, IBackgroundActor, BackgroundMetadata, BackgroundsConfiguration>
	{
		public BackgroundData(string id) : base(id) { }
		public static string TypeId => "Background";
		protected override string CommandNameAndId => "back " + "id:" + Id;
		protected override float? DefaultZOffset => Config.ZOffset;
		protected override void AddCommandParameters()
		{
			AddBaseParameters();
		}
	}

	public class TextPrinterData : ActorData<TextPrinterManager, ITextPrinterActor, TextPrinterMetadata, TextPrintersConfiguration>
	{
		public TextPrinterData(string id) : base(id) { }
		public static string TypeId => "Text Printer";
		protected override string CommandNameAndId => "printer " + Id;
		protected override void AddCommandParameters()
		{
			AddBaseParameters(includeZPos:false);
		}
	}

	public class ChoiceHandlerData : ActorData<ChoiceHandlerManager, IChoiceHandlerActor, ChoiceHandlerMetadata, ChoiceHandlersConfiguration>
	{
		public ChoiceHandlerData(string id) : base(id) { }
		public static string TypeId => "ChoiceHandler";
		protected override string CommandNameAndId => "choice";
		protected Dictionary<ChoiceState, ChoiceHandlerButton> ChoiceButtons;

		public override string GetCommandLine( bool inlined = false, bool paramsOnly = false)
		{
			var choiceList = new List<string>();

			foreach (var param in CommandParameters)
			{
				if(!param.Selected) continue;
				var choiceString = CommandNameAndId + " " + param.GetCommandValue() + " handler:" + Id;
				choiceList.Add(inlined ? "[" + choiceString + "]" : "@" + choiceString);
			}

			return string.Join("\n", choiceList);
		}

		protected override void AddCommandParameters()
		{
			ChoiceButtons = new Dictionary<ChoiceState, ChoiceHandlerButton>();

			foreach(var choiceState in Actor.Choices)
			{
				ChoiceButtons.Add(choiceState, GameObject.GetComponentsInChildren<ChoiceHandlerButton>().FirstOrDefault(c => c.ChoiceState == choiceState));
			}

			foreach (var button in ChoiceButtons)
			{
				CommandParameters.Add(new CommandParameterData<Vector2>(button.Value.ChoiceState.Summary + " pos", () => (Vector2)button.Value.transform.localPosition, v => button.Value.transform.localPosition = v, (i, p) => i.Vector2Field(p)));
			}
		}
	}
}