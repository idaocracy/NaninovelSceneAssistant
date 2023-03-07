using Naninovel;
using System.Collections.Generic;
using System;
using System.Linq;
using Naninovel.Commands;

namespace NaninovelSceneAssistant
{
    [InitializeAtRuntime]
    public class SceneAssistantManager : IEngineService
    {
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

        public SceneAssistantManager(ISpawnManager spawnManager, IScriptPlayer scriptPlayer, ICustomVariableManager variableManager, IUnlockableManager unlockableManager,
                IStateManager stateManager, IScriptManager scriptManager)
        {
            this.spawnManager = spawnManager;
            this.scriptPlayer = scriptPlayer;
            this.variableManager = variableManager;
            this.unlockableManager = unlockableManager;
            this.stateManager = stateManager;
            this.scriptManager = scriptManager;
        }

        public virtual UniTask InitializeServiceAsync() => UniTask.CompletedTask;

        public virtual void ResetService()
        {
            if(Initialised) ResetLists();
        }

        public virtual void DestroyService()
        {
            if (Initialised) DestroySceneAssistant();
        }

        public virtual async void InitializeSceneAssistant()
        {
            actorServices = Engine.FindAllServices<IActorManager>();
            ScriptsList = await scriptManager.LocateScriptsAsync();

            ResetLists();
            variableManager.OnVariableUpdated += HandleVariableUpdated;
            unlockableManager.OnItemUpdated += HandleUnlockableUpdated;

            stateManager.OnGameLoadFinished += HandleOnGameLoadFinished;
            stateManager.OnResetFinished += ResetLists;
            stateManager.OnRollbackFinished += ResetLists;

            scriptPlayer.AddPostExecutionTask(HandlePlayedCommand);

            Initialised = true; 
        }
        public virtual void DestroySceneAssistant()
        {
            if (Initialised)
            {
                ObjectList.Clear();
                CustomVarList.Clear();
                UnlockablesList.Clear();

                variableManager.OnVariableUpdated -= HandleVariableUpdated;
                unlockableManager.OnItemUpdated -= HandleUnlockableUpdated;
                stateManager.OnGameLoadFinished -= HandleOnGameLoadFinished;
                stateManager.OnResetFinished -= ResetLists;
                stateManager.OnRollbackFinished -= ResetLists;

                Initialised = false;
            }
        }

        private void HandleOnGameLoadFinished(GameSaveLoadArgs obj) => ResetLists();

        protected virtual void ResetLists()
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




