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
        private IScriptManager scriptManager;
        private ICustomVariableManager variableManager;
        private IUnlockableManager unlockableManager;
        private IStateManager stateManager;
        private IReadOnlyCollection<IActorManager> actorServices;
        public Dictionary<string, INaninovelObjectData> ObjectList { get; protected set; } = new Dictionary<string, INaninovelObjectData>();
        public Dictionary<Type, bool> ObjectTypeList { get; protected set; } = new Dictionary<Type, bool>();
        public SortedList<string, VariableValue> CustomVarList { get; protected set; } = new SortedList<string, VariableValue> { };
        public SortedList<string, UnlockableValue> UnlockablesList { get; protected set; } = new SortedList<string, UnlockableValue> { };
        public IReadOnlyCollection<string> ScriptsList { get; protected set; }

        public Action OnSceneAssistantReset;

        public bool Initialised { get; protected set; } = false;

        public SceneAssistantManager(SceneAssistantConfiguration config, ISpawnManager spawnManager, IScriptPlayer scriptPlayer, ICustomVariableManager variableManager, IUnlockableManager unlockableManager,
                IStateManager stateManager, IScriptManager scriptManager)
        {
            Configuration = config;
            this.spawnManager = spawnManager;
            this.scriptPlayer = scriptPlayer;
            this.variableManager = variableManager;
            this.unlockableManager = unlockableManager;
            this.stateManager = stateManager;
            this.scriptManager = scriptManager;
        }

        public virtual UniTask InitializeServiceAsync()
        {
            return UniTask.CompletedTask;
        }

        public void ResetService()
        {            

        }

        public void DestroyService()
        {
            if (ObjectList.Count > 0) DestroySceneAssistant();
            Initialised = false;
        }

        public async void InitializeSceneAssistant()
        {
            actorServices = Engine.FindAllServices<IActorManager>();

            RefreshLists();
            ScriptsList = await scriptManager.LocateScriptsAsync();

            variableManager.OnVariableUpdated += HandleVariableUpdated;
            unlockableManager.OnItemUpdated += HandleUnlockableUpdated;

            stateManager.OnGameLoadFinished += HandleOnGameLoadFinished;
            stateManager.OnResetFinished += RefreshLists;
            stateManager.OnRollbackFinished += RefreshLists;

            scriptPlayer.AddPostExecutionTask(HandlePlayedCommand);

            Initialised = true; 
        }

        private void HandleOnGameLoadFinished(GameSaveLoadArgs obj) => RefreshLists();

        protected void RefreshLists()
        {
            ObjectList.Clear();
            RefreshObjectList();

            CustomVarList.Clear();
            foreach (var variable in variableManager.GetAllVariables()) CustomVarList.Add(variable.Name, new VariableValue(variable.Name));

            UnlockablesList.Clear();
            foreach (var unlockable in unlockableManager.GetAllItems()) UnlockablesList.Add(unlockable.Key, new UnlockableValue(unlockable.Key));
        }

        protected void HandleVariableUpdated(CustomVariableUpdatedArgs args)
        {
            if (CustomVarList.ContainsKey(args.Name)) CustomVarList[args.Name].Value = args.Value;
            else CustomVarList.Add(args.Name, new VariableValue(args.Name));
        }

        protected void HandleUnlockableUpdated(UnlockableItemUpdatedArgs args)
        {
            if (UnlockablesList.ContainsKey(args.Id)) UnlockablesList[args.Id].Value = args.Unlocked;
            else UnlockablesList.Add(args.Id, new UnlockableValue(args.Id));
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
            if (!ObjectExists(typeof(CameraData)))
            {
                var camera = new CameraData();
                ObjectList.Add(camera.Id, camera);
            }
            RefreshSpawnList();
            RefreshActorList();
            RefreshObjectTypeList();

            OnSceneAssistantReset?.Invoke();
        }

        protected virtual void RefreshActorList()
        {
            foreach (var actorService in actorServices)
            {
                foreach (var actor in actorService.GetAllActors())
                {
                    if (actor is ICharacterActor character) 
                        if (!ObjectExists(typeof(CharacterData), character.Id) && character.Visible) ObjectList.Add(character.Id, new CharacterData(character.Id));
                    if (actor is IBackgroundActor background) 
                        if (!ObjectExists(typeof(BackgroundData), background.Id) && background.Visible) ObjectList.Add(background.Id, new BackgroundData(background.Id));
                    if (actor is IChoiceHandlerActor choiceHandler) 
                        if (!ObjectExists(typeof(ChoiceHandlerData), choiceHandler.Id) && choiceHandler.Visible) ObjectList.Add(choiceHandler.Id, new ChoiceHandlerData(choiceHandler.Id));
                    if (actor is ITextPrinterActor textPrinter) 
                        if (!ObjectExists(typeof(TextPrinterData), textPrinter.Id) && textPrinter.Visible) ObjectList.Add(textPrinter.Id, new TextPrinterData(textPrinter.Id));
                }
            }
        }

        protected virtual void RefreshSpawnList()
        {
            foreach (var spawn in spawnManager.GetAllSpawned()) if(!ObjectExists(typeof(SpawnData), spawn.Path)) ObjectList.Add(spawn.Path, new SpawnData(spawn.Path));
        }

        public virtual UniTask HandlePlayedCommand(Command command = null)
        {
            RefreshObjectList();
            //if (command is ModifyCharacter) RefreshActorList();
            //if (command is ModifyBackground) RefreshActorList();
            //if (command is ModifyTextPrinter) RefreshActorList();
            //if (command is AddChoice) RefreshActorList();
            //if (command is Naninovel.Commands.Spawn) RefreshSpawnList();
            //if (command is DestroySpawned) RefreshSpawnList();
            //if (command is DestroyAllSpawned) RefreshSpawnList();

            //if (command is HideAllActors) RefreshActorList();
            //if (command is HideAllCharacters) RefreshActorList();
            //if (command is HideActors) RefreshActorList();
            //if (command is HidePrinter) RefreshActorList();
            //if (command is ClearChoiceHandler) RefreshActorList();

            RefreshObjectTypeList();

            return UniTask.CompletedTask;
        }

        private void RefreshObjectTypeList()
        {
            if (ObjectList.Count == 0) return;
            
            foreach (var obj in ObjectList.Values.ToList())
            {
                if (!ObjectTypeList.Keys.Any(t => t == obj.GetType())) ObjectTypeList.Add(obj.GetType(), true);
            }

            foreach (var type in ObjectTypeList.Keys.ToList())
            {
                if (!ObjectExists(type)) ObjectTypeList.Remove(type);
            }
        }

        private bool ObjectExists(Type type, string id = null)
        {
            if (ObjectList.Count == 0) return false;

            if (!ObjectList.Any(c => c.Value.GetType() == type))  return false;
            else
            {
                if (string.IsNullOrEmpty(id)) return true;
                else return (ObjectList.Any(c => c.Value.GetType() == type && c.Key == id));
            }
        }


    }
}




