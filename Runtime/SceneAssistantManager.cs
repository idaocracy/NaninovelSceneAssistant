using Naninovel;
using Naninovel.Commands;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;

namespace NaninovelSceneAssistant
{
    [InitializeAtRuntime]
    public class SceneAssistantManager : IEngineService
    {
        public virtual SceneAssistantConfiguration Configuration { get; }
        private ISpawnManager spawnManager;
        private IScriptPlayer scriptPlayer;
        private ICustomVariableManager variableManager;
        private IUnlockableManager unlockableManager;
        private IStateManager stateManager;
        private IReadOnlyCollection<IActorManager> actorServices;

        public List<INaninovelObject> ObjectList { get; protected set; } = new List<INaninovelObject>();
        public SortedList<string, CustomVar> CustomVarList { get; protected set; } = new SortedList<string, CustomVar> { };
        public SortedList<string, Unlockable> UnlockablesList { get; protected set; } = new SortedList<string, Unlockable> { };

        public SceneAssistantManager(SceneAssistantConfiguration config, ISpawnManager spawnManager, IScriptPlayer scriptPlayer, ICustomVariableManager variableManager, IUnlockableManager unlockableManager,
                IStateManager stateManager)
        {
            Configuration = config;
            this.spawnManager = spawnManager;
            this.scriptPlayer = scriptPlayer;
            this.variableManager = variableManager;
            this.unlockableManager = unlockableManager;
            this.stateManager = stateManager;
            actorServices = Engine.FindAllServices<IActorManager>();
        }

        public virtual UniTask InitializeServiceAsync()
        {
#if UNITY_EDITOR
            //Engine.OnInitializationFinished += InitializeSceneAssistant;
            EditorApplication.playModeStateChanged += DetectPlayModeChanged;
#endif



            return UniTask.CompletedTask;
        }


        private void DetectPlayModeChanged(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.EnteredPlayMode)
            {
                Engine.OnInitializationFinished += InitializeSceneAssistant;
            }
        }

        public void ResetService()
        {            

        }

        public void DestroyService()
        {
            if (ObjectList.Count > 0) DestroySceneAssistant();
        }

        public void InitializeSceneAssistant()
        {
            actorServices = Engine.FindAllServices<IActorManager>();

            RefreshLists();
            variableManager.OnVariableUpdated += HandleVariableUpdated;
            unlockableManager.OnItemUpdated += HandleUnlockableUpdated;

            stateManager.OnGameLoadFinished += HandleOnGameLoadFinished;
            stateManager.OnResetFinished += RefreshLists;
            stateManager.OnRollbackFinished += RefreshLists;

            scriptPlayer.AddPostExecutionTask(HandlePlayedCommand);
        }

        private void HandleOnGameLoadFinished(GameSaveLoadArgs obj) => RefreshLists();

        protected void RefreshLists()
        {
            ObjectList.Clear();
            RefreshObjectList();

            CustomVarList.Clear();
            foreach (var variable in variableManager.GetAllVariables()) CustomVarList.Add(variable.Name, new CustomVar(variable.Name, variable.Value));

            UnlockablesList.Clear();
            foreach (var unlockable in unlockableManager.GetAllItems()) UnlockablesList.Add(unlockable.Key, new Unlockable(unlockable.Key, unlockable.Value));
        }

        protected void HandleVariableUpdated(CustomVariableUpdatedArgs args)
        {
            if (CustomVarList.ContainsKey(args.Name)) CustomVarList[args.Name].Value = args.Value;
            else CustomVarList.Add(args.Name, new CustomVar(args.Name, args.Value));
        }

        protected void HandleUnlockableUpdated(UnlockableItemUpdatedArgs args)
        {
            if (UnlockablesList.ContainsKey(args.Id)) UnlockablesList[args.Id].Value = args.Unlocked;
            else UnlockablesList.Add(args.Id, new Unlockable(args.Id, args.Unlocked));
        }

        public void DestroySceneAssistant()
        {
            scriptPlayer.RemovePostExecutionTask(HandlePlayedCommand);
            unlockableManager.OnItemUpdated -= HandleUnlockableUpdated;
            variableManager.OnVariableUpdated -= HandleVariableUpdated;
            ObjectList.Clear();
        }

        protected virtual void RefreshObjectList()
        {
            if(!ObjectExists(typeof(CameraObject))) ObjectList.Add(new CameraObject());
            RefreshSpawnList();
            RefreshActorList();
        }

        protected virtual void RefreshActorList(Type type = null)
        {
            foreach (var actorService in actorServices)
            {
                foreach (var actor in actorService.GetAllActors())
                {
                    if (actor is ICharacterActor character) 
                        if (!ObjectExists(typeof(ICharacterActor), character.Id) && character.Visible) ObjectList.Add(new CharacterObject(character.Id));
                    if (actor is IBackgroundActor background) 
                        if (!ObjectExists(typeof(IBackgroundActor), background.Id) && background.Visible) ObjectList.Add(new BackgroundObject(background.Id));
                    if (actor is IChoiceHandlerActor choiceHandler) 
                        if (!ObjectExists(typeof(IChoiceHandlerActor), choiceHandler.Id) && choiceHandler.Visible) ObjectList.Add(new ChoiceHandlerObject(choiceHandler.Id));
                    if (actor is ITextPrinterActor textPrinter) 
                        if (!ObjectExists(typeof(ITextPrinterActor), textPrinter.Id) && textPrinter.Visible) ObjectList.Add(new TextPrinterObject(textPrinter.Id));
                }
            }
        }

        protected virtual void RefreshSpawnList()
        {
            foreach (var spawn in spawnManager.GetAllSpawned()) if(!ObjectExists(typeof(SpawnObject), spawn.Path)) ObjectList.Add(new SpawnObject(spawn.Path));
        }

        public virtual UniTask HandlePlayedCommand(Command command = null)
        {
            if (command is ModifyCharacter) RefreshActorList();
            if (command is ModifyBackground) RefreshActorList();
            if (command is ModifyTextPrinter) RefreshActorList();
            if (command is AddChoice) RefreshActorList();
            if (command is Spawn) RefreshSpawnList();
            if (command is DestroySpawned) RefreshSpawnList();
            if (command is DestroyAllSpawned) RefreshSpawnList();

            if (command is HideAllActors) RefreshActorList();
            if (command is HideAllCharacters) RefreshActorList();
            if (command is HideActors) RefreshActorList();
            if (command is HidePrinter) RefreshActorList();
            if (command is ClearChoiceHandler) RefreshActorList();

            return UniTask.CompletedTask;
        }

        private bool ObjectExists(Type type, string id = null)
        {
            if (!ObjectList.Any(c => c.GetType() == type))  return false;
            else
            {
                if (string.IsNullOrEmpty(id)) return true;
                else return (ObjectList.Any(c => c.GetType() == type && c.Id == c.Id));
            }

        }

        public string GetAllCommands()
        {
            var allString = String.Empty;

            foreach (var o in ObjectList)
            {
                allString = allString + "@" + o.GetCommandLine() + "\n";
            }
            return allString;
        }
    }
}




