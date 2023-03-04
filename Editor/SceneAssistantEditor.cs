using Naninovel;
using Naninovel.Commands;
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
        private static SceneAssistantEditor sceneAssistantEditor;
        private ISceneAssistantLayout sceneAssistantLayout { get => this; }
        public string[] Tabs { get; protected set; } = new string[] { "Objects", "Variables", "Unlockables", "Scripts", "Debug" };
        protected INaninovelObjectData CurrentObject => sceneAssistantManager.ObjectList.Values.ToArray()[objectIndex];
        protected static string ClipboardString { get => clipboardString; set { clipboardString = value; EditorGUIUtility.systemCopyBuffer = value; if (logResults) Debug.Log(value); } }

        private static SceneAssistantManager sceneAssistantManager;
        private static IScriptPlayer scriptPlayer;
        private static IStateManager stateManager;
        private static int objectIndex = 0;
        private static int tabIndex = 0;
        private static string clipboardString = string.Empty;
        private static string search;
        private static Vector2 scrollPos;
        private static float textAreaHeight;
        private static bool logResults;
        private static PlaybackSpot currentPrintSpot;

        [MenuItem("Naninovel/New Scene Assistant", false, 360)]
        public static void ShowWindow() {
            sceneAssistantEditor = GetWindow<SceneAssistantEditor>("Naninovel Scene Assistant");
        }

        private void Awake()
        {
            EditorGUIUtility.labelWidth = 150;
            if (Engine.Initialized) SetupSceneAssistant();
        }

        [InitializeOnEnterPlayMode]
        private static void DetectEngineInitialization()
        {
            if (HasOpenInstances<SceneAssistantEditor>()) Engine.OnInitializationFinished += SetupSceneAssistant;
        }

        private static void SetupSceneAssistant()
        {
            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
            scriptPlayer = Engine.GetService<IScriptPlayer>();
            stateManager = Engine.GetService<StateManager>();
            sceneAssistantManager.InitializeSceneAssistant();
            sceneAssistantManager.OnSceneAssistantReset += HandleReset;
            scriptPlayer.OnCommandExecutionFinish += HandleCommandExecuted;
            sceneAssistantEditor = GetWindow<SceneAssistantEditor>("Naninovel Scene Assistant");
        }

        private static void HandleReset()
        {
            if (sceneAssistantManager.ObjectList.Count >= objectIndex) return;
            else objectIndex = 0; 
        }

        private static void HandleCommandExecuted(Command command)
        {
            if (command is PrintText printText) currentPrintSpot = printText.PlaybackSpot;
            if (sceneAssistantEditor != null) sceneAssistantEditor.Repaint();
        }

        public void OnGUI()
        {
            if (Engine.Initialized && sceneAssistantManager?.ObjectList.Count > 0)
            {
                ShowTabs(sceneAssistantManager, sceneAssistantLayout);
            }
            else EditorGUILayout.LabelField("Naninovel is not initialized.");
        }

        protected virtual void ShowTabs(SceneAssistantManager sceneAssistant, ISceneAssistantLayout layout)
        {
            GUILayout.Space(10f);
            EditorGUI.BeginChangeCheck();
            tabIndex = GUILayout.Toolbar(tabIndex, Tabs, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck()) search = string.Empty;
            GUILayout.Space(5f);

            switch (tabIndex)
            {
                case 0:
                    DrawSceneAssistant(layout);
                    break;
                case 1:
                    DrawCustomVariables(sceneAssistant.CustomVarList, layout);
                    break;
                case 2:
                    DrawUnlockables(sceneAssistant.UnlockablesList, layout);
                    break;
                case 3:
                    DrawScripts(sceneAssistant.ScriptsList);
                    break;
                case 4:
                    DrawDebug();
                    break;
            }

        }

        protected virtual void DrawSceneAssistant(ISceneAssistantLayout layout)
        {
            EditorGUI.BeginChangeCheck();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos); 
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            DrawCommandOptions();
            GUILayout.Space(5);
            DrawTypeOptions();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawIdField();

            EditorGUILayout.Space(10);

            DrawCommandParameters(CurrentObject.Params, layout);

            EditorGUILayout.Space(5);

            DrawCommandParameterOptions();

            DrawCommandTextArea();

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            if(EditorGUI.EndChangeCheck()) GUI.FocusControl(null); 
        }

        protected virtual void DrawCommandOptions()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (DrawButton("Copy command (@)")) ClipboardString = CurrentObject.GetCommandLine();
            if (DrawButton("Copy command ([])")) ClipboardString = CurrentObject.GetCommandLine(inlined:true);
            if (DrawButton("Copy all")) ClipboardString = CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList);
            if (DrawButton("Copy selected")) ClipboardString = CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, selected:true);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        private void DrawIdField()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (DrawButton("Id")) ClipboardString = CurrentObject.Id;
            objectIndex = EditorGUILayout.Popup(objectIndex, sceneAssistantManager.ObjectList.Keys.ToArray(), new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(150));
            if (DrawButton("Inspect object")) Selection.activeGameObject = CurrentObject.GameObject;
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawTypeOptions()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var list = sceneAssistantManager.ObjectTypeList.Keys.ToList();

            foreach(var type in list)
            {
                var typeText = type.GetProperty("TypeId").GetValue(null).ToString() ?? type.Name;
                float typeTextWidth = EditorStyles.label.CalcSize(new GUIContent(typeText)).x;
                sceneAssistantManager.ObjectTypeList[type] = EditorGUILayout.Toggle("", sceneAssistantManager.ObjectTypeList[type], GUILayout.Width(15));
                EditorGUILayout.LabelField(typeText, EditorStyles.miniLabel, GUILayout.Width(typeTextWidth));
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        protected virtual void DrawCommandParameters(List<ParameterValue> parameters, ISceneAssistantLayout layout)
        {
            if (parameters == null || parameters.Count == 0 || layout == null) return;

            for( int i=0; i < parameters.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                parameters[i].DisplayField(layout);
                EditorGUILayout.EndHorizontal();
            }
        }
        protected virtual void DrawCommandParameterOptions()
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

        protected virtual void DrawCommandTextArea()
        {
            GUIStyle style = new GUIStyle(GUI.skin.textArea);
            style.fontSize = 12;
            style.wordWrap = true;
            textAreaHeight = style.CalcHeight(new GUIContent(clipboardString), position.width);

            EditorGUILayout.Space(5);
            clipboardString = EditorGUILayout.TextArea(clipboardString, style, GUILayout.Height(textAreaHeight + 10));
            logResults = EditorGUILayout.ToggleLeft("Log Results", logResults, EditorStyles.miniLabel);
        }

        protected virtual void DrawCustomVariables(SortedList<string, VariableValue> variables, ISceneAssistantLayout layout)
        {
            if (variables == null && variables.Count == 0) return;
            GUILayout.Space(5);

            DrawSearchField();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (VariableValue variable in variables.Values)
            {
                if (!string.IsNullOrEmpty(search) && variable.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;

                EditorGUILayout.BeginHorizontal();
                variable.DisplayField(layout);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        protected virtual void DrawUnlockables(SortedList<string, UnlockableValue> unlockables, ISceneAssistantLayout layout)
        {
            if (unlockables == null) return;
            GUILayout.Space(5);

            DrawSearchField();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (UnlockableValue unlockable in unlockables.Values)
            {
                if (!string.IsNullOrEmpty(search) && unlockable.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;

                EditorGUILayout.BeginHorizontal();
                unlockable.DisplayField(layout);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        protected virtual void DrawScripts(IReadOnlyCollection<string> scripts)
        {
            if (scripts == null) return;
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search: ", GUILayout.Width(50));
            search = GUILayout.TextField(search);
            GUILayout.EndHorizontal();


            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayout.Space(5);


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();


            foreach (string script in scripts)
            {
                if (!string.IsNullOrEmpty(search) && script.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                {
                    if (GUILayout.Button(script, GUILayout.Width(300), GUILayout.Height(20))) PlayScriptAsync(script);
                    GUILayout.Space(10);
                }
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            async void PlayScriptAsync(string script)
            {
                Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
                await stateManager.ResetStateAsync(() => scriptPlayer.PreloadAndPlayAsync(script));
            }
        }

        //private void OnDestroy()
        //{
        //    if(sceneAssistantManager?.ObjectList?.Count > 0) sceneAssistantManager.DestroySceneAssistant();
        //}

        protected virtual void DrawDebug()
        {
            //todo optimize this list
            EditorGUILayout.LabelField($"Current script: {scriptPlayer.PlayedScript.Name}");
            EditorGUILayout.LabelField($"Current line number & inline index: {currentPrintSpot.LineNumber}.{currentPrintSpot.InlineIndex}");
            EditorGUILayout.LabelField($"Currently loaded actors: {string.Join(",", Engine.FindAllServices<IActorManager>().Select(e => e.GetAllActors().ToList().Select(a => a.Id).ToList()))}");
        }

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
                DrawValueInfo(param);
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

        public void Vector3Field(ParameterValue param, bool includeZPos = true, ParameterValue toggleWith = null) 
            => WrapInLayout(() => { if (includeZPos) param.Value = EditorGUILayout.Vector3Field("", (Vector3)param.Value); else param.Value = (Vector3)EditorGUILayout.Vector2Field("", (Vector3)param.Value); }, param, toggleWith);
    
        public void Vector4Field(ParameterValue param, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector4Field("", (Vector4)param.Value), param, toggleWith);
        public void EnumField(ParameterValue param, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.EnumPopup((Enum)param.Value), param, toggleWith);

        public void EmptyField(ParameterValue param, ParameterValue toggleWith = null)
            => WrapInLayout(null, param, toggleWith);

        public void StringListField(ParameterValue param, string[] stringValues, ParameterValue toggleWith = null)
        {
            if (param.Condition != null && param.Condition() == false) return;
            var stringIndex = stringValues.IndexOf(param.Value);
            DrawValueInfo(param);

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
            DrawValueInfo(param);

            EditorGUI.BeginDisabledGroup(!param.Selected);
            stringIndex = EditorGUILayout.Popup(stringIndex, values.Keys.ToArray());
            if (toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;
            EditorGUI.EndDisabledGroup();

            param.Value = (T)values.FirstOrDefault(s => s.Key == values.Keys.ToArray()[stringIndex]).Value;
        }


        public void PosField(ParameterValue param, CameraConfiguration cameraConfiguration, bool includeZPos = true, ParameterValue toggleWith = null)
        {
            if (param.Condition != null && param.Condition() == false) return;
            DrawValueInfo(param);
            var position = cameraConfiguration.WorldToSceneSpace((Vector3)param.Value);
            position.x *= 100;
            position.y *= 100;

            EditorGUI.BeginDisabledGroup(!param.Selected);

            if(includeZPos) position = EditorGUILayout.Vector3Field("", position);
            else position = EditorGUILayout.Vector2Field("", position);
            if (toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;

            EditorGUI.EndDisabledGroup();
            
            position.x /= 100;
            position.y /= 100;
            position = cameraConfiguration.SceneToWorldSpace(position);
            param.Value = position;
        }

        public void VariableField(VariableValue var)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(var.Name, GUILayout.Width(150));
            var.Value = EditorGUILayout.DelayedTextField(var.Value);
            EditorGUILayout.EndHorizontal();
        }

        public void UnlockableField(UnlockableValue unlockable)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(unlockable.Name, GUILayout.Width(150));
            unlockable.EnumValue = (UnlockableValue.UnlockableState)EditorGUILayout.EnumPopup(unlockable.EnumValue);

            if (unlockable.EnumValue == UnlockableValue.UnlockableState.Unlocked) unlockable.Value = true;
            else unlockable.Value = false;

            EditorGUILayout.EndHorizontal();
        }

        public void WrapInLayout(Action layoutField, ParameterValue param, ParameterValue toggleWith = null)
        {
            if (param.Condition != null && param.Condition() == false) return;
            DrawValueInfo(param);
            EditorGUI.BeginDisabledGroup(!param.Selected);
            if(layoutField != null) layoutField();
            if(toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;
            EditorGUI.EndDisabledGroup();
        }

        public static bool DrawButton(string label)
        {
            if (GUILayout.Button(label, GUILayout.Width(150))) return true;
            else return false;
        }

        private static void DrawSearchField()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search: ", GUILayout.Width(50));
            search = GUILayout.TextField(search);
            GUILayout.EndHorizontal();
        }
        public void DrawValueInfo(ParameterValue param)
        {
            if (param.IsParameter)
            {
                param.Selected = EditorGUILayout.Toggle(param.Selected, GUILayout.Width(20f));
                if (DrawButton(param.Name)) ClipboardString = param.GetCommandValue();
            }
            else
            {
                GUILayout.Space(25f);
                GUILayout.Label(param.Name, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter}, GUILayout.Width(150));
            }
        }

        public void Vector3Field(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new NotImplementedException();
        }
    }

}
