using Naninovel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace NaninovelSceneAssistant
{
    public class SceneAssistantEditorbackup
        : EditorWindow, ISceneAssistantLayout
    {
        private ISceneAssistantLayout layout { get => this; }  
        public string[] Tabs { get; protected set; }
        protected INaninovelObject CurrentObject => sceneAssistantManager.ObjectList[objectIndex];
        protected string[] ObjectDropdown => sceneAssistantManager.ObjectList.Select(p => p.Id).ToArray();
        protected string[] TypeList => sceneAssistantManager.ObjectList.Select(p => p.TypeId).ToArray();
        protected string ClipboardString { get => clipboardString; set { clipboardString = value; EditorGUIUtility.systemCopyBuffer = value; if (logResults) Debug.Log(value); } }

        private static SceneAssistantManager sceneAssistantManager;
        private static int objectIndex = 0;
        private static int tabIndex = 0;
        private static string clipboardString = string.Empty;
        private static Vector2 scrollPos = default;
        private static bool logResults;

        [MenuItem("Naninovel/New Scene Assistant", false, 350)]
        public static void ShowWindow() => GetWindow<SceneAssistantEditor>("Naninovel Scene Assistant");

        private void Awake()
        {
            EditorGUIUtility.labelWidth = 150;
            Tabs = new string[] { "Objects", "Custom Variables", "Unlockables" };
            if (Engine.Initialized) SetupSceneAssistant();
        }

        [InitializeOnEnterPlayMode]
        private static void DetectEngineInitialization()
        {
            Engine.OnInitializationFinished += SetupSceneAssistant;
        }

        private static void SetupSceneAssistant()
        {
            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
            sceneAssistantManager.InitializeSceneAssistant();
        }


        public void OnGUI()
        {
            if (Engine.Initialized) 
            {
                ShowTabs(sceneAssistantManager, layout);
            }
            else EditorGUILayout.LabelField("Naninovel is not initialized.");
        }

        protected virtual void ShowTabs(SceneAssistantManager sceneAssistant, ISceneAssistantLayout layout)
        {
            GUILayout.Space(10f);
            tabIndex = GUILayout.Toolbar(tabIndex, Tabs, EditorStyles.toolbarButton);
            GUILayout.Space(10f);

            switch (tabIndex)
            {
                case 0:
                    ShowSceneAssistant(layout);
                    ShowCustomVariables(CurrentObject.CustomVars, CurrentObject.Id);
                    break;
                case 1:
                    ShowCustomVariables(sceneAssistant.CustomVarList);
                    break;
                case 2:
                    ShowUnlockables(sceneAssistant.UnlockablesList);
                    break;
            }
        }

        protected virtual void ShowSceneAssistant(ISceneAssistantLayout layout)
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

            ShowCommandParameters(CurrentObject.Params, layout);

            ShowOptionButtons();

            EditorGUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(true);
            clipboardString = EditorGUILayout.TextArea(clipboardString, new GUIStyle(GUI.skin.textField) { fontSize = 10, wordWrap = true }, GUILayout.Height(50));
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
        protected virtual void DrawIdField()
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

        protected virtual void ShowTypeOptions()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int i = 0; i < TypeList.Length; i++)
            {
                if (i == 3)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                }

                //typeBools[i] = EditorGUILayout.Toggle("", typeBools[i], GUILayout.Width(15));
                //EditorGUILayout.LabelField(typeList[i], EditorStyles.miniLabel, GUILayout.Width(25 + typeList[i].Replace(" ", "").Length * 4));
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        protected virtual void ShowCommandParameters(List<CommandParam> parameters, ISceneAssistantLayout layout)
        {
            if (parameters == null || parameters.Count == 0) return;
            if (layout == null) return;

            if (CurrentObject.HasPosValues(out var posParamIndex, out var positionParamIndex))
            {
                parameters[posParamIndex].Selected = !parameters[positionParamIndex].Selected;
                parameters[positionParamIndex].Selected = !parameters[posParamIndex].Selected;
            }

            foreach (CommandParam param in parameters)
            {
                EditorGUILayout.BeginHorizontal();
                ShowValueOptions(param);
                param.DisplayField(layout);
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
                variable.DisplayField(layout);
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
                unlockable.DisplayField(layout);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }


        //private void OnDestroy()
        //{
        //    if(sceneAssistantManager?.ObjectList?.Count > 0) sceneAssistantManager.DestroySceneAssistant();
        //}

        protected virtual bool ShowButton(string label)
        {
            if (GUILayout.Button(label, GUILayout.Width(150))) return true;
            else return false;
        }

        protected virtual void ShowValueOptions(CommandParam param)
        {
            if (param.HasCommandOptions)
            {
                param.Selected = EditorGUILayout.Toggle(param.Selected, GUILayout.Width(20f));
                if (ShowButton(param.Name)) ClipboardString = param.GetCommandValue();
            }
            else
            {
                GUILayout.Space(25f);
                GUILayout.Label(param.Name.ToString(), GUILayout.Width(150));
            }
        }

        public void Vector3Field(CommandParam param) => param.Value = EditorGUILayout.Vector3Field("", (Vector3)param.Value);
        public void SliderField(CommandParam param, float min, float max) => param.Value = (EditorGUILayout.Slider((float)param.Value, min, max));
        public void BoolField(CommandParam param) => param.Value = EditorGUILayout.Toggle((bool)param.Value);
        public void StringField(CommandParam param) => param.Value = EditorGUILayout.DelayedTextField((string)param.Value);
        public void ColorField(CommandParam param) => param.Value = EditorGUILayout.ColorField((Color)param.Value);
        public void FloatField(CommandParam param) => param.Value = EditorGUILayout.FloatField((float)param.Value);
        public void Vector2Field(CommandParam param) => param.Value = EditorGUILayout.Vector2Field("", (Vector2)param.Value);
        public void IntField(CommandParam param) => param.Value = EditorGUILayout.IntField((int)param.Value);
        public void EnumField(CommandParam param) => param.Value = EditorGUILayout.EnumPopup((Enum)param.Value);
        public void Vector4Field(CommandParam param) => param.Value = EditorGUILayout.Vector4Field("", (Vector4)param.Value);
        public void StringListField(CommandParam param, string[] stringValues)
        {
            var stringIndex = stringValues.IndexOf(param.Value);
            stringIndex = EditorGUILayout.Popup(stringIndex, stringValues);
            param.Value = stringValues[stringIndex];
        }

        public void PosField(CommandParam param)
        {
            var cameraConfiguration = Engine.GetConfiguration<CameraConfiguration>();
            var position = cameraConfiguration.WorldToSceneSpace((Vector3)param.Value);
            position.x *= 100;
            position.y *= 100;
            position = EditorGUILayout.Vector3Field("", position);
            position.x /= 100;
            position.y /= 100;
            position = cameraConfiguration.SceneToWorldSpace(position);
            param.Value = position;
        }

        public void CustomVarField(CustomVar var)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(var.Name);
            var.Value = EditorGUILayout.DelayedTextField(var.Value);
            EditorGUILayout.EndHorizontal();
        }

        public void UnlockableField(Unlockable unlockable, int stateIndex, string[] states)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(unlockable.Name);
            stateIndex = EditorGUILayout.Popup(stateIndex, states);
            unlockable.Value = stateIndex == 1 ? true : false;
            EditorGUILayout.EndHorizontal();
        }
    }
}