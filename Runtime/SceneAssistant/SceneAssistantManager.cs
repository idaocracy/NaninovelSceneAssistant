using Naninovel;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NaninovelSceneAssistant
{
	[InitializeAtRuntime]
	public class SceneAssistantManager : IEngineService
	{
		private IReadOnlyCollection<IActorManager> actorServices;
		private ISpawnManager spawnManager;
		private IScriptPlayer scriptPlayer;
		private ICustomVariableManager variableManager;
		private IUnlockableManager unlockableManager;
		private IStateManager stateManager;

		public Dictionary<string, INaninovelObjectData> ObjectList { get; protected set; } = new Dictionary<string, INaninovelObjectData>();
		public Dictionary<Type, bool> ObjectTypeList { get; protected set; } = new Dictionary<Type, bool>();
		public SortedList<string, VariableData> CustomVarList { get; protected set; } = new SortedList<string, VariableData> { };
		public SortedList<string, UnlockableData> UnlockablesList { get; protected set; } = new SortedList<string, UnlockableData> { };
		public List<string> ScriptsList { get; protected set; } = new List<string>();
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
		}
		public virtual async void InitializeSceneAssistant()
		{
			GetServices();
			await LocateScriptsAsync();

			ResetSceneAssistant();

			scriptPlayer.OnCommandExecutionStart += ClearSceneAssistantOnCommandStart;
			scriptPlayer.OnCommandExecutionFinish += ResetSceneAssistantOnCommandFinish;

			stateManager.OnRollbackStarted += ClearSceneAssistant;
			stateManager.OnRollbackFinished += ResetSceneAssistant;

			stateManager.OnGameLoadStarted += ClearSceneAssistantOnGameLoading;
			stateManager.OnGameLoadFinished += ResetSceneAssistantOnGameLoaded;

			stateManager.OnResetStarted += ClearSceneAssistant;
			stateManager.OnResetFinished += ResetSceneAssistant;

			Initialized = true;
		}
		private async UniTask LocateScriptsAsync()
		{
			var resourceProviderManager = Engine.GetService<IResourceProviderManager>();
			var scriptsConfiguration = Engine.GetConfiguration<ScriptsConfiguration>();

			foreach (var provider in resourceProviderManager.GetProviders(scriptsConfiguration.Loader.ProviderTypes))
			{
				var paths = await provider.LocateResourcesAsync<Script>(scriptsConfiguration.Loader.PathPrefix);
				foreach (var path in paths) ScriptsList.Add(path.Split("/".ToCharArray()).Last());
			}

			await UniTask.CompletedTask;
		}
		public virtual void DestroySceneAssistant()
		{
			ClearSceneAssistant();

			scriptPlayer.OnCommandExecutionStart -= ClearSceneAssistantOnCommandStart;
			scriptPlayer.OnCommandExecutionFinish -= ResetSceneAssistantOnCommandFinish;

			stateManager.OnRollbackStarted -= ClearSceneAssistant;
			stateManager.OnRollbackFinished -= ResetSceneAssistant;

			stateManager.OnGameLoadStarted -= ClearSceneAssistantOnGameLoading;
			stateManager.OnGameLoadFinished -= ResetSceneAssistantOnGameLoaded;

			stateManager.OnResetStarted -= ClearSceneAssistant;
			stateManager.OnResetFinished -= ResetSceneAssistant;
			
			Initialized = false;
		}
		private void ClearSceneAssistantOnCommandStart(Command command) => ClearSceneAssistant();
		private void ResetSceneAssistantOnCommandFinish(Command command) => ResetSceneAssistant();
		private void ClearSceneAssistantOnGameLoading(GameSaveLoadArgs obj) => ClearSceneAssistant();
		private void ResetSceneAssistantOnGameLoaded(GameSaveLoadArgs obj) => ResetSceneAssistant();

		public virtual void ClearSceneAssistant()
		{
			OnSceneAssistantCleared?.Invoke();
			
			foreach(var obj in ObjectList.Values) 
			{
				if(obj is IDisposable disposable) disposable.Dispose();
			}
			IsAvailable = false;
			CustomVarList.Clear();
			UnlockablesList.Clear();
			ObjectTypeList.Clear();
			ObjectList.Clear();
		}

		protected virtual void ResetSceneAssistant()
		{
			ClearSceneAssistant();
			ResetObjectList();

			foreach (var variable in variableManager.GetAllVariables()) CustomVarList.Add(variable.Name, new VariableData(variable.Name));
			foreach (var unlockable in unlockableManager.GetAllItems()) UnlockablesList.Add(unlockable.Key, new UnlockableData(unlockable.Key));
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
		private void ResetCamera()
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