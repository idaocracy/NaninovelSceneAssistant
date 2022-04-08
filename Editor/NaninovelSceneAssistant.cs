// Copyright 2022 idaocracy. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Naninovel;
using Naninovel.UI;
using System;
using System.Linq;
using System.Reflection;

public class NaninovelSceneAssistant : EditorWindow
{

    private struct SceneObject
    {
        public GameObject SceneGameObject;
        public SceneObjectType SceneObjectType;

        public SceneObject(SceneObjectType sceneObjectType, GameObject sceneGameObject)
        {
            SceneObjectType = sceneObjectType;
            SceneGameObject = sceneGameObject;
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

    public enum SceneObjectType { Character, Background, Spawn, Camera, Printer, ChoiceHandler };
    private GUIStyle _textFieldStyle;
    private string _clipboardString = string.Empty;
    private bool _usePosOverPosition = true;
    private string _objectId = string.Empty;
    private int _objectIndex = 0;

    private Vector2 _scrollPos = Vector2.zero;

    private bool _copyAppearance = true;
    private bool _copyLookDirection = true;
    private bool _copyTintColor = true;
    private bool _copyZoom = true;
    private bool _copyOrthographic = true;
    private bool _copyCameraComponents = true;
    private bool _copySpawnParams = true;
    private bool _copyPosition = true;
    private bool _copyRotation = true;
    private bool _copyScale = true;

    private bool _copyPosX = true;
    private bool _copyPosY = true;
    private bool _copyPosZ = true;
    private bool _copyRotX = true;
    private bool _copyRotY = true;
    private bool _copyRotZ = true;
    private bool _copyScaleX = true;
    private bool _copyScaleY = true;
    private bool _copyScaleZ = true;

    private bool _copyCharacters = true;
    private bool _copyBackgrounds = true;
    private bool _copySpawns = true;
    private bool _copyCamera = true;
    private bool _copyPrinters = true;
    private bool _copyChoices = true;

    private List<MonoBehaviour> _cameraComponentList = new List<MonoBehaviour>();
    private Dictionary<string, SceneObject> _sceneObjectList = new Dictionary<string, SceneObject>();

    [MenuItem("Naninovel/Scene Assistant", false, 350)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<NaninovelSceneAssistant>("Naninovel Scene Assistant");
    }

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
        if (_sceneObjectList.Count.Equals(0)) _objectIndex = 0;
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

            _sceneObjectList.Clear();

            if (characterManager != null)
            {
                foreach (ICharacterActor actor in characterManager.GetAllActors())
                {
                    var actorObject = actor as MonoBehaviourActor<CharacterMetadata>;
                    _sceneObjectList.Add(actor.Id, new SceneObject(SceneObjectType.Character, actorObject.GameObject));
                }
            }
            if (backgroundManager != null)
            {
                foreach (IBackgroundActor actor in backgroundManager.GetAllActors())
                {
                    var actorObject = actor as MonoBehaviourActor<BackgroundMetadata>;
                    _sceneObjectList.Add(actor.Id, new SceneObject(SceneObjectType.Background, actorObject.GameObject));
                }
            }
            if (spawnManager != null)
            {
                foreach (SpawnedObject spawn in spawnManager.GetAllSpawned())
                {
                    _sceneObjectList.Add(spawn.Path, new SceneObject(SceneObjectType.Spawn, spawn.GameObject));
                }
            }
            if (cameraManager != null)
            {
                _cameraComponentList.Clear();
                _sceneObjectList.Add("Camera", new SceneObject(SceneObjectType.Camera, cameraManager.Camera.gameObject));
                foreach (MonoBehaviour cameraComponent in cameraManager.Camera.GetComponents<MonoBehaviour>()) _cameraComponentList.Add(cameraComponent);
            }
            if (printerManager != null)
            {
                foreach (ITextPrinterActor printer in printerManager.GetAllActors())
                {
                    var printerObject = printer as MonoBehaviourActor<TextPrinterMetadata>;
                    _sceneObjectList.Add(printer.Id, new SceneObject(SceneObjectType.Printer, printerObject.GameObject));
                }
            }
            if (choiceHandlerManager != null)
            {
                foreach (IChoiceHandlerActor choiceHandler in choiceHandlerManager.GetAllActors())
                {
                    var choiceHandlerObject = choiceHandler as MonoBehaviourActor<ChoiceHandlerMetadata>;
                    _sceneObjectList.Add(choiceHandler.Id, new SceneObject(SceneObjectType.ChoiceHandler, choiceHandlerObject.GameObject));
                }
            }
        }
    }

    public void OnGUI()
    {
        GUILayout.Space(20);
        if (Engine.Initialized)
        {
            _textFieldStyle = new GUIStyle(GUI.skin.textField);
            _textFieldStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (GUILayout.Button("Copy command (@)", GUILayout.Height(20), GUILayout.Width(150))) GUIUtility.systemCopyBuffer = _clipboardString = CopyCommand(_objectId, false);
            if (GUILayout.Button("Copy command ([])", GUILayout.Height(20), GUILayout.Width(150))) GUIUtility.systemCopyBuffer = _clipboardString = CopyCommand(_objectId, true);
            if (GUILayout.Button("Copy all", GUILayout.Height(20), GUILayout.Width(150))) GUIUtility.systemCopyBuffer = _clipboardString = CopyAllOrSelected(true);
            if (GUILayout.Button("Copy selected", GUILayout.Height(20), GUILayout.Width(150))) GUIUtility.systemCopyBuffer = _clipboardString = CopyAllOrSelected(false);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _copyCharacters = GUILayout.Toggle(_copyCharacters, "Characters", GUILayout.Height(20));
            GUILayout.Space(5);
            _copyBackgrounds = GUILayout.Toggle(_copyBackgrounds, "Backgrounds", GUILayout.Height(20));
            GUILayout.Space(5);
            _copySpawns = GUILayout.Toggle(_copySpawns, "Spawns", GUILayout.Height(20));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _copyCamera = GUILayout.Toggle(_copyCamera, "Camera", GUILayout.Height(20));
            GUILayout.Space(5);
            _copyPrinters = GUILayout.Toggle(_copyPrinters, "Printers", GUILayout.Height(20));
            GUILayout.Space(5);
            _copyChoices = GUILayout.Toggle(_copyChoices, "Choices", GUILayout.Height(20));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select all", GUILayout.Height(20), GUILayout.Width(70))) _copyCharacters = _copyBackgrounds = _copySpawns = _copyCamera = _copyPrinters = _copyChoices = true;
            if (GUILayout.Button("Deselect All", GUILayout.Height(20), GUILayout.Width(80))) _copyCharacters = _copyBackgrounds = _copySpawns = _copyCamera = _copyPrinters = _copyChoices = false;
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
                switch (_sceneObjectList[_objectId].SceneObjectType)
                {

                    case (SceneObjectType.Character):
                        characterManager.GetActor(_objectId).Position = _usePosOverPosition ?
                            Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(new Vector3(0.5f, 0.5f, Engine.GetConfiguration<CharactersConfiguration>().ZOffset))
                            : new Vector3(0, 0, Engine.GetConfiguration<CharactersConfiguration>().ZOffset);
                        characterManager.GetActor(_objectId).Rotation = new Quaternion(0, 0, 0, 0);
                        characterManager.GetActor(_objectId).Scale = new Vector3(1, 1, 1);
                        break;

                    case (SceneObjectType.Background):
                        backgroundManager.GetActor(_objectId).Position = _usePosOverPosition ?
                            Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(new Vector3(0.5f, 0.5f, Engine.GetConfiguration<BackgroundsConfiguration>().ZOffset))
                            : new Vector3(0, 0, Engine.GetConfiguration<BackgroundsConfiguration>().ZOffset);
                        backgroundManager.GetActor(_objectId).Rotation = new Quaternion(0, 0, 0, 0);
                        backgroundManager.GetActor(_objectId).Scale = new Vector3(1, 1, 1);
                        break;

                    case (SceneObjectType.Spawn):
                        spawnManager.GetSpawned(_objectId).Transform.position = _usePosOverPosition ?
                            Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(new Vector3(0.5f, 0.5f, 99))
                            : new Vector3(0, 0, 99);
                        spawnManager.GetSpawned(_objectId).Transform.rotation = new Quaternion(0, 0, 0, 0);
                        spawnManager.GetSpawned(_objectId).Transform.localScale = new Vector3(1, 1, 1);
                        break;

                    case (SceneObjectType.Camera):
                        cameraManager.Offset = Engine.GetConfiguration<CameraConfiguration>().InitialPosition;
                        cameraManager.Rotation = new Quaternion(0, 0, 0, 0);
                        break;

                    case (SceneObjectType.Printer):
                        printerManager.GetActor(_objectId).Position = _usePosOverPosition ?
                            Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(new Vector3(0.5f, 0.5f, Engine.GetConfiguration<TextPrintersConfiguration>().ZOffset))
                            : new Vector3(0, 0, Engine.GetConfiguration<TextPrintersConfiguration>().ZOffset);
                        printerManager.GetActor(_objectId).Rotation = new Quaternion(0, 0, 0, 0);
                        printerManager.GetActor(_objectId).Scale = new Vector3(1, 1, 1);
                        break;

                    case (SceneObjectType.ChoiceHandler):
                        foreach (ChoiceHandlerButton choice in _sceneObjectList[_objectId].SceneGameObject.GetComponentsInChildren<ChoiceHandlerButton>()) choice.transform.localPosition = Vector2.zero;
                        break;

                }
            }


            if (GUILayout.Button("Select all", GUILayout.Height(20), GUILayout.Width(70)))
            {
                _copyAppearance = _copyLookDirection = _copyTintColor = _copyZoom = _copyOrthographic = _copyCameraComponents = _copyPosition = _copyRotation = _copyScale
                    = _copyPosX = _copyPosY = _copyPosZ = _copyRotX = _copyRotY = _copyRotZ = _copyScaleX = _copyScaleY = _copyScaleZ = true;
            }

            if (GUILayout.Button("Deselect All", GUILayout.Height(20), GUILayout.Width(80)))
            {
                _copyAppearance = _copyLookDirection = _copyTintColor = _copyZoom = _copyOrthographic = _copyCameraComponents = _copyPosition = _copyRotation = _copyScale
                    = _copyPosX = _copyPosY = _copyPosZ = _copyRotX = _copyRotY = _copyRotZ = _copyScaleX = _copyScaleY = _copyScaleZ = false;
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
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true));
            _clipboardString = EditorGUILayout.TextArea(_clipboardString, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            _usePosOverPosition = GUILayout.Toggle(_usePosOverPosition, "Copy Pos instead of Position");
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

        if (_sceneObjectList.Count > 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (GUILayout.Button("Id", GUILayout.Width(140), GUILayout.Height(20))) _clipboardString = _objectId;
            _objectIndex = EditorGUILayout.Popup(_objectIndex, _sceneObjectList.Keys.ToArray(), _textFieldStyle, GUILayout.Height(20), GUILayout.Width(140));
            _objectId = _sceneObjectList.Keys.ToArray()[_objectIndex];
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // CHARACTER TYPE
            if (_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.Character))
            {
                GUILayout.BeginVertical();
                AppearanceField(characterManager.GetActor(_objectId).Appearance);
                characterManager.GetActor(_objectId).LookDirection = LookDirectionField(characterManager.GetActor(_objectId).LookDirection);
                characterManager.GetActor(_objectId).TintColor = TintColorField(characterManager.GetActor(_objectId).TintColor);
                GUILayout.Space(10);
                if (_usePosOverPosition) characterManager.GetActor(_objectId).Position = PosField(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(characterManager.GetActor(_objectId).Position));
                else characterManager.GetActor(_objectId).Position = PositionField(characterManager.GetActor(_objectId).Position);
                characterManager.GetActor(_objectId).Rotation = RotationField(characterManager.GetActor(_objectId).Rotation);
                characterManager.GetActor(_objectId).Scale = ScaleField(characterManager.GetActor(_objectId).Scale);
                GUILayout.EndVertical();
            }

            // BACKGROUND TYPE
            if (_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.Background))
            {
                GUILayout.BeginVertical();
                AppearanceField(backgroundManager.GetActor(_objectId).Appearance);
                backgroundManager.GetActor(_objectId).TintColor = TintColorField(backgroundManager.GetActor(_objectId).TintColor);
                GUILayout.Space(10);
                if (_usePosOverPosition) backgroundManager.GetActor(_objectId).Position = PosField(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(backgroundManager.GetActor(_objectId).Position));
                else backgroundManager.GetActor(_objectId).Position = PositionField(backgroundManager.GetActor(_objectId).Position);
                backgroundManager.GetActor(_objectId).Rotation = RotationField(backgroundManager.GetActor(_objectId).Rotation);
                backgroundManager.GetActor(_objectId).Scale = ScaleField(backgroundManager.GetActor(_objectId).Scale);
                GUILayout.EndVertical();
            }

            // SPAWN TYPE
            if (_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.Spawn))
            {
                GUILayout.BeginVertical();
                if (_usePosOverPosition) spawnManager.GetSpawned(_objectId).Transform.position = PosField(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(spawnManager.GetSpawned(_objectId).Transform.position));
                else spawnManager.GetSpawned(_objectId).Transform.position = PositionField(spawnManager.GetSpawned(_objectId).Transform.position);
                spawnManager.GetSpawned(_objectId).Transform.rotation = RotationField(spawnManager.GetSpawned(_objectId).Transform.rotation);
                spawnManager.GetSpawned(_objectId).Transform.localScale = ScaleField(spawnManager.GetSpawned(_objectId).Transform.localScale);

                if (!CheckSpawnParamMethodIsNull(_objectId)) SpawnParamsField();

                GUILayout.EndVertical();

            }

            // CAMERA TYPE
            if (_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.Camera))
            {
                GUILayout.BeginVertical();
                cameraManager.Zoom = ZoomField(cameraManager.Zoom);
                cameraManager.Orthographic = OrthographicField(cameraManager.Orthographic);
                GUILayout.Space(10);
                cameraManager.Offset = PositionField(cameraManager.Offset);
                cameraManager.Rotation = RotationField(cameraManager.Rotation);
                GUILayout.Space(10);


                if (_cameraComponentList.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    _copyCameraComponents = GUILayout.Toggle(_copyCameraComponents, "", GUILayout.Width(20), GUILayout.Height(20));
                    if (GUILayout.Button("Camera Components", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = string.Join(",", _cameraComponentList.Select(x => x.GetType().Name + "." + x.enabled.ToString().ToLower()).ToArray());
                    GUILayout.EndHorizontal();

                    foreach (var cameraComponent in _cameraComponentList)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(25);
                        cameraComponent.enabled = CameraComponentsField(cameraComponent, cameraComponent.enabled);
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndVertical();
            }

            // PRINTER TYPE
            if (_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.Printer))
            {
                GUILayout.BeginVertical();
                AppearanceField(printerManager.GetActor(_objectId).Appearance);
                printerManager.GetActor(_objectId).TintColor = TintColorField(printerManager.GetActor(_objectId).TintColor);
                GUILayout.Space(10);
                if (_usePosOverPosition) printerManager.GetActor(_objectId).Position = PosField(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(printerManager.GetActor(_objectId).Position));
                else printerManager.GetActor(_objectId).Position = PositionField(printerManager.GetActor(_objectId).Position);
                printerManager.GetActor(_objectId).Rotation = RotationField(printerManager.GetActor(_objectId).Rotation);
                printerManager.GetActor(_objectId).Scale = ScaleField(printerManager.GetActor(_objectId).Scale);
                GUILayout.EndVertical();
            }

            // CHOICE HANDLER TYPE
            if (_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.ChoiceHandler))
            {
                GUILayout.BeginVertical();
                foreach (ChoiceHandlerButton choice in _sceneObjectList[_objectId].SceneGameObject.GetComponentsInChildren<ChoiceHandlerButton>())
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
        _copyAppearance = GUILayout.Toggle(_copyAppearance, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Appearance", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = appearance;
        appearance = EditorGUILayout.DelayedTextField(appearance, _textFieldStyle, GUILayout.Height(20), GUILayout.Width(180));
        GUILayout.EndHorizontal();

        if (_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.Character) && appearance != characterManager.GetActor(_objectId).Appearance) characterManager.GetActor(_objectId).Appearance = appearance;
        if (_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.Background) && appearance != backgroundManager.GetActor(_objectId).Appearance) backgroundManager.GetActor(_objectId).Appearance = appearance;
        if (_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.Printer) && appearance != printerManager.GetActor(_objectId).Appearance) printerManager.GetActor(_objectId).Appearance = appearance;

    }

    private CharacterLookDirection LookDirectionField(CharacterLookDirection characterLookDirection)
    {
        GUILayout.BeginHorizontal();
        string[] options = new string[] { "Center", "Left", "Right" };
        var lookIndex = Array.IndexOf(options, characterLookDirection.ToString());


        _copyLookDirection = GUILayout.Toggle(_copyLookDirection, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Look Direction", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = options[lookIndex];
        lookIndex = EditorGUILayout.Popup(lookIndex, options, _textFieldStyle, GUILayout.Height(20), GUILayout.Width(180));
        GUILayout.EndHorizontal();

        return (CharacterLookDirection)lookIndex;
    }

    private Color TintColorField(Color tintColor)
    {
        GUILayout.BeginHorizontal();
        _copyTintColor = GUILayout.Toggle(_copyTintColor, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Tint Color", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = "#" + ColorUtility.ToHtmlStringRGBA(tintColor);
        tintColor = EditorGUILayout.ColorField(tintColor, GUILayout.Height(20), GUILayout.Width(180));
        GUILayout.EndHorizontal();

        return tintColor;
    }

    private bool OrthographicField(bool orthographic)
    {
        string[] options = new string[] { "True", "False" };
        var orthoIndex = Array.IndexOf(options, orthographic.ToString());

        GUILayout.BeginHorizontal();
        _copyOrthographic = GUILayout.Toggle(_copyOrthographic, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Orthographic", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = options[orthoIndex].ToLower();
        orthoIndex = EditorGUILayout.Popup(orthoIndex, options, _textFieldStyle, GUILayout.Height(20), GUILayout.Width(180));
        GUILayout.EndHorizontal();

        return bool.Parse(options[orthoIndex]);
    }


    private float ZoomField(float zoom)
    {
        GUILayout.BeginHorizontal();
        _copyZoom = GUILayout.Toggle(_copyZoom, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Zoom", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = zoom.ToString("0.##");
        zoom = EditorGUILayout.Slider(zoom, 0f, 1f, GUILayout.Width(180), GUILayout.Height(20));
        GUILayout.EndHorizontal();

        return zoom;
    }

    private Vector2 ChoicePosField(Vector2 position, string label)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(130));
        if (GUILayout.Button("Pos", GUILayout.Width(50), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = position.x + "," + position.y;

        position = EditorGUILayout.Vector2Field("", position, GUILayout.Width(130), GUILayout.Height(20));

        _copyPosX = GUILayout.Toggle(_copyPosX, "", GUILayout.Width(20), GUILayout.Height(20));
        _copyPosY = GUILayout.Toggle(_copyPosY, "", GUILayout.Width(20), GUILayout.Height(20));

        GUILayout.EndHorizontal();

        return position;
    }

    private bool CameraComponentsField(MonoBehaviour cameraComponent, bool enabled)
    {
        string[] options = new string[] { "True", "False" };
        var boolIndex = Array.IndexOf(options, enabled.ToString());

        if (GUILayout.Button(cameraComponent.GetType().Name, GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = cameraComponent.GetType().Name + "." + options[boolIndex].ToLower();

        boolIndex = EditorGUILayout.Popup(boolIndex, options, _textFieldStyle, GUILayout.Height(20), GUILayout.Width(180));
        return bool.Parse(options[boolIndex]);

    }

    private string SpawnParamsField()
    {
        if (CheckSpawnParamMethodIsNull(_objectId)) return string.Empty;

        MethodInfo methodInfo = _sceneObjectList[_objectId].SceneGameObject.GetComponent<Naninovel.Commands.Spawn.IParameterized>().GetType().GetMethod("SceneAssistantParameters");

        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        _copySpawnParams = GUILayout.Toggle(_copySpawnParams, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Spawn Parameters", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString =
        (string)methodInfo.Invoke(_sceneObjectList[_objectId].SceneGameObject.GetComponent<Naninovel.Commands.Spawn.IParameterized>(), new object[] { });
        GUILayout.EndHorizontal();

        return (string)methodInfo.Invoke(_sceneObjectList[_objectId].SceneGameObject.GetComponent<Naninovel.Commands.Spawn.IParameterized>(), new object[] { });
    }

    private bool CheckSpawnParamMethodIsNull(string objectId) => _sceneObjectList[objectId].SceneGameObject.GetComponent<Naninovel.Commands.Spawn.IParameterized>()?.GetType().GetMethod("SceneAssistantParameters") == null;

    private Vector3 PositionField(Vector3 position)
    {
        GUILayout.BeginHorizontal();
        _copyPosition = GUILayout.Toggle(_copyPosition, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button(_sceneObjectList[_objectId].SceneObjectType.Equals(SceneObjectType.Camera) ? "Offset" : "Position", GUILayout.Width(150), GUILayout.Height(20)))
            GUIUtility.systemCopyBuffer = _clipboardString = CopyVector(position, _copyPosX, _copyPosY, _copyPosZ);
        position = EditorGUILayout.Vector3Field("", position, GUILayout.Width(180), GUILayout.Height(20));

        _copyPosX = GUILayout.Toggle(_copyPosX, "", GUILayout.Width(20), GUILayout.Height(20));
        _copyPosY = GUILayout.Toggle(_copyPosY, "", GUILayout.Width(20), GUILayout.Height(20));
        _copyPosZ = GUILayout.Toggle(_copyPosZ, "", GUILayout.Width(20), GUILayout.Height(20));
        GUILayout.EndHorizontal();

        return position;
    }

    private Vector3 PosField(Vector3 pos)
    {
        GUILayout.BeginHorizontal();

        _copyPosition = GUILayout.Toggle(_copyPosition, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Pos", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = CopyVector(new Vector3(pos.x * 100, pos.y * 100, pos.z), _copyPosX, _copyPosY, _copyPosZ);

        pos.x = pos.x * 100;
        pos.y = pos.y * 100;

        pos = EditorGUILayout.Vector3Field("", pos, GUILayout.Width(180), GUILayout.Height(20));

        _copyPosX = GUILayout.Toggle(_copyPosX, "", GUILayout.Width(20), GUILayout.Height(20));
        _copyPosY = GUILayout.Toggle(_copyPosY, "", GUILayout.Width(20), GUILayout.Height(20));
        _copyPosZ = GUILayout.Toggle(_copyPosZ, "", GUILayout.Width(20), GUILayout.Height(20));

        pos.x = pos.x / 100;
        pos.y = pos.y / 100;

        GUILayout.EndHorizontal();

        return Engine.GetConfiguration<CameraConfiguration>().SceneToWorldSpace(pos);

    }

    private Quaternion RotationField(Quaternion rotation)
    {
        GUILayout.BeginHorizontal();

        _copyRotation = GUILayout.Toggle(_copyRotation, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Rotation", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = CopyVector(rotation.eulerAngles, _copyRotX, _copyRotY, _copyRotZ);

        rotation.eulerAngles = EditorGUILayout.Vector3Field("", rotation.eulerAngles, GUILayout.Width(180), GUILayout.Height(20));

        _copyRotX = GUILayout.Toggle(_copyRotX, "", GUILayout.Width(20), GUILayout.Height(20));
        _copyRotY = GUILayout.Toggle(_copyRotY, "", GUILayout.Width(20), GUILayout.Height(20));
        _copyRotZ = GUILayout.Toggle(_copyRotZ, "", GUILayout.Width(20), GUILayout.Height(20));
        GUILayout.EndHorizontal();

        return rotation;
    }

    private Vector3 ScaleField(Vector3 scale)
    {
        GUILayout.BeginHorizontal();

        _copyScale = GUILayout.Toggle(_copyScale, "", GUILayout.Width(20), GUILayout.Height(20));
        if (GUILayout.Button("Scale", GUILayout.Width(150), GUILayout.Height(20))) GUIUtility.systemCopyBuffer = _clipboardString = CopyVector(scale, _copyScaleX, _copyScaleY, _copyScaleZ);

        scale = EditorGUILayout.Vector3Field("", scale, GUILayout.Width(180), GUILayout.Height(20));

        _copyScaleX = GUILayout.Toggle(_copyScaleX, "", GUILayout.Width(20), GUILayout.Height(20));
        _copyScaleY = GUILayout.Toggle(_copyScaleY, "", GUILayout.Width(20), GUILayout.Height(20));
        _copyScaleZ = GUILayout.Toggle(_copyScaleZ, "", GUILayout.Width(20), GUILayout.Height(20));

        GUILayout.EndHorizontal();

        return scale;
    }

    private string CopyCommand(string objectId, bool inlined)
    {
        var commandString = string.Empty;

        if (_sceneObjectList[objectId].SceneObjectType.Equals(SceneObjectType.Character))
        {
            commandString = "char " + objectId +
            (_copyAppearance && characterManager.GetActor(objectId).Appearance != null ? "." + characterManager.GetActor(objectId).Appearance : string.Empty) +
            (_copyLookDirection ? " look:" + characterManager.GetActor(objectId).LookDirection.ToString().ToLower() : string.Empty) +
            (_copyTintColor ? " tint:#" + ColorUtility.ToHtmlStringRGBA(characterManager.GetActor(objectId).TintColor) : string.Empty) +
            (_copyPosition && _usePosOverPosition ? " pos:" + CopyVector(ParsePos(characterManager.GetActor(objectId).Position), _copyPosX, _copyPosY, _copyPosZ) : string.Empty) +
            (_copyPosition && !_usePosOverPosition ? " position:" + CopyVector(characterManager.GetActor(objectId).Position, _copyPosX, _copyPosY, _copyPosZ) : string.Empty) +
            (_copyRotation ? " rotation:" + CopyVector(characterManager.GetActor(objectId).Rotation.eulerAngles, _copyRotX, _copyRotY, _copyRotZ) : string.Empty) +
            (_copyScale ? " scale:" + CopyVector(characterManager.GetActor(objectId).Scale, _copyScaleX, _copyScaleY, _copyScaleZ) : string.Empty);
        }
        else if (_sceneObjectList[objectId].SceneObjectType.Equals(SceneObjectType.Background))
        {
            commandString = "back id:" + objectId +
            (_copyAppearance && backgroundManager.GetActor(objectId).Appearance != null ? " appearance:" + backgroundManager.GetActor(objectId).Appearance : string.Empty) +
            (_copyTintColor ? " tint:#" + ColorUtility.ToHtmlStringRGBA(backgroundManager.GetActor(objectId).TintColor) : string.Empty) +
            (_copyPosition && _usePosOverPosition ? " pos:" + CopyVector(ParsePos(backgroundManager.GetActor(objectId).Position), _copyPosX, _copyPosY, _copyPosZ) : string.Empty) +
            (_copyPosition && !_usePosOverPosition ? " position:" + CopyVector(backgroundManager.GetActor(objectId).Position, _copyPosX, _copyPosY, _copyPosZ) : string.Empty) +
            (_copyRotation ? " rotation:" + CopyVector(backgroundManager.GetActor(objectId).Rotation.eulerAngles, _copyRotX, _copyRotY, _copyRotZ) : string.Empty) +
            (_copyScale ? " scale:" + CopyVector(backgroundManager.GetActor(objectId).Scale, _copyScaleX, _copyScaleY, _copyScaleZ) : string.Empty);
        }
        else if (_sceneObjectList[objectId].SceneObjectType.Equals(SceneObjectType.Spawn))
        {
            commandString = "spawn " + objectId +
            (_copyPosition && _usePosOverPosition ? " pos:" + CopyVector(ParsePos(spawnManager.GetSpawned(objectId).Transform.position), _copyPosX, _copyPosY, _copyPosZ) : string.Empty) +
            (_copyPosition && !_usePosOverPosition ? " position:" + CopyVector(spawnManager.GetSpawned(objectId).Transform.position, _copyPosX, _copyPosY, _copyPosZ) : string.Empty) +
            (_copyRotation ? " rotation:" + CopyVector(spawnManager.GetSpawned(objectId).Transform.eulerAngles, _copyRotX, _copyRotY, _copyRotZ) : string.Empty) +
            (_copyScale ? " scale:" + CopyVector(spawnManager.GetSpawned(objectId).Transform.localScale, _copyScaleX, _copyScaleY, _copyScaleZ) : string.Empty) +
            (_copySpawnParams && !CheckSpawnParamMethodIsNull(objectId) ? " params:" + SpawnParamsField() : string.Empty);
        }
        else if (_sceneObjectList[objectId].SceneObjectType.Equals(SceneObjectType.Camera))
        {
            commandString = "camera" +
            (_copyZoom ? " zoom:" + cameraManager.Zoom : string.Empty) +
            (_copyOrthographic ? " orthographic:" + cameraManager.Orthographic.ToString().ToLower() : string.Empty) +
            (_copyPosition ? " offset:" + CopyVector(cameraManager.Offset, _copyPosX, _copyPosY, _copyPosZ) : string.Empty) +
            (_copyRotation ? " rotation:" + CopyVector(cameraManager.Rotation.eulerAngles, _copyRotX, _copyRotY, _copyRotZ) : string.Empty) +
            (_copyCameraComponents && _cameraComponentList.Count > 0 ? " set:" + string.Join(",", _cameraComponentList.Select(x => x.GetType().Name + "." + x.enabled.ToString().ToLower()).ToArray()) : string.Empty);
        }
        else if (_sceneObjectList[objectId].SceneObjectType.Equals(SceneObjectType.Printer))
        {
            commandString = "printer " + objectId +
            (_copyAppearance && printerManager.GetActor(objectId).Appearance != null ? "." + printerManager.GetActor(objectId).Appearance : string.Empty) +
            (_copyTintColor ? " tint:#" + ColorUtility.ToHtmlStringRGBA(printerManager.GetActor(objectId).TintColor) : string.Empty) +
            (_copyPosition && _usePosOverPosition ? " pos:" + CopyVector(ParsePos(printerManager.GetActor(objectId).Position), _copyPosX, _copyPosY, _copyPosZ) : string.Empty) +
            (_copyPosition && !_usePosOverPosition ? " position:" + CopyVector(printerManager.GetActor(objectId).Position, _copyPosX, _copyPosY, _copyPosZ) : string.Empty) +
            (_copyRotation ? " rotation:" + CopyVector(printerManager.GetActor(objectId).Rotation.eulerAngles, _copyRotX, _copyRotY, _copyRotZ) : string.Empty) +
            (_copyScale ? " scale:" + CopyVector(printerManager.GetActor(objectId).Scale, _copyScaleX, _copyScaleY, _copyScaleZ) : string.Empty);
        }
        else if (_sceneObjectList[objectId].SceneObjectType.Equals(SceneObjectType.ChoiceHandler))
        {
            foreach (ChoiceHandlerButton choice in _sceneObjectList[objectId].SceneGameObject.GetComponentsInChildren<ChoiceHandlerButton>())
            {
                commandString = commandString + (inlined ? "[" : "@") + "choice " + "\"" + choice.ChoiceState.Summary + "\"" + " handler:" + objectId + " pos:" + choice.transform.localPosition.x + "," + choice.transform.localPosition.y + (inlined ? "]" : "") + "\n";
            }
            return commandString;
        }

        if (string.IsNullOrEmpty(commandString)) return string.Empty;
        if (inlined) return "[" + commandString + "]";
        else return "@" + commandString;
    }

    private static string CopyVector(Vector3 vector, bool copyX, bool copyY, bool copyZ) => (copyX ? vector.x.ToString("0.##") : string.Empty) + "," + (copyY ? vector.y.ToString("0.##") : string.Empty) + "," + (copyZ ? vector.z.ToString("0.##") : string.Empty);
    private static Vector3 ParsePos(Vector3 position) => new Vector3(Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(position).x * 100, Engine.GetConfiguration<CameraConfiguration>().WorldToSceneSpace(position).y * 100, position.z);
    private string CopyAllOrSelected(bool copyAll)
    {
        var allString = string.Empty;

        _copyAppearance = _copyLookDirection = _copyTintColor = _copyZoom = _copyOrthographic = _copyCameraComponents = _copyPosition = _copyRotation = _copyScale
                = _copyPosX = _copyPosY = _copyPosZ = _copyRotX = _copyRotY = _copyRotZ = _copyScaleX = _copyScaleY = _copyScaleZ = true;

        foreach (var obj in _sceneObjectList)
        {
            if (!copyAll)
            {
                if (!_copyCharacters && _sceneObjectList[obj.Key].SceneObjectType.Equals(SceneObjectType.Character)) continue;
                if (!_copyBackgrounds && _sceneObjectList[obj.Key].SceneObjectType.Equals(SceneObjectType.Background)) continue;
                if (!_copySpawns && _sceneObjectList[obj.Key].SceneObjectType.Equals(SceneObjectType.Spawn)) continue;
                if (!_copyCamera && _sceneObjectList[obj.Key].SceneObjectType.Equals(SceneObjectType.Camera)) continue;
                if (!_copyPrinters && _sceneObjectList[obj.Key].SceneObjectType.Equals(SceneObjectType.Printer)) continue;
                if (!_copyChoices && _sceneObjectList[obj.Key].SceneObjectType.Equals(SceneObjectType.ChoiceHandler)) continue;
                allString = allString + CopyCommand(obj.Key, false) + "\n";
            }
            else allString = allString + CopyCommand(obj.Key, false) + "\n";
        }
        return allString;
    }
}



