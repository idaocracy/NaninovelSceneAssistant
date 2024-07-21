using Naninovel;
using Naninovel.UI;
using Naninovel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NaninovelSceneAssistant
{
	[InitializeAtRuntime]
	public class SceneAssistantManager : IEngineService<SceneAssistantConfiguration>
	{
		public SceneAssistantConfiguration Configuration { get; }
		public SceneAssistantManager(SceneAssistantConfiguration config) => Configuration = config;
		
		private IReadOnlyCollection<IActorManager> actorServices;
		private ISpawnManager spawnManager;
		private IScriptPlayer scriptPlayer;
		private ICustomVariableManager variableManager;
		private IUnlockableManager unlockableManager;
		private IStateManager stateManager;
		private IUIManager uiManager;

		public Dictionary<string, INaninovelObjectData> ObjectList { get; protected set; } = new Dictionary<string, INaninovelObjectData>();
		public Dictionary<Type, bool> ObjectTypeList { get; protected set; } = new Dictionary<Type, bool>();
		public SortedList<string, VariableData> VariableDataList { get; protected set; } = new SortedList<string, VariableData> { };
		public SortedList<string, UnlockableData> UnlockableDataList { get; protected set; } = new SortedList<string, UnlockableData> { };
		public SortedList<string, ScriptData> ScriptDataList { get; protected set; } = new SortedList<string, ScriptData>();
		public List<IUIData> ModalUIDataList { get; protected set; } = new List<IUIData>();
		public List<IUIData> UIDataList { get; protected set; } = new List<IUIData>();
		public Dictionary<string, string[]> VariableFilterMenus { get; protected set; } = new Dictionary<string, string[]>() { };
		public Dictionary<string, string> UnlockableFilterMenus { get; protected set; } = new Dictionary<string, string>() { };
		public Dictionary<string, string> ScriptFilterMenus { get; protected set; } = new Dictionary<string, string>() { };

		public bool IsAvailable { get; protected set; }
		public bool Initialized {get; protected set; }

		public Action OnSceneAssistantCleared;
		public Action OnSceneAssistantReset;
		public virtual UniTask InitializeServiceAsync() => UniTask.CompletedTask;
		public virtual void ResetService()
		{
			if(Initialized) ClearSceneAssistant();
		}
		public virtual void DestroyService()
		{
			if(Initialized) DestroySceneAssistant();
		}
		public virtual void GetServices()
		{
			actorServices = Engine.FindAllServices<IActorManager>();
			spawnManager = Engine.GetService<ISpawnManager>();
			scriptPlayer = Engine.GetService<IScriptPlayer>();
			variableManager = Engine.GetService<ICustomVariableManager>();
			unlockableManager = Engine.GetService<IUnlockableManager>();
			stateManager = Engine.GetService<IStateManager>();
			uiManager = Engine.GetService<IUIManager>();
		}
		public virtual async void InitializeSceneAssistant()
		{
			if(Initialized) return;
			GetServices();
			ResetSceneAssistant();

			scriptPlayer.OnCommandExecutionStart += ClearSceneAssistantOnCommandStart;
			scriptPlayer.OnCommandExecutionFinish += ResetSceneAssistantOnCommandFinish;
			
			stateManager.OnGameLoadFinished += HandleGameLoadFinished;
			stateManager.OnResetFinished += UpdateDataLists;
			stateManager.OnRollbackFinished += UpdateDataLists;
			
			foreach (var variable in variableManager.GetAllVariables()) 
				VariableDataList.Add(variable.Name, new VariableData(variable.Name));
			variableManager.OnVariableUpdated += HandleOnVariableUpdated;
			InitializeVariableFilterMenus();

			foreach (var unlockable in unlockableManager.GetAllItems()) 
				UnlockableDataList.Add(unlockable.Key, new UnlockableData(unlockable.Key));
			unlockableManager.OnItemUpdated += HandleOnUnlockableUpdated;
			InitializeUnlockableFilterMenus();
			
			await LocateScriptsAsync();
			InitializeScriptFilterMenus();


			Initialized = true;
		}

		private void HandleGameLoadFinished(GameSaveLoadArgs args) => UpdateDataLists();

		private void UpdateDataLists()
		{
			foreach (var variableData in VariableDataList.ToList())
			{
				if (variableManager.GetAllVariables().All(v => v.Name != variableData.Key))
					VariableDataList.Remove(variableData.Key);
			}
		}

		private void InitializeVariableFilterMenus()
		{
			VariableFilterMenus.Add("All", Array.Empty<string>());

			if (Configuration.CreateCharactersVariableFilterMenu)
			{
				var actorMap = Engine.GetService<ICharacterManager>().Configuration.ActorMetadataMap;
				VariableFilterMenus.Add("Characters", actorMap.GetAllIds().Select(id => id).ToArray());
			}
			
			foreach (var menu in Configuration.CustomVariableFilterMenus) 
				if(!string.IsNullOrEmpty(menu)) VariableFilterMenus.Add(FormatFilterMenuItem(menu), new string[] { menu });
		}
        
		private void InitializeScriptFilterMenus()
		{
			ScriptFilterMenus.Add("All", string.Empty);
			if (Configuration.CreateChapterFilterMenu) ScriptFilterMenus.Add("Chapters", Configuration.ChapterVariableFilterTemplate);
			foreach (var menu in Configuration.ScriptFilterMenus)
				if(!string.IsNullOrEmpty(menu)) ScriptFilterMenus.Add(FormatFilterMenuItem(menu), menu);
		}

		private void InitializeUnlockableFilterMenus()
		{
			UnlockableFilterMenus.Add("All", string.Empty);
			if (Configuration.CreateCGFilterMenu) UnlockableFilterMenus.Add("CG", "CG/");
			foreach (var menu in Configuration.UnlockableFilterMenus)
				if(!string.IsNullOrEmpty(menu)) UnlockableFilterMenus.Add(FormatFilterMenuItem(menu), menu);
		}
		
		private static string FormatFilterMenuItem(string value)
		{
			if (!value.EndsWith("_")) return value;
			return value.Substring(0, value.Length - 1);
		}
		
		private void HandleOnVariableUpdated(CustomVariableUpdatedArgs args)
		{
			if(!VariableDataList.ContainsKey(args.Name)) VariableDataList.Add(args.Name, new VariableData(args.Name));
			else
			{
				var variable = VariableDataList.FirstOrDefault(v => v.Key == args.Name);
				var argsValue = Naninovel.ExpressionEvaluator.Evaluate<string>(args.Name);
				if (variable.Value.Value != argsValue) variable.Value.Value = argsValue;
			}
		}

		private void HandleOnUnlockableUpdated(UnlockableItemUpdatedArgs args)
		{
			if(!UnlockableDataList.ContainsKey(args.Id)) VariableDataList.Add(args.Id, new VariableData(args.Id));
			else
			{
				var unlockable = UnlockableDataList.FirstOrDefault(u => u.Key == args.Id);
				if (unlockable.Value.Value != args.Unlocked) unlockable.Value.Value = args.Unlocked;
			}
		}
		
		private async UniTask LocateScriptsAsync()
		{
			var resourceProviderManager = Engine.GetService<IResourceProviderManager>();
			var scriptsConfiguration = Engine.GetConfiguration<ScriptsConfiguration>();

			foreach (var provider in resourceProviderManager.GetProviders(scriptsConfiguration.Loader.ProviderTypes))
			{
				var paths = await provider.LoadResourcesAsync<Script>(scriptsConfiguration.Loader.PathPrefix);
				foreach (var resource in paths)
				{
					var scriptName = resource.Path.Split("/".ToCharArray()).Last();
					ScriptDataList.Add(scriptName, new ScriptData(resource));
				}
			}
			
			await UniTask.CompletedTask;
		}
		public virtual void DestroySceneAssistant()
		{
			ClearSceneAssistant();

			stateManager.OnGameLoadFinished -= HandleGameLoadFinished;
			stateManager.OnResetFinished -= UpdateDataLists;
			stateManager.OnRollbackFinished -= UpdateDataLists;
			
			scriptPlayer.OnCommandExecutionStart -= ClearSceneAssistantOnCommandStart;
			scriptPlayer.OnCommandExecutionFinish -= ResetSceneAssistantOnCommandFinish;
			
			variableManager.OnVariableUpdated -= HandleOnVariableUpdated;
			unlockableManager.OnItemUpdated -= HandleOnUnlockableUpdated;
			
			VariableDataList.Clear();
			UnlockableDataList.Clear();
			ScriptDataList.Clear();
			UIDataList.Clear();
			ModalUIDataList.Clear();
			
			VariableFilterMenus.Clear();
			UnlockableFilterMenus.Clear();
			ScriptFilterMenus.Clear();
			
			if (scriptPlayer.PlayedScript != null && !scriptPlayer.Playing)
			{
				scriptPlayer.SetWaitingForInputEnabled(true);

				if (!scriptPlayer.Playing)
				{
					Type type = typeof(IScriptPlayer);
					MethodInfo methodInfo = type.GetMethod("Resume");
					if (methodInfo != null) methodInfo.Invoke(scriptPlayer, new object[] { null });
					else type.GetMethod("Play").Invoke(scriptPlayer, new object[] { null });
				}
			}
			
			Initialized = false;
		}
		
		private void ClearSceneAssistantOnCommandStart(Command command) 
		{
			ClearSceneAssistant();
			if(command is Stop) ResetSceneAssistant();
		}
		
		private void ResetSceneAssistantOnCommandFinish(Command command) => ResetSceneAssistant();

		public virtual void ClearSceneAssistant()
		{
			if(!IsAvailable) return;
			OnSceneAssistantCleared?.Invoke();
			IsAvailable = false;
			
			foreach(var obj in ObjectList.Values) 
			{
				foreach(var data in obj.CommandParameters) if(data is IDisposable disposable) disposable.Dispose();
			}
			ObjectTypeList.Clear();
			ObjectList.Clear();
			UIDataList.Clear();
			ModalUIDataList.Clear();
		}

		protected virtual void ResetSceneAssistant()
		{
			if(IsAvailable) return;
			
			ResetObjectList();
			ResetUIDataList();
			IsAvailable = true;
			OnSceneAssistantReset?.Invoke();
		}

		protected virtual void ResetObjectList()
		{
			ResetCamera();
			ResetActorList();
			ResetSpawnList();
			ResetObjectTypeList();
		}

		public virtual void ResetUIDataList()
		{
			var uiTransform = Engine.RootObject.transform.Find("UI").transform;
			for(int i = 0; i < uiTransform.childCount; i++)
			{
				if(uiTransform.GetChild(i).TryGetComponent<CustomUI>(out var ui)) AddUIData(UIDataList, ui);
				else if(uiTransform.GetChild(i).name == "TextPrinter" || uiTransform.GetChild(i).name == "ChoiceHandler") 
				{
					var uiActorTransform = uiTransform.GetChild(i);
					for (int t = 0; t < uiActorTransform.childCount; t++)
                    {
						if (uiActorTransform.GetChild(t).TryGetComponent<CustomUI>(out var uiActor))
							AddUIData(UIDataList, uiActor);
					}
				}
			}

			UIDataList = UIDataList.OrderBy(u => u.GameObject.GetComponent<Canvas>().sortingOrder).Reverse().ToList();

			var modalUiTransform = Engine.RootObject.transform.Find("ModalUI").transform;
			for (int i = 0; i < modalUiTransform.childCount; i++)
			{
				if (modalUiTransform.GetChild(i).TryGetComponent<CustomUI>(out var ui)) AddUIData(ModalUIDataList, ui);
			}
			ModalUIDataList = ModalUIDataList.OrderBy(u => u.GameObject.GetComponent<Canvas>().sortingOrder).Reverse().ToList();

			void AddUIData(List<IUIData> uIDatas, CustomUI ui)
            {
				if (ui is SaveLoadMenu saveLoadMenu)
				{
					uIDatas.Add(new SaveUIData(saveLoadMenu));
					uIDatas.Add(new LoadUIData(saveLoadMenu));
					uIDatas.Add(new QuickLoadUIData(saveLoadMenu));
				}
				else if (ui is ToastUI toastUI) uIDatas.Add(new ToastUIData(toastUI));
				else if (ui is UITextPrinterPanel textPrinter) uIDatas.Add(new TextPrinterUIData(textPrinter));
				else if (ui is ChoiceHandlerPanel choiceHandler) uIDatas.Add(new ChoiceHandlerUIData(choiceHandler));
				else uIDatas.Add(new UIData<CustomUI>(ui));
			}
		}

		protected virtual void ResetCamera()
		{
			var camera = new CameraData();
			ObjectList.Add(camera.Id, camera);
		}

		protected virtual void ResetActorList()
		{
			foreach (var actorService in actorServices)
			{
				foreach (var actor in actorService.GetAllActors())
				{
					if (actor is ICharacterActor character && character.Visible) ObjectList.Add(character.Id, new CharacterData(character.Id));
					if (actor is IBackgroundActor background && background.Visible) ObjectList.Add(background.Id, new BackgroundData(background.Id));
					if (actor is IChoiceHandlerActor choiceHandler && choiceHandler.Visible) ObjectList.Add(choiceHandler.Id, new ChoiceHandlerData(choiceHandler.Id));
					if (actor is ITextPrinterActor textPrinter && textPrinter.Visible) ObjectList.Add(textPrinter.Id, new TextPrinterData(textPrinter.Id));
				}
			}
		}
		protected virtual void ResetSpawnList()
		{
			foreach (var spawn in spawnManager.GetAllSpawned()) ObjectList.Add(spawn.Path, new SpawnData(spawn.Path));
		}
		private void ResetObjectTypeList()
		{
			if (ObjectList.Count == 0) return;

			foreach (var obj in ObjectList.Values.ToList())
			{
				if (!ObjectTypeList.Keys.Any(t => t == obj.GetType())) ObjectTypeList.Add(obj.GetType(), true);
			}
		}
	}
}