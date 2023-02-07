using Naninovel;
using Naninovel.Commands;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NaninovelSceneAssistant;
using Naninovel.UI;
using PlasticPipe.PlasticProtocol.Messages;

namespace NaninovelSceneAssistant
{
    [InitializeAtRuntime]
    public class SceneAssistantManager : IEngineService
    {
        public virtual SceneAssistantConfiguration Configuration { get; }

        private ICameraManager cameraManager;
        private ICharacterManager characterManager;
        private IBackgroundManager backgroundManager;
        private IChoiceHandlerManager choiceHandlerManager;
        private ITextPrinterManager textPrinterManager;
        private ISpawnManager spawnManager;
        private IScriptPlayer scriptPlayer;
        private ICustomVariableManager variableManager;
        private IUnlockableManager unlockableManager;

        protected IEngineService[] actorManagers { get; set; }

        public List<INaninovelObject> ObjectList { get; protected set; } = new List<INaninovelObject>();
        public SortedList<string, CustomVar> CustomVarList { get; protected set; } = new SortedList<string, CustomVar> { };
        public SortedList<string, Unlockable> UnlockablesList { get; protected set; } = new SortedList<string, Unlockable> { };

        public bool Initialized = false;
        public SceneAssistantManager(SceneAssistantConfiguration config, ICameraManager cameraManager, ICharacterManager characterManager, 
                IBackgroundManager backgroundManager, IChoiceHandlerManager choiceHandlerManager, ITextPrinterManager textPrinterManager, 
                ISpawnManager spawnManager, IScriptPlayer scriptPlayer, ICustomVariableManager variableManager, IUnlockableManager unlockableManager)
        {
            Configuration = config;
            this.cameraManager = cameraManager;
            this.characterManager = characterManager;
            this.backgroundManager = backgroundManager;
            this.choiceHandlerManager = choiceHandlerManager;
            this.textPrinterManager = textPrinterManager;
            this.spawnManager = spawnManager;
            this.scriptPlayer = scriptPlayer;
            this.variableManager = variableManager;
            this.unlockableManager = unlockableManager;
        }

        public virtual UniTask InitializeServiceAsync()
        {
        #if UNITY_EDITOR
            InitializeSceneAssistant();
            ObjectList.Add(new CameraObject());

    #endif

            return UniTask.CompletedTask;
        }

        public void ResetService()
        {            

        }

        public void DestroyService()
        {
        }

        public void InitializeSceneAssistant()
        {
            foreach (var variable in variableManager.GetAllVariables()) CustomVarList.Add(variable.Name, new CustomVar(variable.Name, variable.Value));
            foreach (var unlockable in unlockableManager.GetAllItems()) UnlockablesList.Add(unlockable.Key, new Unlockable(unlockable.Key, unlockable.Value));

            HandleCommand();
            scriptPlayer.AddPostExecutionTask(HandleCommand);
        }

        public void DestroySceneAssistant()
        {
            scriptPlayer.RemovePostExecutionTask(HandleCommand);
            ObjectList.Clear();
        }

        public void RefreshList()
        {
            //actorManagers = new IEngineServices[] { characterManager, backgroundManager, textPrinterManager, choiceHandlerManager };

            //foreach(var manager in actorManagers)
            //{
            //    foreach (actor in manager.GetAllActors()
            //    { 

            //    }
            //}
        }

        public virtual UniTask HandleCommand(Command command = null)
        {

            if (command is ModifyCharacter character && ActorExistsAndIsVisible<ICharacterManager, CharacterObject>(characterManager, character.IdAndAppearance.Name)) 
                ObjectList.Add(new CharacterObject(character.IdAndAppearance.Name));

            if (command is ModifyBackground background)
            {
                var backId = background.Id.HasValue ? background.Id.Value : "MainBackground";
                if (ActorExistsAndIsVisible<IBackgroundManager, BackgroundObject>(backgroundManager, backId))
                    ObjectList.Add(new BackgroundObject(backId));
            }

            if (command is PrintText textPrinter)
            {
                var printerId = textPrinter.PrinterId.HasValue ? textPrinter.PrinterId.Value : textPrinterManager.DefaultPrinterId;
                if (ActorExistsAndIsVisible<ITextPrinterManager, TextPrinterObject>(textPrinterManager, printerId)) 
                    ObjectList.Add(new TextPrinterObject(printerId));
            }

            if (command is AddChoice choice && ActorExistsAndIsVisible<IChoiceHandlerManager, ChoiceHandlerObject>(choiceHandlerManager, choice.HandlerId))
                ObjectList.Add(new ChoiceHandlerObject(choice.HandlerId));

            if (command is Spawn spawn) if(!ObjectExists<SpawnObject>(spawn.Path)) ObjectList.Add(new SpawnObject(spawn.Path));

            return UniTask.CompletedTask;
        }

        private bool ActorExistsAndIsVisible<TService, TActorObject>(TService service, string id)
            where TService : class, IActorManager
            where TActorObject : INaninovelObject
            => !ObjectExists<TActorObject>(id) && service.GetActor(id).Visible;
        

        private bool ObjectExists<NaninovelObject>(string id = null)
        => ObjectList.Contains(ObjectList.FirstOrDefault(o => o.Id == id));

        public string GetAllCommands()
        {
            var allString = String.Empty;

            foreach (var o in ObjectList)
            {
                Debug.Log(o.Id);
                Debug.Log(o.GetCommandLine());
                allString = allString + "@" + o.GetCommandLine() + "\n";
            }
            return allString;
        }
    }
}

public class SceneAssistant : EditorWindow, ISceneAssistantLayout
{
    public string[] Tabs { get; protected set; }
    protected INaninovelObject CurrentObject { get => sceneAssistantManager.ObjectList[objectIndex]; }
    protected string[] ObjectDropdown { get => sceneAssistantManager.ObjectList.Select(p => p.Id).ToArray(); }
    protected string ClipboardString { get => clipboardString; set { clipboardString = value; EditorGUIUtility.systemCopyBuffer = value; if (logResults) Debug.Log(value); } }

    private string[] typeList = new string[] { "Characters", "Backgrounds", "Text Printers", "Choices", "Camera", "Spawns" };
    private bool[] typeBools = new bool[] { true, true, true, true, true, true };

    private SceneAssistantManager sceneAssistantManager;
    private int objectIndex = 0;
    private int tabIndex = 0;
    private string clipboardString = string.Empty;
    private Vector2 scrollPos = default;
    private bool logResults;
    private ISceneAssistantLayout sceneAssistantLayout;

    [MenuItem("Naninovel/New Scene Assistant", false, 350)]
    public static void ShowWindow() => GetWindow<SceneAssistant>("Naninovel Scene Assistant");

    private void Awake()
    {
        EditorGUIUtility.labelWidth = 150;
        Tabs = new string[] { "Objects", "Custom Variables", "Unlockables" };
        sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
        sceneAssistantLayout = this is ISceneAssistantLayout  sceneAssistant ? sceneAssistant : null;
    }

    public void OnGUI()
    {
        if (Engine.Initialized && Engine.TryGetService(out sceneAssistantManager))
        {
            ShowTabs();
        }
        else EditorGUILayout.LabelField("Naninovel is not initialized.");
    }


    private void ShowTabs()
    {
        GUILayout.Space(10f);
        tabIndex = GUILayout.Toolbar(tabIndex, Tabs, EditorStyles.toolbarButton);
        GUILayout.Space(10f);

        switch (tabIndex)
        {
            case 0:
                ShowSceneAssistant();
                ShowCustomVariables(CurrentObject.CustomVars, CurrentObject.Id);
                break;
            case 1:
                ShowCustomVariables(sceneAssistantManager.CustomVarList);
                break;
            case 2:
                ShowUnlockables(sceneAssistantManager.UnlockablesList);
                break;
        }
    }

    private void ShowSceneAssistant()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical();

        DrawCommandButtons();
        GUILayout.Space(5);
        ShowTypeOptions();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        DrawIdField();

        EditorGUILayout.Space(10);

        ShowCommandParameters(CurrentObject.Params);

        ShowOptionButtons();

        EditorGUILayout.Space(5);
        EditorGUI.BeginDisabledGroup(true);
        clipboardString = EditorGUILayout.TextArea(clipboardString, new GUIStyle(GUI.skin.textField) { fontSize = 10, wordWrap = true}, GUILayout.Height(50));
        EditorGUI.EndDisabledGroup();
        logResults = EditorGUILayout.ToggleLeft("Log Results", logResults, EditorStyles.miniLabel);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }

    protected virtual void ShowOptionButtons()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Select all", EditorStyles.miniButton)) CurrentObject.Params.ForEach(p => p.Selected = true);
        if (GUILayout.Button("Deselect all", EditorStyles.miniButton)) CurrentObject.Params.ForEach(p => p.Selected = false);
        if (GUILayout.Button("Default", EditorStyles.miniButton)) CurrentObject.Params.ForEach(p => p.GetDefaultValue());
        if (GUILayout.Button("Rollback", EditorStyles.miniButton)) Rollback();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        async void Rollback() => await Engine.GetService<IStateManager>().RollbackAsync(s => s.PlayerRollbackAllowed);

    }


    protected virtual void DrawCommandButtons()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        if (ShowButton("Copy command (@)")) ClipboardString = "@" + CurrentObject.GetCommandLine();
        if (ShowButton("Copy command ([])")) ClipboardString = "[" + CurrentObject.GetCommandLine() + "]";
        if (ShowButton("Copy all")) ClipboardString = sceneAssistantManager.GetAllCommands();
        if (ShowButton("Copy selected")) ClipboardString = sceneAssistantManager.GetAllCommands();
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    private void DrawIdField()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        if (ShowButton("Id")) ClipboardString = CurrentObject.Id;
        objectIndex = EditorGUILayout.Popup(objectIndex, ObjectDropdown, new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(150));
        if (ShowButton("Inspect object")) Selection.activeGameObject = CurrentObject.GameObject;
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void ShowTypeOptions()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        for (int i = 0; i < typeList.Length; i++)
        {
            if (i == 3)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }

            typeBools[i] = EditorGUILayout.Toggle("", typeBools[i], GUILayout.Width(15));
            EditorGUILayout.LabelField(typeList[i], EditorStyles.miniLabel, GUILayout.Width(25 + typeList[i].Replace(" ","").Length * 4));
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    protected virtual void ShowCommandParameters(List<CommandParam> parameters)
    {
        if (parameters == null) return;

        if (CurrentObject.HasPosValues(out var posParamIndex, out var positionParamIndex))
        {
            parameters[posParamIndex].Selected = !parameters[positionParamIndex].Selected;
            parameters[positionParamIndex].Selected = !parameters[posParamIndex].Selected;
        }

        foreach (CommandParam param in parameters)
        {
            EditorGUILayout.BeginHorizontal();
            ShowValueOptions(param);
            param.DisplayField(sceneAssistantLayout);
            EditorGUILayout.EndHorizontal();
        }
    }

    protected virtual void ShowCustomVariables(SortedList<string, CustomVar> vars, string id = null)
    {
        if (vars == null) return;

        EditorGUILayout.BeginScrollView(scrollPos);
        if (id != null) EditorGUILayout.LabelField($"{id} Variables", EditorStyles.boldLabel);

        foreach (CustomVar variable in vars.Values)
        {
            EditorGUILayout.BeginHorizontal();
            variable.DisplayField();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    protected virtual void ShowUnlockables(SortedList<string, Unlockable> unlockables)
    {
        if (unlockables == null) return;

        EditorGUILayout.BeginScrollView(scrollPos);
        foreach (Unlockable unlockable in unlockables.Values)
        {
            EditorGUILayout.BeginHorizontal();
            unlockable.DisplayField();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }


    private void OnDestroy()
    {
        //if(sceneAssistantManager?.ObjectList?.Count > 0) sceneAssistantManager.DestroySceneAssistant();
        //Engine.OnInitializationFinished -= sceneAssistantManager.InitializeSceneAssistant;

    }

    public static bool ShowButton(string label)
    {
        if (GUILayout.Button(label, GUILayout.Width(150))) return true;
        else return false;
    }

    public void ShowValueOptions(CommandParam param)
    {
        if (param.HasOptions)
        {
            param.Selected = EditorGUILayout.Toggle(param.Selected, GUILayout.Width(20f));
            if (ShowButton(param.Id)) ClipboardString = param.GetValue;
        }
        else GUILayout.Label(param.Id.ToString());
    }

    public void Vector3Field(CommandParam param)  => param.setValue(EditorGUILayout.Vector3Field("", (Vector3)param.getValue()));
 
    public void SliderField(CommandParam param, float minValue, float maxValue) => param.setValue(EditorGUILayout.Slider((float)param.getValue(), minValue, maxValue));

    public void BoolField(CommandParam param) => param.setValue(EditorGUILayout.Toggle((bool)param.getValue()));
}

