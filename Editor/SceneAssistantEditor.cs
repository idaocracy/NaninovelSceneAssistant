using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace NaninovelSceneAssistant
{
    public class SceneAssistantEditor : EditorWindow, ISceneAssistantLayout
    {
        private ISceneAssistantLayout layout { get => this; }
        public string[] Tabs { get; protected set; }
        protected INaninovelObject CurrentObject => sceneAssistantManager.ObjectList.Values.ToArray()[objectIndex];
        protected string[] ObjectDropdown => sceneAssistantManager.ObjectList.Keys.ToArray();
        protected Dictionary<Type, bool> TypeList => sceneAssistantManager.ObjectTypeList;
        protected string ClipboardString { get => clipboardString; set { clipboardString = value; EditorGUIUtility.systemCopyBuffer = value; if (logResults) Debug.Log(value); } }

        private static SceneAssistantManager sceneAssistantManager;
        private static IScriptPlayer scriptPlayer;
        private static IStateManager stateManager;
        private static int objectIndex = 0;
        private static int tabIndex = 0;
        private static string clipboardString = string.Empty;
        private static Vector2 scrollPos = default;
        private static bool logResults;

        [MenuItem("Naninovel/New Scene Assistant", false, 360)]
        public static void ShowWindow() => GetWindow<SceneAssistantEditor>("Naninovel Scene Assistant");

        private void Awake()
        {
            EditorGUIUtility.labelWidth = 150;
            Tabs = new string[] { "Objects", "Custom Variables", "Unlockables", "Scripts" };
            if (Engine.Initialized) sceneAssistantManager.InitializeSceneAssistant();
        }

        [InitializeOnEnterPlayMode]
        private static void DetectEngineInitialization()
        { 
            Engine.OnInitializationFinished += SetupSceneAssistant;
            // todo find a way to repaint the editor on command execution
        }

        //private void Update()
        //{
        //    Repaint();
        //}

        private static void SetupSceneAssistant()
        {
            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
            scriptPlayer = Engine.GetService<IScriptPlayer>();
            stateManager = Engine.GetService<StateManager>();
            sceneAssistantManager.InitializeSceneAssistant();
        }

        public void OnGUI()
        {
            if (Engine.Initialized && sceneAssistantManager?.ObjectList.Count > 0)
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
                    ShowCustomVariables(CurrentObject.CustomVars, layout, CurrentObject.Id);
                    break;
                case 1:
                    ShowCustomVariables(sceneAssistant.CustomVarList, layout);
                    break;
                case 2:
                    ShowUnlockables(sceneAssistant.UnlockablesList, layout);
                    break;
                case 3:
                    ShowScriptsList(sceneAssistant.ScriptsList, layout);
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
            if (GUILayout.Button("Default", EditorStyles.miniButton)) CurrentObject.Params.Where(p => p.Value != null).ToList().ForEach(p => p.Value = p.GetDefaultValue());
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
            if (ShowButton("Copy command (@)")) ClipboardString = CurrentObject.GetCommandLine();
            if (ShowButton("Copy command ([])")) ClipboardString = CurrentObject.GetCommandLine(true);
            if (ShowButton("Copy all")) ClipboardString = CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList);
            if (ShowButton("Copy selected")) ClipboardString = CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, true);
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
            var list = TypeList.Keys.ToList();

            foreach(var type in list)
            {
                TypeList[type] = EditorGUILayout.Toggle("", TypeList[type], GUILayout.Width(15));
                EditorGUILayout.LabelField(type.Name, EditorStyles.miniLabel, GUILayout.Width(25 + type.Name.Length * 4));
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        protected virtual void ShowCommandParameters(List<ParameterValue> parameters, ISceneAssistantLayout layout)
        {
            if (parameters == null || parameters.Count == 0 || layout == null) return;

            for( int i=0; i < parameters.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                parameters[i].DisplayField(layout);
                EditorGUILayout.EndHorizontal();
            }
        }

        protected virtual void ShowCustomVariables(SortedList<string, VariableValue> vars, ISceneAssistantLayout layout, string id = null)
        {
            if (vars == null) return;

            EditorGUILayout.BeginScrollView(scrollPos);
            if (id != null) EditorGUILayout.LabelField($"{id} Variables", EditorStyles.boldLabel);

            foreach (VariableValue variable in vars.Values)
            {
                EditorGUILayout.BeginHorizontal();
                variable.DisplayField(layout);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        protected virtual void ShowUnlockables(SortedList<string, UnlockableValue> unlockables, ISceneAssistantLayout layout)
        {
            if (unlockables == null) return;

            EditorGUILayout.BeginScrollView(scrollPos);
            foreach (UnlockableValue unlockable in unlockables.Values)
            {
                EditorGUILayout.BeginHorizontal();
                unlockable.DisplayField(layout);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        protected virtual void ShowScriptsList(IReadOnlyCollection<string> scripts, ISceneAssistantLayout layout)
        {
            if (scripts == null) return;

            EditorGUILayout.BeginScrollView(scrollPos);
            foreach (string script in scripts)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(script)) PlayScriptAsync(script);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private async void PlayScriptAsync(string script)
        {
            Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
            await stateManager.ResetStateAsync(() => scriptPlayer.PreloadAndPlayAsync(script));
        }

        //private void OnDestroy()
        //{
        //    if(sceneAssistantManager?.ObjectList?.Count > 0) sceneAssistantManager.DestroySceneAssistant();
        //}

        public void SliderField(ParameterValue param, float min, float max, Func<bool> condition = null, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Slider((float)param.Value, min, max), param, condition, toggleWith);

        public void BoolField(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Toggle((bool)param.Value), param, condition, toggleWith);

        public void StringField(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.DelayedTextField((string)param.Value), param, condition, toggleWith);

        public void ColorField(ParameterValue param, bool includeAlpha = true, Func<bool> condition = null, ParameterValue toggleWith = null)
        => WrapInLayout(() => param.Value = EditorGUILayout.ColorField(GUIContent.none, (Color)param.Value, true, includeAlpha, false), param, condition, toggleWith);

        //FloatField needs to be setup differently to support sliding when displaying label only.
        public void FloatField(ParameterValue param, float? min = null, float? max = null, Func<bool> condition = null, ParameterValue toggleWith = null)
        {
            if (condition != null && !condition()) return;

            if (param.IsParameter)
            {
                ShowParameterOptions(param);
                param.Value = EditorGUILayout.FloatField(Mathf.Clamp((float)param.Value, min ?? float.MinValue, max ?? float.MaxValue));
                if (toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;
            }
            else
            {
                GUILayout.Space(25f);
                EditorGUI.BeginDisabledGroup(!param.Selected);

                TextAnchor orginalAlignment = EditorStyles.label.alignment;
                EditorStyles.label.alignment = TextAnchor.MiddleCenter;

                param.Value = EditorGUILayout.FloatField(param.Name, Mathf.Clamp((float)param.Value, min ?? float.MinValue, max ?? float.MaxValue));

                EditorStyles.label.alignment = orginalAlignment;
                EditorGUI.EndDisabledGroup();
            }
        }

        public void IntField(ParameterValue param, int? minValue = null, int? maxValue = null, Func<bool> condition = null, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.IntField((int)param.Value), param, condition, toggleWith);

        public void Vector2Field(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector2Field("", (Vector2)param.Value), param, condition, toggleWith);

        public void Vector3Field(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null) 
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector3Field("", (Vector3)param.Value), param, condition, toggleWith);
    
        public void Vector4Field(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector4Field("", (Vector2)param.Value), param, condition, toggleWith);
        public void EnumField(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.EnumPopup((Enum)param.Value), param, condition, toggleWith);

        public void StringListField(ParameterValue param, string[] stringValues, Func<bool> condition = null, ParameterValue toggleWith = null)
        {
            if (condition != null && false) return;
            var stringIndex = stringValues.IndexOf(param.Value ?? "None");
            ShowParameterOptions(param);

            EditorGUI.BeginDisabledGroup(!param.Selected);
            stringIndex = EditorGUILayout.Popup(stringIndex, stringValues);
            if (toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;
            EditorGUI.EndDisabledGroup();

            if (stringValues[stringIndex] != "None") param.Value = stringValues[stringIndex];
        }

        public void PosField(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null)
        {
            if (condition != null && false) return;
            var cameraConfiguration = Engine.GetConfiguration<CameraConfiguration>();
            ShowParameterOptions(param);
            var position = cameraConfiguration.WorldToSceneSpace((Vector3)param.Value);
            position.x *= 100;
            position.y *= 100;

            EditorGUI.BeginDisabledGroup(!param.Selected);
            position = EditorGUILayout.Vector3Field("", position);
            if(toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;

            EditorGUI.EndDisabledGroup();
            
            position.x /= 100;
            position.y /= 100;
            position = cameraConfiguration.SceneToWorldSpace(position);
            param.Value = position;
        }

        public void CustomVarField(VariableValue var)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(var.Name);
            var.Value = EditorGUILayout.DelayedTextField(var.Value);
            EditorGUILayout.EndHorizontal();
        }

        public void UnlockableField(UnlockableValue unlockable, int stateIndex, string[] states)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(unlockable.Name);
            stateIndex = EditorGUILayout.Popup(stateIndex, states);
            unlockable.Value = stateIndex == 1 ? true : false;
            EditorGUILayout.EndHorizontal();
        }

        public void WrapInLayout(Action layoutField, ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null)
        {
            if (condition != null && false) return;
            ShowParameterOptions(param);
            EditorGUI.BeginDisabledGroup(!param.Selected);
            layoutField();
            if (toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;
            EditorGUI.EndDisabledGroup();
        }

        public static bool ShowButton(string label)
        {
            if (GUILayout.Button(label, GUILayout.Width(150))) return true;
            else return false;
        }

        public void ShowParameterOptions(ParameterValue param)
        {
            //todo sort out pos and position toggles again
            if (param.IsParameter)
            {
                param.Selected = EditorGUILayout.Toggle(param.Selected, GUILayout.Width(20f));
                if (ShowButton(param.Name)) ClipboardString = param.GetCommandValue();
            }
            else
            {
                GUILayout.Space(25f);
                GUILayout.Label(param.Name, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter}, GUILayout.Width(150));
            }
        }
    }

}
