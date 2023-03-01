using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NaninovelSceneAssistant
{
    public class SceneAssistantEditor : EditorWindow, ISceneAssistantLayout
    {
        private ISceneAssistantLayout layout { get => this; }
        public string[] Tabs { get; protected set; }
        protected INaninovelObjectData CurrentObject => sceneAssistantManager.ObjectList.Values.ToArray()[objectIndex];
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
            sceneAssistantManager.OnSceneAssistantReset += HandleReset;
        }

        private static void HandleReset()
        {
            if (sceneAssistantManager.ObjectList.Count >= objectIndex) return;
            else objectIndex = 0; 
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

            ShowCommandButtons();
            GUILayout.Space(5);
            ShowTypeOptions();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            ShowIdField();

            EditorGUILayout.Space(10);

            ShowCommandParameters(CurrentObject.Params, layout);

            EditorGUILayout.Space(5);

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

        protected virtual void ShowCommandButtons()
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
        private void ShowIdField()
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



        public void BoolField(ParameterValue param, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Toggle((bool)param.Value), param, toggleWith);

        public void IntField(ParameterValue param, int? minValue = null, int? maxValue = null, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.IntField((int)param.Value), param, toggleWith);

        public void IntSliderField(ParameterValue param, int min, int max, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.IntSlider((int)param.Value, min, max), param, toggleWith);

        public void StringField(ParameterValue param, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.DelayedTextField((string)param.Value), param, toggleWith);

        public void ColorField(ParameterValue param, bool includeAlpha = true, ParameterValue toggleWith = null)
        => WrapInLayout(() => param.Value = EditorGUILayout.ColorField(GUIContent.none, (Color)param.Value, true, includeAlpha, false), param,toggleWith);

        //FloatField needs to be setup differently to support sliding when displaying label only.
        public void FloatField(ParameterValue param, float? min = null, float? max = null, ParameterValue toggleWith = null)
        {
            if (param.Condition != null && !param.Condition()) return;

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

        public void FloatSliderField(ParameterValue param, float min, float max, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Slider((float)param.Value, min, max), param, toggleWith);
        public void Vector2Field(ParameterValue param, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector2Field("", (Vector2)param.Value), param, toggleWith);

        public void Vector3Field(ParameterValue param, ParameterValue toggleWith = null) 
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector3Field("", (Vector3)param.Value), param, toggleWith);
    
        public void Vector4Field(ParameterValue param, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector4Field("", (Vector4)param.Value), param, toggleWith);
        public void EnumField(ParameterValue param, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.EnumPopup((Enum)param.Value), param, toggleWith);

        public void EmptyField(ParameterValue param, ParameterValue toggleWith = null)
            => WrapInLayout(null, param, toggleWith);

        public void StringListField(ParameterValue param, string[] stringValues, ParameterValue toggleWith = null)
        {
            if (param.Condition != null && param.Condition() == false) return;
            var stringIndex = stringValues.IndexOf(param.Value ?? "None");
            ShowParameterOptions(param);

            EditorGUI.BeginDisabledGroup(!param.Selected);
            stringIndex = EditorGUILayout.Popup(stringIndex, stringValues);
            if (toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;
            EditorGUI.EndDisabledGroup();

            param.Value = stringValues[stringIndex];
        }


        public void TypeListField<T>(ParameterValue param, Dictionary<string, T> values, ParameterValue toggleWith = null)
        {
            if (param.Condition != null && param.Condition() == false) return;

            var stringIndex = Array.IndexOf(values.Values.ToArray(), param.Value ?? values["None"]);
            ShowParameterOptions(param);

            EditorGUI.BeginDisabledGroup(!param.Selected);
            stringIndex = EditorGUILayout.Popup(stringIndex, values.Keys.ToArray());
            if (toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;
            EditorGUI.EndDisabledGroup();

            param.Value = (T)values.FirstOrDefault(s => s.Key == values.Keys.ToArray()[stringIndex]).Value;
        }


        public void PosField(ParameterValue param, ParameterValue toggleWith = null)
        {
            if (param.Condition != null && param.Condition() == false) return;
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

        public void VariableField(VariableValue var)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(var.Name);
            var.Value = EditorGUILayout.DelayedTextField(var.Value);
            EditorGUILayout.EndHorizontal();
        }

        public void UnlockableField(UnlockableValue unlockable)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(unlockable.Name);
            unlockable.EnumValue = (UnlockableValue.UnlockableState)EditorGUILayout.EnumPopup(unlockable.EnumValue);

            if (unlockable.EnumValue == UnlockableValue.UnlockableState.Unlocked) unlockable.Value = true;
            else unlockable.Value = false;

            EditorGUILayout.EndHorizontal();
        }

        public void WrapInLayout(Action layoutField, ParameterValue param, ParameterValue toggleWith = null)
        {
            if (param.Condition != null && param.Condition() == false) return;
            ShowParameterOptions(param);
            EditorGUI.BeginDisabledGroup(!param.Selected);
            if(layoutField != null) layoutField();
            if(toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;
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
