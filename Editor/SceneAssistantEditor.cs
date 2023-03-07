using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NaninovelSceneAssistant
{
    public partial class SceneAssistantEditor : EditorWindow
    {
        private static SceneAssistantEditor sceneAssistantEditor;
        public string[] Tabs { get; protected set; } = new string[] { "Objects", "Variables", "Unlockables", "Scripts"};
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

        [MenuItem("Naninovel/Scene Assistant", false, 360)]
        public static void ShowWindow()
        {
            sceneAssistantEditor = GetWindow<SceneAssistantEditor>("Naninovel Scene Assistant");
        }

        private void Awake()
        {
            EditorGUIUtility.labelWidth = 150;
            if (Engine.Initialized) SetupAndInitializeSceneAssistant();
        }

        [InitializeOnEnterPlayMode]
        private static void DetectEngineInitialization()
        {
            if (HasOpenInstances<SceneAssistantEditor>()) Engine.OnInitializationFinished += SetupAndInitializeSceneAssistant;
        }

        private void OnDestroy()
        {
            if (sceneAssistantManager != null && sceneAssistantManager.Initialised) sceneAssistantManager.DestroySceneAssistant();
        }

        private static void SetupAndInitializeSceneAssistant()
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
            }

        }

        protected virtual void DrawSceneAssistant(ISceneAssistantLayout layout)
        {
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
        }

        protected virtual void DrawCommandOptions()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (DrawButton("Copy command (@)")) ClipboardString = CurrentObject.GetCommandLine();
            if (DrawButton("Copy command ([])")) ClipboardString = CurrentObject.GetCommandLine(inlined: true);
            if (DrawButton("Copy all")) ClipboardString = CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList);
            if (DrawButton("Copy selected")) ClipboardString = CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, selected: true);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        protected void DrawIdField()
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

        protected virtual void DrawTypeOptions()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var list = sceneAssistantManager.ObjectTypeList.Keys.ToList();

            foreach (var type in list)
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

            for (int i = 0; i < parameters.Count; i++)
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
            if (stateManager.Configuration.EnableStateRollback) 
            { 
                if (GUILayout.Button("Rollback", EditorStyles.miniButton)) CurrentObject.Params.Where(p => p.Value != null).ToList().ForEach(p => p.Value = p.GetStateValue());
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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

    }
}
