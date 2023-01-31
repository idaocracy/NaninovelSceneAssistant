using Naninovel;
using Naninovel.Commands;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NaninovelSceneAssistant;

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

        public string[] ObjectDropdown { get => ObjectList.Select(p => p.Id).ToArray(); }
        public string ClipboardString { get => clipboardString; set { clipboardString = value; EditorGUIUtility.systemCopyBuffer = value; if (logResults) Debug.Log(value); } }
        public NaninovelObject CurrentlyDisplayedObject { get => ObjectList[objectIndex]; set { CurrentlyDisplayedObject.ShowParamValues(); } }

        private bool logResults = false;
        private int objectIndex  = 0;
        private string clipboardString = string.Empty;
        private List<NaninovelObject> ObjectList = new List<NaninovelObject>();

        public SceneAssistantManager(SceneAssistantConfiguration config, ICameraManager cameraManager,
                ICharacterManager characterManager, IBackgroundManager backgroundManager, IChoiceHandlerManager choiceHandlerManager, ITextPrinterManager textPrinterManager, ISpawnManager spawnManager, IScriptPlayer scriptPlayer)
        {
            Configuration = config;
            this.cameraManager = cameraManager;
            this.characterManager = characterManager;
            this.backgroundManager = backgroundManager;
            this.choiceHandlerManager = choiceHandlerManager;
            this.textPrinterManager = textPrinterManager;
            this.spawnManager = spawnManager;
            this.scriptPlayer = scriptPlayer;
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
        }

        public void InitializeSceneAssistant()
        {
            if (ObjectList.Count == 0) return;
            DisplayObjectList();
            scriptPlayer.AddPostExecutionTask(HandleCommandExecuted);
        }

        public void DeinitializeSceneAssistant()
        {
            scriptPlayer.RemovePostExecutionTask(HandleCommandExecuted);
            ObjectList.Clear();
        }

        protected virtual UniTask HandleCommandExecuted(Command command)
        {
            //// Add Character to the list
            //if (command is ModifyCharacter modifyChar && modifyChar.Visible) 
            //    if(!ObjectExists(modifyChar.Id)) ObjectList.Add(new ActorObject<ICharacterActor, CharacterMetadata>(characterManager.GetActor(modifyChar.Id), characterManager.Configuration.GetMetadataOrDefault(modifyChar.Id)));

           

            ////Add Background to the list
            //if (command is ModifyBackground modifyBack && modifyBack.Visible)
            //    if (!ObjectExists(modifyBack.Id)) ObjectList.Add(new ActorObject<IBackgroundActor, BackgroundMetadata>(backgroundManager.GetActor(modifyBack.Id), backgroundManager.Configuration.GetMetadataOrDefault(modifyBack.Id)));

            ////Add Text Printer to the list
            //if (command is ModifyTextPrinter modifyPrinter && modifyPrinter.Visible)
            //    if (!ObjectExists(modifyPrinter.Id))  ObjectList.Add(new ActorObject<ITextPrinterActor, TextPrinterMetadata>(textPrinterManager.GetActor(modifyPrinter.Id), textPrinterManager.Configuration.GetMetadataOrDefault(modifyPrinter.Id)));

            //// Add Spawn to the list. 
            //if (command is Spawn spawn)
            //    if (!ObjectExists(spawn.Path)) ObjectList.Add(new SpawnObject<SpawnedObject>(spawnManager.GetSpawned(spawn.Path)));

            //// Remove Spawn from the list.
            //if (command is DestroySpawned despawn)
            //    ObjectList.Remove(ObjectList.FirstOrDefault(o => o is SpawnObject<SpawnedObject> && o.Id == despawn.Path));

            return UniTask.CompletedTask;
        }

        private bool ObjectExists(string id) => ObjectList.Contains(ObjectList.FirstOrDefault(o => o.Id == id));

        private bool ObjectExists(NaninovelObject naninovelObject) => ObjectList.Contains(ObjectList.FirstOrDefault(o => o == naninovelObject));

        public virtual void AddToList(NaninovelObject naninovelObject)
        {
            if (!ObjectExists(naninovelObject)) ObjectList.Add(naninovelObject);
        }

        public virtual void RemoveFromList(string id)
        {
            if(!ObjectExists(id)) ObjectList.Remove(ObjectList.FirstOrDefault(o => o.Id == id));
        }

        public virtual void InitializeObjectList()
        {
            //Add the main camera to the list.
             if(!ObjectExists(nameof(cameraManager.Camera))) ObjectList.Add(new CameraObject<ICameraManager>(cameraManager));

            //Add all visible character actors to the list
            foreach (var c in characterManager.GetAllActors())
                if (!ObjectExists(c.Id) && c.Visible)
                    ObjectList.Add(new CharacterObject(characterManager.GetActor(c.Id), characterManager.Configuration.GetMetadataOrDefault(c.Id)));

            ////Add all visible background actors to the list
            //foreach (var b in backgroundManager.GetAllActors()) 
            //    if (!ObjectExists(b.Id) && b.Visible) 
            //        ObjectList.Add(new ActorObject<IBackgroundActor, BackgroundMetadata>(backgroundManager.GetActor(b.Id), backgroundManager.Configuration.GetMetadataOrDefault(b.Id)));

            ////Add all visible text printer actors to the list
            //foreach (var p in textPrinterManager.GetAllActors())
            //    if (!ObjectExists(p.Id) && p.Visible)
            //        ObjectList.Add(new ActorObject<ITextPrinterActor, TextPrinterMetadata>(textPrinterManager.GetActor(p.Id), textPrinterManager.Configuration.GetMetadataOrDefault(p.Id)));

            ////Add all visible choice handler actors to the list
            //foreach (var h in choiceHandlerManager.GetAllActors())
            //    if (!ObjectExists(h.Id) && h.Visible)
            //        ObjectList.Add(new ActorObject<IChoiceHandlerActor, ChoiceHandlerMetadata>(choiceHandlerManager.GetActor(h.Id), choiceHandlerManager.Configuration.GetMetadataOrDefault(h.Id)));
        }

        private void DisplayObjectList()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            DrawCommandButtons();

            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10f);

            DrawIdField();

            EditorGUILayout.Space(20f);

            CurrentlyDisplayedObject = ObjectList[objectIndex];

            clipboardString = GUILayout.TextArea(clipboardString, GUILayout.Height(250));

            logResults = EditorGUILayout.Toggle("Log Results", logResults);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
        }



        private void DrawCommandButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (SceneAssistantHelpers.ShowButton("Copy command (@)")) ClipboardString = "@" + CurrentlyDisplayedObject.GetCommandLine();
            if (SceneAssistantHelpers.ShowButton("Copy command ([])")) ClipboardString = "[" + CurrentlyDisplayedObject.GetCommandLine() + "]";
            if (SceneAssistantHelpers.ShowButton("Copy all")) ClipboardString = SceneAssistantHelpers.GetAllCommands(ObjectList);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawIdField()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (SceneAssistantHelpers.ShowButton("Id")) ClipboardString = CurrentlyDisplayedObject.Id;
            objectIndex = EditorGUILayout.Popup(objectIndex, ObjectDropdown, GUILayout.Width(150));
            if (SceneAssistantHelpers.ShowButton("Inspect object")) Selection.activeGameObject = CurrentlyDisplayedObject.GetGameObject();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}

public class SceneAssistant : EditorWindow
{
    [MenuItem("Naninovel/New Scene Assistant", false, 350)]
    public static void ShowWindow() => GetWindow<SceneAssistant>("Naninovel Scene Assistant");
    private SceneAssistantManager sceneAssistantManager;

    private void Awake()
    {
        sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
        sceneAssistantManager.InitializeObjectList();
    }


    public void OnGUI()
    {
        EditorGUIUtility.labelWidth = 150;
        if (Engine.Initialized) sceneAssistantManager?.InitializeSceneAssistant();
        else EditorGUILayout.LabelField("Naninovel is not initialized.");
    }

    public void OnDestroy()
    {

    }
}

