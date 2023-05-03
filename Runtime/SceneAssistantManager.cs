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
        private IScriptManager scriptManager;
        private ICustomVariableManager variableManager;
        private IUnlockableManager unlockableManager;
        private IStateManager stateManager;

        public Dictionary<string, INaninovelObjectData> ObjectList { get; protected set; } = new Dictionary<string, INaninovelObjectData>();
        public Dictionary<Type, bool> ObjectTypeList { get; protected set; } = new Dictionary<Type, bool>();
        public SortedList<string, VariableData> CustomVarList { get; protected set; } = new SortedList<string, VariableData> { };
        public SortedList<string, UnlockableData> UnlockablesList { get; protected set; } = new SortedList<string, UnlockableData> { };
        public IReadOnlyCollection<string> ScriptsList { get; protected set; }
        public bool Initialised { get; protected set; } = false;

        public Action OnSceneAssistantReset;

        public virtual UniTask InitializeServiceAsync() => UniTask.CompletedTask;

        public virtual void ResetService()
        {
            if(Initialised) RebuildLists();
        }

        public virtual void DestroyService()
        {
            if (Initialised) DestroySceneAssistant();
        }

        public virtual void GetServices()
        {
            actorServices = Engine.FindAllServices<IActorManager>();
            spawnManager = Engine.GetService<ISpawnManager>();
            scriptPlayer = Engine.GetService<IScriptPlayer>();
            variableManager = Engine.GetService<ICustomVariableManager>();
            unlockableManager = Engine.GetService<IUnlockableManager>();
            stateManager = Engine.GetService<IStateManager>();
            scriptManager = Engine.GetService<IScriptManager>();
        }

        public virtual async void InitializeSceneAssistant()
        {
            GetServices();
            ScriptsList = await scriptManager.LocateScriptsAsync();

            RebuildLists();
            variableManager.OnVariableUpdated += HandleVariableUpdated;
            unlockableManager.OnItemUpdated += HandleUnlockableUpdated;

            stateManager.OnGameLoadFinished += HandleOnGameLoadFinished;
            stateManager.OnResetFinished += RebuildLists;
            stateManager.OnRollbackFinished += RebuildLists;

            scriptPlayer.AddPostExecutionTask(HandlePlayedCommand);

            Initialised = true; 
        }


        public virtual void DestroySceneAssistant()
        {
            if (Initialised)
            {
                ClearObjectList();
                CustomVarList.Clear();
                UnlockablesList.Clear();

                variableManager.OnVariableUpdated -= HandleVariableUpdated;
                unlockableManager.OnItemUpdated -= HandleUnlockableUpdated;
                stateManager.OnGameLoadFinished -= HandleOnGameLoadFinished;
                stateManager.OnResetFinished -= RebuildLists;
                stateManager.OnRollbackFinished -= RebuildLists;

                scriptPlayer.RemovePostExecutionTask(HandlePlayedCommand);
                Initialised = false;
            }
        }

        public virtual UniTask HandlePlayedCommand(Command command)
        {
            RefreshObjectList();
            RefreshObjectTypeList();

            return UniTask.CompletedTask;
        }

        private void HandleOnGameLoadFinished(GameSaveLoadArgs obj) => RebuildLists();

        protected virtual void RebuildLists()
        {
            ClearObjectList();
            RefreshObjectList();
            OnSceneAssistantReset?.Invoke();

            CustomVarList.Clear();
            foreach (var variable in variableManager.GetAllVariables()) CustomVarList.Add(variable.Name, new VariableData(variable.Name));

            UnlockablesList.Clear();
            foreach (var unlockable in unlockableManager.GetAllItems()) UnlockablesList.Add(unlockable.Key, new UnlockableData(unlockable.Key));
        }

        private void ClearObjectList()
        {
            foreach (var obj in ObjectList)
            {
                if (obj.Value is IDisposable disposable) disposable.Dispose();
            }

            ObjectList.Clear();
        }

        protected void HandleVariableUpdated(CustomVariableUpdatedArgs args)
        {
            if (CustomVarList.ContainsKey(args.Name)) CustomVarList[args.Name].Value = args.Value;
            else CustomVarList.Add(args.Name, new VariableData(args.Name));
        }

        protected void HandleUnlockableUpdated(UnlockableItemUpdatedArgs args)
        {
            if (UnlockablesList.ContainsKey(args.Id)) UnlockablesList[args.Id].Value = args.Unlocked;
            else UnlockablesList.Add(args.Id, new UnlockableData(args.Id));
        }

        protected virtual void RefreshObjectList()
        {
            RefreshCamera();
            RefreshActorList();
            RefreshSpawnList();
            RefreshObjectTypeList();
            RefreshDynamicParameters();
        }

        private void RefreshDynamicParameters()
        {
            foreach (var obj in ObjectList)
            {
                if (obj.Value is IDynamicCommandParameter dynamic) dynamic.UpdateCommandParameters();
            }
        }

        private void RefreshCamera()
        {
            if (!ObjectExists(typeof(CameraData)))
            {
                var camera = new CameraData();
                ObjectList.Add(camera.Id, camera);
            }
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

        protected bool ObjectExists(Type type, string id = null)
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