// Copyright 2022 idaocracy. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Naninovel;
using Naninovel.UI;
using System;
using System.Linq;
using NaninovelSceneAssistant;



public class NaninovelSceneAssistantOld : EditorWindow
{
    private struct SceneObject
    {
        public string ObjId;
        public string ObjType;
        public GameObject SceneObj;

        public SceneObject(string objId, string objType, GameObject gameObject)
        {
            ObjId = objId;
            ObjType = objType;
            SceneObj = gameObject;
        }
    }

    private ICharacterManager characterManager;
    private IBackgroundManager backgroundManager;
    private ISpawnManager spawnManager;
    private ICameraManager cameraManager;
    private ITextPrinterManager printerManager;
    private IChoiceHandlerManager choiceHandlerManager;
    private IStateManager stateManager;
    private IScriptPlayer scriptPlayer;

    private GUIStyle textFieldStyle;
    private string clipboardString = string.Empty;
    private bool usePosOverPosition = true;
    private int objIndex = 0;
    private SceneObject objId;

    private Vector2 scrollPos = Vector2.zero;

    private bool copyAppearance = true;
    private bool copyLookDirection = true;
    private bool copyTintColor = true;
    private bool copyZoom = true;
    private bool copyOrthographic = true;
    private bool copyCameraComponents = true;
    //private bool copySpawnParams = true;
    private bool[] copyPosition = new bool[] { true, true, true, true };
    private bool[] copyRotation = new bool[] { true, true, true, true };
    private bool[] copyScale = new bool[] { true, true, true, true };

    private bool[] copyObjectTypes = { true, true, true, true, true, true };

    private List<MonoBehaviour> cameraComponentList = new List<MonoBehaviour>();
    private List<SceneObject> objList = new List<SceneObject>();

    private Editor cameraEditor;


    [MenuItem("Naninovel/Scene Assistant", false, 350)]
    public static void ShowWindow() => EditorWindow.GetWindow<NaninovelSceneAssistantOld>("Naninovel Scene Assistant");

    public void OnHierarchyChange()
    {
        RefreshList();
        Repaint();
    }

    public void Awake()
    {
        RefreshList();
        Repaint();
        
    }

    private void RefreshList()
    {

        if (objList.Count.Equals(0)) objIndex = 0;
        if (Engine.Initialized)
        {
            characterManager = Engine.GetService<ICharacterManager>();
            backgroundManager = Engine.GetService<IBackgroundManager>();
            spawnManager = Engine.GetService<ISpawnManager>();
            cameraManager = Engine.GetService<ICameraManager>();
            printerManager = Engine.GetService<ITextPrinterManager>();
            choiceHandlerManager = Engine.GetService<IChoiceHandlerManager>();
            stateManager = Engine.GetService<IStateManager>();
            scriptPlayer = Engine.GetService<IScriptPlayer>();

            objList.Clear();

            foreach (ICharacterActor actor in characterManager.GetAllActors())
            {
                var actorObject = actor as MonoBehaviourActor<CharacterMetadata>;
                objList.Add(new SceneObject(actor.Id, "char", actorObject.GameObject));
            }

            foreach (IBackgroundActor actor in backgroundManager.GetAllActors())
            {
                var actorObject = actor as MonoBehaviourActor<BackgroundMetadata>;
                objList.Add(new SceneObject(actor.Id, "back id:", actorObject.GameObject));
            }

            foreach (SpawnedObject spawn in spawnManager.GetAllSpawned()) objList.Add(new SceneObject(spawn.Path, "spawn", spawn.GameObject));

            cameraComponentList.Clear();
            objList.Add(new SceneObject("Camera", "camera", cameraManager.Camera.gameObject));
            foreach (MonoBehaviour cameraComponent in cameraManager.Camera.GetComponents<MonoBehaviour>()) cameraComponentList.Add(cameraComponent);

            foreach (ITextPrinterActor printer in printerManager.GetAllActors())
            {
                var printerObject = printer as MonoBehaviourActor<TextPrinterMetadata>;
                objList.Add(new SceneObject(printer.Id, "printer", printerObject.GameObject));
            }
            
            foreach (IChoiceHandlerActor choiceHandler in choiceHandlerManager.GetAllActors())
            {
                var choiceHandlerObject = choiceHandler as MonoBehaviourActor<ChoiceHandlerMetadata>;
                objList.Add(new SceneObject(choiceHandler.Id, "choice", choiceHandlerObject.GameObject));
            }
            
        }
    }

    public void OnGUI()
    {
        GUILayout.Space(20);
        if (Engine.Initialized)
        {
            textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (GUILayout.Button("Copy command (@)", GUILayout.Height(20), GUILayout.Width(150))) GUIUtility.systemCopyBuffer = clipboardString = CopyCommand(objId, false);
            if (GUILayout.Button("Copy command ([])", GUILayout.Height(20), GUILayout.Width(150))) GUIUtility.systemCopyBuffer = clipboardString = CopyCommand(objId, true);
            if (GUILayout.Button("Copy all", GUILayout.Height(20), GUILayout.Width(150))) GUIUtility.systemCopyBuffer = clipboardString = CopyAllOrSelected(true);
            if (GUILayout.Button("Copy selected", GUILayout.Height(20), GUILayout.Width(150))) GUIUtility.systemCopyBuffer = clipboardString = CopyAllOrSelected(false);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            copyObjectTypes[0] = GUILayout.Toggle(copyObjectTypes[0], "Characters", GUILayout.Height(20));
            GUILayout.Space(5);
            copyObjectTypes[1] = GUILayout.Toggle(copyObjectTypes[1], "Backgrounds", GUILayout.Height(20));
            GUILayout.Space(5);
            copyObjectTypes[2] = GUILayout.Toggle(copyObjectTypes[2], "Spawns", GUILayout.Height(20));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            copyObjectTypes[3] = GUILayout.Toggle(copyObjectTypes[3], "Camera", GUILayout.Height(20));
            GUILayout.Space(5);
            copyObjectTypes[4] = GUILayout.Toggle(copyObjectTypes[4], "Printers", GUILayout.Height(20));
            GUILayout.Space(5);
            copyObjectTypes[5] = GUILayout.Toggle(copyObjectTypes[5], "Choices", GUILayout.Height(20));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select all", GUILayout.Height(20), GUILayout.Width(70))) for (int i = 0; i < copyObjectTypes.Length; i++) copyObjectTypes[i] = true;
            if (GUILayout.Button("Deselect All", GUILayout.Height(20), GUILayout.Width(80))) for (int i = 0; i < copyObjectTypes.Length; i++) copyObjectTypes[i] = false;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(5);

            IdField();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (Engine.GetConfiguration<StateConfiguration>().EnableStateRollback)
            {
                if (GUILayout.Button("Rollback", GUILayout.Height(20), GUILayout.Width(60))) Rollback();

            }
            if (GUILayout.Button("Nullify Transforms", GUILayout.Height(20), GUILayout.Width(120)))
            {
                switch (objId.ObjType)
                {
                    case ("char"):
                        characterManager.GetActor(objId.ObjId).Position = usePosOverPosition ?
                            Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(new Vector3(0.5f, 0.5f, Engine.GetConfiguration<CharactersConfiguration>().ZOffset))
                            : new Vector3(0, 0, Engine.GetConfiguration<CharactersConfiguration>().ZOffset);
                        characterManager.GetActor(objId.ObjId).Rotation = new Quaternion(0, 0, 0, 0);
                        characterManager.GetActor(objId.ObjId).Scale = new Vector3(1, 1, 1);
                        break;

                    case ("back id:"):
                        backgroundManager.GetActor(objId.ObjId).Position = usePosOverPosition ?
                            Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(new Vector3(0.5f, 0.5f, Engine.GetConfiguration<BackgroundsConfiguration>().ZOffset))
                            : new Vector3(0, 0, Engine.GetConfiguration<BackgroundsConfiguration>().ZOffset);
                        backgroundManager.GetActor(objId.ObjId).Rotation = new Quaternion(0, 0, 0, 0);
                        backgroundManager.GetActor(objId.ObjId).Scale = new Vector3(1, 1, 1);
                        break;

                    case ("spawn"):
                        spawnManager.GetSpawned(objId.ObjId).Transform.position = usePosOverPosition ?
                            Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(new Vector3(0.5f, 0.5f, 99))
                            : new Vector3(0, 0, 99);
                        spawnManager.GetSpawned(objId.ObjId).Transform.rotation = new Quaternion(0, 0, 0, 0);
                        spawnManager.GetSpawned(objId.ObjId).Transform.localScale = new Vector3(1, 1, 1);
                        break;

                    case ("camera"):
                        cameraManager.Offset = Engine.GetConfiguration<CameraConfiguration>().InitialPosition;
                        cameraManager.Rotation = new Quaternion(0, 0, 0, 0);
                        break;

                    case ("printer"):
                        printerManager.GetActor(objId.ObjId).Position = usePosOverPosition ?
                            Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(new Vector3(0.5f, 0.5f, Engine.GetConfiguration<TextPrintersConfiguration>().ZOffset))
                            : new Vector3(0, 0, Engine.GetConfiguration<TextPrintersConfiguration>().ZOffset);
                        printerManager.GetActor(objId.ObjId).Rotation = new Quaternion(0, 0, 0, 0);
                        printerManager.GetActor(objId.ObjId).Scale = new Vector3(1, 1, 1);
                        break;

                    case ("choice"):
                        foreach (ChoiceHandlerButton choice in objId.SceneObj.GetComponentsInChildren<ChoiceHandlerButton>()) choice.transform.localPosition = Vector2.zero;
                        break;

                }
            }


            if (GUILayout.Button("Select all", GUILayout.Height(20), GUILayout.Width(70)))
            {
                copyAppearance = copyLookDirection = copyTintColor = copyZoom = copyOrthographic = copyCameraComponents = true;

                for (int i = 0; i < copyPosition.Length; i++) copyPosition[i] = true;
                for (int i = 0; i < copyRotation.Length; i++) copyRotation[i] = true;
                for (int i = 0; i < copyScale.Length; i++) copyScale[i] = true;
            }
                
            if (GUILayout.Button("Deselect All", GUILayout.Height(20), GUILayout.Width(80)))
            {
                if (objId.ObjType.Equals("char") || objId.ObjType.Equals("back id:") || objId.ObjType.Equals("printer")) copyAppearance = false;
                if (objId.ObjType.Equals("char")) copyLookDirection = false;
                if (objId.ObjType.Equals("char") || objId.ObjType.Equals("back id:") || objId.ObjType.Equals("printer")) copyTintColor = false;
                if (objId.ObjType.Equals("camera")) copyZoom = copyOrthographic = copyCameraComponents = false;

                for (int i = 0; i < copyPosition.Length; i++) copyPosition[i] = false;
                if (!objId.ObjType.Equals("choice")) for (int i = 0; i < copyRotation.Length; i++) copyRotation[i] = false;
                if (!objId.ObjType.Equals("camera") || !objId.ObjType.Equals("choice")) for (int i = 0; i < copyScale.Length; i++) copyScale[i] = false;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Clipboard", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
            clipboardString = EditorGUILayout.TextArea(clipboardString, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            usePosOverPosition = GUILayout.Toggle(usePosOverPosition, "Copy Pos instead of Position");
            GUILayout.EndVertical();
            GUILayout.Space(30);
        }
        else GUILayout.Label("Naninovel engine is not initialized.");
    }

    private async void Rollback()
    {
        await stateManager.RollbackAsync(s => s.PlayerRollbackAllowed);
    }

    private void IdField()
    {
        if (objList.Count > 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (GUILayout.Button("Id", GUILayout.Width(140), GUILayout.Height(20))) clipboardString = objId.ObjId;
            objIndex = EditorGUILayout.Popup(objIndex, objList.Select(p => p.ObjId.ToString()).ToArray(), textFieldStyle, GUILayout.Height(20), GUILayout.Width(140));
            objId = objList[objIndex];
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select object", GUILayout.Width(140), GUILayout.Height(20)))
            {
                Selection.activeGameObject = objId.SceneObj;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // CHARACTER TYPE
            if (objId.ObjType.Equals("char"))
            {
                GUILayout.BeginVertical();
                AppearanceField(characterManager.GetActor(objId.ObjId).Appearance);
                characterManager.GetActor(objId.ObjId).LookDirection = LookDirectionField(characterManager.GetActor(objId.ObjId).LookDirection);
                characterManager.GetActor(objId.ObjId).TintColor = TintColorField(characterManager.GetActor(objId.ObjId).TintColor);
                GUILayout.Space(10);
                if (usePosOverPosition) characterManager.GetActor(objId.ObjId).Position = PosField(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(characterManager.GetActor(objId.ObjId).Position));
                else characterManager.GetActor(objId.ObjId).Position = PositionField(characterManager.GetActor(objId.ObjId).Position);
                characterManager.GetActor(objId.ObjId).Rotation = RotationField(characterManager.GetActor(objId.ObjId).Rotation);
                characterManager.GetActor(objId.ObjId).Scale = ScaleField(characterManager.GetActor(objId.ObjId).Scale);
                GUILayout.EndVertical();
            }

            // BACKGROUND TYPE
            if (objId.ObjType.Equals("back id:"))
            {
                GUILayout.BeginVertical();
                AppearanceField(backgroundManager.GetActor(objId.ObjId).Appearance);
                backgroundManager.GetActor(objId.ObjId).TintColor = TintColorField(backgroundManager.GetActor(objId.ObjId).TintColor);
                GUILayout.Space(10);
                if (usePosOverPosition) backgroundManager.GetActor(objId.ObjId).Position = PosField(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(backgroundManager.GetActor(objId.ObjId).Position));
                else backgroundManager.GetActor(objId.ObjId).Position = PositionField(backgroundManager.GetActor(objId.ObjId).Position);
                backgroundManager.GetActor(objId.ObjId).Rotation = RotationField(backgroundManager.GetActor(objId.ObjId).Rotation);
                backgroundManager.GetActor(objId.ObjId).Scale = ScaleField(backgroundManager.GetActor(objId.ObjId).Scale);
                GUILayout.EndVertical();
            }

            // SPAWN TYPE
            if (objId.ObjType.Equals("spawn"))
            {
                GUILayout.BeginVertical();
                if (usePosOverPosition) spawnManager.GetSpawned(objId.ObjId).Transform.position = PosField(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(spawnManager.GetSpawned(objId.ObjId).Transform.position));
                else spawnManager.GetSpawned(objId.ObjId).Transform.position = PositionField(spawnManager.GetSpawned(objId.ObjId).Transform.position);
                spawnManager.GetSpawned(objId.ObjId).Transform.rotation = RotationField(spawnManager.GetSpawned(objId.ObjId).Transform.rotation);
                spawnManager.GetSpawned(objId.ObjId).Transform.localScale = ScaleField(spawnManager.GetSpawned(objId.ObjId).Transform.localScale);

                //SpawnParamsField();

                GUILayout.EndVertical();
            }

            // CAMERA TYPE
            if (objId.ObjType.Equals("camera"))
            {
                cameraEditor.DrawDefaultInspector();
                cameraEditor.OnInspectorGUI();

                //GUILayout.BeginVertical();
                //cameraManager.Zoom = ZoomField(cameraManager.Zoom);
                //cameraManager.Orthographic = OrthographicField(cameraManager.Orthographic);
                //GUILayout.Space(10);
                //cameraManager.Offset = PositionField(cameraManager.Offset);
                //cameraManager.Rotation = RotationField(cameraManager.Rotation);
                //GUILayout.Space(10);

                //if (cameraComponentList.Count > 0)
                //{
                //    GUILayout.BeginHorizontal();
                //    copyCameraComponents = GUILayout.Toggle(copyCameraComponents, "", GUILayout.Width(20), GUILayout.Height(20));
                //    if (GUILayout.Button("Camera Components", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = string.Join(",", cameraComponentList.Select(x => x.GetType().Name + "." + x.enabled.ToString().ToLower()).ToArray());
                //    GUILayout.EndHorizontal();

                //    foreach (var cameraComponent in cameraComponentList)
                //    {
                //        GUILayout.BeginHorizontal();
                //        GUILayout.Space(25);
                //        cameraComponent.enabled = CameraComponentsField(cameraComponent, cameraComponent.enabled);
                //        GUILayout.EndHorizontal();
                //    }
                //}
                //GUILayout.EndVertical();
            }

            // PRINTER TYPE
            if (objId.ObjType.Equals("printer"))
            {
                GUILayout.BeginVertical();
                AppearanceField(printerManager.GetActor(objId.ObjId).Appearance);
                printerManager.GetActor(objId.ObjId).TintColor = TintColorField(printerManager.GetActor(objId.ObjId).TintColor);
                GUILayout.Space(10);
                if (usePosOverPosition) printerManager.GetActor(objId.ObjId).Position = PosField(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(printerManager.GetActor(objId.ObjId).Position));
                else printerManager.GetActor(objId.ObjId).Position = PositionField(printerManager.GetActor(objId.ObjId).Position);
                printerManager.GetActor(objId.ObjId).Rotation = RotationField(printerManager.GetActor(objId.ObjId).Rotation);
                printerManager.GetActor(objId.ObjId).Scale = ScaleField(printerManager.GetActor(objId.ObjId).Scale);
                GUILayout.EndVertical();
            }

            // CHOICE HANDLER TYPE
            if (objId.ObjType.Equals("choice"))
            {
                GUILayout.BeginVertical();
                foreach (ChoiceHandlerButton choice in objList[objIndex].SceneObj.GetComponentsInChildren<ChoiceHandlerButton>())
                {
                    choice.transform.localPosition = ChoicePosField(choice.transform.localPosition, choice.ChoiceState.Summary);
                }
                GUILayout.EndVertical();
            }

        }
    }

    private void AppearanceField(string appearance)
    {
        GUILayout.BeginHorizontal();
        copyAppearance = GUILayout.Toggle(copyAppearance, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Appearance", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = appearance;
        appearance = EditorGUILayout.DelayedTextField(appearance, textFieldStyle, GUILayout.Height(20), GUILayout.Width(180));
        GUILayout.EndHorizontal();

        if (objId.ObjType.Equals("char") && appearance != characterManager.GetActor(objId.ObjId).Appearance) characterManager.GetActor(objId.ObjId).Appearance = appearance;
        if (objId.ObjType.Equals("back id:") && appearance != backgroundManager.GetActor(objId.ObjId).Appearance) backgroundManager.GetActor(objId.ObjId).Appearance = appearance;
        if (objId.ObjType.Equals("printer") && appearance != printerManager.GetActor(objId.ObjId).Appearance) printerManager.GetActor(objId.ObjId).Appearance = appearance;

    }

    private CharacterLookDirection LookDirectionField(CharacterLookDirection characterLookDirection)
    {
        GUILayout.BeginHorizontal();
        string[] options = new string[] { "Center", "Left", "Right" };
        var lookIndex = Array.IndexOf(options, characterLookDirection.ToString());


        copyLookDirection = GUILayout.Toggle(copyLookDirection, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Look Direction", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = options[lookIndex];
        lookIndex = EditorGUILayout.Popup(lookIndex, options, textFieldStyle, GUILayout.Height(20), GUILayout.Width(180));
        GUILayout.EndHorizontal();

        return (CharacterLookDirection)lookIndex;
    }

    private Color TintColorField(Color tintColor)
    {
        GUILayout.BeginHorizontal();
        copyTintColor = GUILayout.Toggle(copyTintColor, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Tint Color", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = "#" + ColorUtility.ToHtmlStringRGBA(tintColor);
        tintColor = EditorGUILayout.ColorField(tintColor, GUILayout.Height(20), GUILayout.Width(180));
        GUILayout.EndHorizontal();

        return tintColor;
    }

    private bool OrthographicField(bool orthographic)
    {
        string[] options = new string[] { "True", "False" };
        var orthoIndex = Array.IndexOf(options, orthographic.ToString());

        GUILayout.BeginHorizontal();
        copyOrthographic = GUILayout.Toggle(copyOrthographic, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Orthographic", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = options[orthoIndex].ToLower();
        orthoIndex = EditorGUILayout.Popup(orthoIndex, options, textFieldStyle, GUILayout.Height(20), GUILayout.Width(180));
        GUILayout.EndHorizontal();

        return bool.Parse(options[orthoIndex]);
    }

    private float ZoomField(float zoom)
    {
        GUILayout.BeginHorizontal();
        copyZoom = GUILayout.Toggle(copyZoom, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Zoom", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = zoom.ToString("0.##");
        zoom = EditorGUILayout.Slider(zoom, 0f, 1f, GUILayout.Width(180), GUILayout.Height(20));
        GUILayout.EndHorizontal();

        return zoom;
    }

    private Vector2 ChoicePosField(Vector2 position, string label)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(130));
        if (GUILayout.Button("Pos", GUILayout.Width(50), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = position.x + "," + position.y;

        position = EditorGUILayout.Vector2Field("", position, GUILayout.Width(130), GUILayout.Height(20));

        copyPosition[1] = GUILayout.Toggle(copyPosition[1], "", GUILayout.Width(20), GUILayout.Height(20));
        copyPosition[2] = GUILayout.Toggle(copyPosition[2], "", GUILayout.Width(20), GUILayout.Height(20));

        GUILayout.EndHorizontal();

        return position;
    }

    private bool CameraComponentsField(MonoBehaviour cameraComponent, bool enabled)
    {
        string[] options = new string[] { "True", "False" };
        var boolIndex = Array.IndexOf(options, enabled.ToString());

        if (GUILayout.Button(cameraComponent.GetType().Name, GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = cameraComponent.GetType().Name + "." + options[boolIndex].ToLower();

        boolIndex = EditorGUILayout.Popup(boolIndex, options, textFieldStyle, GUILayout.Height(20), GUILayout.Width(180));
        return bool.Parse(options[boolIndex]);

    }

    //private string SpawnParamsField()
    //{

    //    EditorGUIUtility.labelWidth = 190;


    //    GUILayout.Space(5);
    //    GUILayout.BeginHorizontal();
    //    copySpawnParams = GUILayout.Toggle(copySpawnParams, "", GUILayout.Width(20), GUILayout.Height(20));
    //    if (GUILayout.Button("Spawn Parameters", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString =
    //    objId.SceneObj.GetComponent<ISceneAssistant>()?.SceneAssistantParameters() ?? null;
    //    GUILayout.EndHorizontal();

    //    GUILayout.BeginVertical();
    //    var spawnString = objId.SceneObj.GetComponent<ISceneAssistant>()?.SceneAssistantParameters();
    //    GUILayout.EndHorizontal();
    //    return spawnString;
    //}

    private Vector3 PositionField(Vector3 position)
    {
        GUILayout.BeginHorizontal();
        copyPosition[0] = GUILayout.Toggle(copyPosition[0], "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button(objList[objIndex].ObjType.Equals("camera") ? "Offset" : "Position", GUILayout.Width(150), GUILayout.Height(20)))
            GUIUtility.systemCopyBuffer = clipboardString = CopyVector(position, copyPosition);
        position = EditorGUILayout.Vector3Field("", position, GUILayout.Width(180), GUILayout.Height(20));

        copyPosition[1] = GUILayout.Toggle(copyPosition[1], "", GUILayout.Width(20), GUILayout.Height(20));
        copyPosition[2] = GUILayout.Toggle(copyPosition[2], "", GUILayout.Width(20), GUILayout.Height(20));
        copyPosition[3] = GUILayout.Toggle(copyPosition[3], "", GUILayout.Width(20), GUILayout.Height(20));
        GUILayout.EndHorizontal();

        return position;
    }

    private Vector3 PosField(Vector3 pos)
    {
        GUILayout.BeginHorizontal();

        copyPosition[0] = GUILayout.Toggle(copyPosition[0], "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Pos", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = CopyVector(new Vector3(pos.x * 100, pos.y * 100, pos.z), copyPosition);

        pos.x = pos.x * 100;
        pos.y = pos.y * 100;

        pos = EditorGUILayout.Vector3Field("", pos, GUILayout.Width(180), GUILayout.Height(20));

        for (int i = 1; i < copyPosition.Length; i++) copyPosition[i] = GUILayout.Toggle(copyPosition[i], "", GUILayout.Width(20), GUILayout.Height(20));

        pos.x = pos.x / 100;
        pos.y = pos.y / 100;

        GUILayout.EndHorizontal();

        return Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(pos);

    }

    private Quaternion RotationField(Quaternion rotation)
    {
        GUILayout.BeginHorizontal();

        copyRotation[0] = GUILayout.Toggle(copyRotation[0], "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Rotation", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = CopyVector(rotation.eulerAngles, copyRotation);

        rotation.eulerAngles = EditorGUILayout.Vector3Field("", rotation.eulerAngles, GUILayout.Width(180), GUILayout.Height(20));

        for (int i = 1; i < copyRotation.Length; i++) copyRotation[i] = GUILayout.Toggle(copyRotation[i], "", GUILayout.Width(20), GUILayout.Height(20));

        GUILayout.EndHorizontal();

        return rotation;
    }

    private Vector3 ScaleField(Vector3 scale)
    {
        GUILayout.BeginHorizontal();

        copyScale[0] = GUILayout.Toggle(copyScale[0], "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Scale", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = clipboardString = CopyVector(scale, copyScale);

        scale = EditorGUILayout.Vector3Field("", scale, GUILayout.Width(180), GUILayout.Height(20));

        for (int i = 1; i < copyScale.Length; i++) copyScale[i] = GUILayout.Toggle(copyScale[i], "", GUILayout.Width(20), GUILayout.Height(20));

        GUILayout.EndHorizontal();

        return scale;
    }

    private string CopyCommand(SceneObject obj, bool inlined)
    {
        var commandString = obj.ObjType + " " + obj.ObjId;

        if (obj.ObjType.Equals("char"))
        {
            commandString = commandString +
            (copyAppearance && characterManager.GetActor(obj.ObjId).Appearance != null ? "." + characterManager.GetActor(obj.ObjId).Appearance : string.Empty) +
            (copyLookDirection ? " look:" + characterManager.GetActor(obj.ObjId).LookDirection.ToString().ToLower() : string.Empty) +
            (copyTintColor ? " tint:#" + ColorUtility.ToHtmlStringRGBA(characterManager.GetActor(obj.ObjId).TintColor) : string.Empty) +
            (copyPosition[0] && usePosOverPosition ? " pos:" + CopyVector(ParsePos(characterManager.GetActor(obj.ObjId).Position), copyPosition) : string.Empty) +
            (copyPosition[0] && !usePosOverPosition ? " position:" + CopyVector(characterManager.GetActor(obj.ObjId).Position, copyPosition) : string.Empty) +
            (copyRotation[0] ? " rotation:" + CopyVector(characterManager.GetActor(obj.ObjId).Rotation.eulerAngles, copyRotation) : string.Empty) +
            (copyScale[0] ? " scale:" + CopyVector(characterManager.GetActor(obj.ObjId).Scale, copyScale) : string.Empty);
        }
        else if (obj.ObjType.Equals("back id:"))
        {
            commandString = obj.ObjType + obj.ObjId +
            (copyAppearance && backgroundManager.GetActor(obj.ObjId).Appearance != null ? " appearance:" + backgroundManager.GetActor(obj.ObjId).Appearance : string.Empty) +
            (copyTintColor ? " tint:#" + ColorUtility.ToHtmlStringRGBA(backgroundManager.GetActor(obj.ObjId).TintColor) : string.Empty) +
            (copyPosition[0] && usePosOverPosition ? " pos:" + CopyVector(ParsePos(backgroundManager.GetActor(obj.ObjId).Position), copyPosition) : string.Empty) +
            (copyPosition[0] && !usePosOverPosition ? " position:" + CopyVector(backgroundManager.GetActor(obj.ObjId).Position, copyPosition) : string.Empty) +
            (copyRotation[0] ? " rotation:" + CopyVector(backgroundManager.GetActor(obj.ObjId).Rotation.eulerAngles, copyRotation) : string.Empty) +
            (copyScale[0] ? " scale:" + CopyVector(backgroundManager.GetActor(obj.ObjId).Scale, copyScale) : string.Empty);
        }
        else if (obj.ObjType.Equals("spawn"))
        {
            commandString = commandString +
            (copyPosition[0] && usePosOverPosition ? " pos:" + CopyVector(ParsePos(spawnManager.GetSpawned(obj.ObjId).Transform.position), copyPosition) : string.Empty) +
            (copyPosition[0] && !usePosOverPosition ? " position:" + CopyVector(spawnManager.GetSpawned(obj.ObjId).Transform.position, copyPosition) : string.Empty) +
            (copyRotation[0] ? " rotation:" + CopyVector(spawnManager.GetSpawned(obj.ObjId).Transform.eulerAngles, copyRotation) : string.Empty) +
            (copyScale[0] ? " scale:" + CopyVector(spawnManager.GetSpawned(obj.ObjId).Transform.localScale, copyScale) : string.Empty);
            //+
            //(copySpawnParams && !CheckSpawnParamMethodIsNull(obj) ? " params:" + SpawnParamsField() : string.Empty);
        }
        else if (obj.ObjType.Equals("camera"))
        {
            commandString = "camera" + 
            (copyZoom ? " zoom:" + cameraManager.Zoom : string.Empty) +
            (copyOrthographic ? " orthographic:" + cameraManager.Orthographic.ToString().ToLower() : string.Empty) +
            (copyPosition[0] ? " offset:" + CopyVector(cameraManager.Offset, copyPosition) : string.Empty) +
            (copyRotation[0] ? " rotation:" + CopyVector(cameraManager.Rotation.eulerAngles, copyRotation) : string.Empty) +
            (copyCameraComponents && cameraComponentList.Count > 0 ? " set:" + string.Join(",", cameraComponentList.Select(x => x.GetType().Name + "." + x.enabled.ToString().ToLower()).ToArray()) : string.Empty);
        }
        else if (obj.ObjType.Equals("printer"))
        {
            commandString = commandString + 
            (copyAppearance && printerManager.GetActor(obj.ObjId).Appearance != null ? "." + printerManager.GetActor(obj.ObjId).Appearance : string.Empty) +
            (copyTintColor ? " tint:#" + ColorUtility.ToHtmlStringRGBA(printerManager.GetActor(obj.ObjId).TintColor) : string.Empty) +
            (copyPosition[0] && usePosOverPosition ? " pos:" + CopyVector(ParsePos(printerManager.GetActor(obj.ObjId).Position), copyPosition) : string.Empty) +
            (copyPosition[0] && !usePosOverPosition ? " position:" + CopyVector(printerManager.GetActor(obj.ObjId).Position, copyPosition) : string.Empty) +
            (copyRotation[0] ? " rotation:" + CopyVector(printerManager.GetActor(obj.ObjId).Rotation.eulerAngles, copyRotation) : string.Empty) +
            (copyScale[0] ? " scale:" + CopyVector(printerManager.GetActor(obj.ObjId).Scale, copyScale) : string.Empty);
        }
        else if (obj.ObjType.Equals("choice"))
        {
            foreach (ChoiceHandlerButton choice in obj.SceneObj.GetComponentsInChildren<ChoiceHandlerButton>())
            {
                commandString = (inlined ? "[" : "@") + "choice " + "\"" + choice.ChoiceState.Summary + "\"" + " handler:" + objId + " pos:" + choice.transform.localPosition.x + "," + choice.transform.localPosition.y + (inlined ? "]" : "") + "\n";
            }
            return commandString;
        }

        if (string.IsNullOrEmpty(commandString)) return string.Empty;
        if (inlined) return "[" + commandString + "]";
        else return "@" + commandString;
    }

    private static string CopyVector(Vector3 vector, bool[] copyValues) => (copyValues[1] ? vector.x.ToString("0.##") : string.Empty) + "," + (copyValues[2] ? vector.y.ToString("0.##") : string.Empty) + "," + (copyValues[3] ? vector.z.ToString("0.##") : string.Empty);
    private static Vector3 ParsePos(Vector3 position) => new Vector3(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(position).x * 100, Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(position).y * 100, position.z);
    private string CopyAllOrSelected(bool copyAll)
    {
        var allString = string.Empty;

        copyAppearance = copyLookDirection = copyTintColor = copyZoom = copyOrthographic = copyCameraComponents = true;

        for (int i = 0; i < copyPosition.Length; i++) copyPosition[i] = true;
        for (int i = 0; i < copyRotation.Length; i++) copyRotation[i] = true;
        for (int i = 0; i < copyScale.Length; i++) copyScale[i] = true;

        foreach (var obj in objList)
        {
            if (!copyAll)
            {
                if (!copyObjectTypes[0] && obj.ObjType.Equals("char")) continue;
                if (!copyObjectTypes[1] && obj.ObjType.Equals("back id:")) continue;
                if (!copyObjectTypes[2] && obj.ObjType.Equals("spawn")) continue;
                if (!copyObjectTypes[3] && obj.ObjType.Equals("camera")) continue;
                if (!copyObjectTypes[4] && obj.ObjType.Equals("printer")) continue;
                if (!copyObjectTypes[5] && obj.ObjType.Equals("choice")) continue;
                allString = allString + CopyCommand(obj, false) + "\n";
            }
            else allString = allString + CopyCommand(obj, false) + "";
        }
        return allString;
    }
}



