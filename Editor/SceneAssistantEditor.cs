using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NaninovelSceneAssistant
{
    public partial class SceneAssistantEditor : EditorWindow
    {
        private static SceneAssistantEditor sceneAssistantEditor;
        public string[] Tabs { get; protected set; } = new string[] { "Objects", "Variables", "Unlockables", "Scripts"};
        protected static INaninovelObjectData CurrentObject => sceneAssistantManager.ObjectList.Values.ToArray()[objectIndex];
        protected static string ClipboardString { get => clipboardString; set { clipboardString = value; EditorGUIUtility.systemCopyBuffer = value; if (logResults) Debug.Log(value); } }
        protected ScriptImporterEditor[] VisualEditors => Resources.FindObjectsOfTypeAll<ScriptImporterEditor>();

        private static SceneAssistantManager sceneAssistantManager;
        private static IScriptPlayer scriptPlayer;
        private static IStateManager stateManager;
        private static ScriptsConfiguration scriptsConfiguration;
        private static int objectIndex;
        private static int tabIndex;
        private static string clipboardString;
        private static string search;
        private static Vector2 scrollPos;
        private static bool logResults;
        private static bool isFocused;
        private static bool commandPlaying;

        [MenuItem("Naninovel/Scene Assistant", false, 360)]
        public static void ShowWindow()
        {
            sceneAssistantEditor = GetWindow<SceneAssistantEditor>("Naninovel Scene Assistant");
        }

        [InitializeOnEnterPlayMode]
        private static void DetectEngineInitialization()
        {
            if (HasOpenInstances<SceneAssistantEditor>()) Engine.OnInitializationFinished += SetupAndInitializeSceneAssistant;
        }

        private void Awake()
        {
            if (Engine.Initialized) SetupAndInitializeSceneAssistant();
        }

        private void OnDestroy()
        {
            if (sceneAssistantManager != null && sceneAssistantManager.Initialised) sceneAssistantManager.DestroySceneAssistant();
        }

        private static void SetupAndInitializeSceneAssistant()
        {
            sceneAssistantEditor = GetWindow<SceneAssistantEditor>();
            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
            scriptPlayer = Engine.GetService<IScriptPlayer>();
            stateManager = Engine.GetService<StateManager>();
            scriptsConfiguration = Engine.GetConfiguration<ScriptsConfiguration>();

            sceneAssistantManager.InitializeSceneAssistant();
            sceneAssistantManager.OnSceneAssistantReset += HandleReset;
            scriptPlayer.AddPostExecutionTask(HandleCommandExecuted);
            scriptPlayer.AddPreExecutionTask(HandleCommandStarted);

        }

        private static void HandleReset()
        {
            if (sceneAssistantManager.ObjectList.Count >= objectIndex) return;
            else objectIndex = 0;
        }

        private static UniTask HandleCommandStarted(Command command)
        {
            if (sceneAssistantEditor != null)
            {
                commandPlaying = true;
                sceneAssistantEditor.Repaint();
            }
            return UniTask.CompletedTask;
        }

        private static UniTask HandleCommandExecuted(Command command)
        {
            if (sceneAssistantEditor != null)
            {
                commandPlaying = false;
                sceneAssistantEditor.Repaint();
            }
            return UniTask.CompletedTask;
        }

        public void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(commandPlaying);
            if (Engine.Initialized && sceneAssistantManager?.ObjectList.Count > 0)
            {
                ShowTabs(sceneAssistantManager, sceneAssistantLayout);
            }
            else EditorGUILayout.LabelField("Naninovel is not initialized.");
            EditorGUI.EndDisabledGroup();
        }

        protected virtual void ShowTabs(SceneAssistantManager sceneAssistant, ISceneAssistantLayout layout)
        {
            GUILayout.Space(10f);
            EditorGUI.BeginChangeCheck();
            tabIndex = GUILayout.Toolbar(tabIndex, Tabs, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                search = string.Empty;
                CurrentObject.Params.ForEach(s => s.Changed = true);
            }
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

            if (EditorWindow.mouseOverWindow == sceneAssistantEditor && !isFocused)
            { 
                scriptPlayer.SetWaitingForInputEnabled(true);
                isFocused = true;
            }
            if (EditorWindow.mouseOverWindow != sceneAssistantEditor && isFocused)
            {
                isFocused = false;
            }
        }

        protected virtual void DrawCommandOptions()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            
            if (scriptsConfiguration.EnableVisualEditor && scriptPlayer.PlayedScript != null)
            {
                EditorGUILayout.LabelField("Visual editor", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(160));
                if (VisualEditors.Length == 0)
                {
                    if (GUILayout.Button("Open Visual Editor", GUILayout.Width(145)))
                    {
                        EditorGUIUtility.PingObject(scriptPlayer.PlayedScript);
                        Selection.activeObject = scriptPlayer.PlayedScript;
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Insert command @", GUILayout.Width(130), GUILayout.Height(20))) ClipboardString = InsertCommandLine(CurrentObject.GetCommandLine());
                    if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) ClipboardString = InsertGenericLine(CurrentObject.GetCommandLine(inlined:true));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Insert all @", GUILayout.Width(130), GUILayout.Height(20)))
                    {
                        foreach (var command in CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList)) InsertCommandLine(command);
                        ClipboardString = string.Join("\n", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList));
                    }

                    if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20)))
                    {
                        foreach (var command in CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined:true)) InsertGenericLine(command);
                        ClipboardString = string.Join("", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined:true));
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Insert selected @", GUILayout.Width(130), GUILayout.Height(20)))
                    {
                        foreach (var command in CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, selected:true)) InsertCommandLine(command);
                        ClipboardString = string.Join("\n", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, selected:true));
                    }

                    if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20)))
                    {
                        foreach (var command in CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined:true, selected:true)) InsertGenericLine(command);
                        ClipboardString = string.Join("", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined:true, selected:true));
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(20);
            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("Clipboard", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(160));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy command @", GUILayout.Width(130), GUILayout.Height(20))) ClipboardString = CurrentObject.GetCommandLine();
            if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) ClipboardString = CurrentObject.GetCommandLine(inlined:true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy all @", GUILayout.Width(130), GUILayout.Height(20))) ClipboardString = string.Join("\n", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList));
            if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) ClipboardString = string.Join("", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined:true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy selected @", GUILayout.Width(130), GUILayout.Height(20))) ClipboardString = string.Join("\n", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, selected:true));
            if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) ClipboardString = string.Join("", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined: true, selected:true));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        protected string InsertCommandLine(string content)
        {
            var command = CommandLineView.CreateDefault(scriptPlayer.PlayedIndex, content.Remove("@"), new VisualElement(), true);
            VisualEditors[0].VisualEditor.InsertLine(command, scriptPlayer.PlayedIndex);
            return content;
        }

        protected string InsertGenericLine(string content)
        {
            var genericTextLine = new GenericTextLineView(scriptPlayer.PlayedIndex, content, new VisualElement());
            VisualEditors[0].VisualEditor.InsertLine(genericTextLine, scriptPlayer.PlayedIndex);
            return content;
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

        protected virtual void DrawCommandParameters(List<ICommandData> parameters, ISceneAssistantLayout layout)
        {
            if (parameters == null || parameters.Count == 0 || layout == null) return;

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].GetLayout(layout);
            }
        }
        protected virtual void DrawCommandParameterOptions()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select", EditorStyles.miniButton)) CurrentObject.Params.ForEach(p => p.Selected = true);
            if (GUILayout.Button("Deselect", EditorStyles.miniButton)) CurrentObject.Params.ForEach(p => p.Selected = false);
            if (GUILayout.Button("Default", EditorStyles.miniButton)) CurrentObject.Params.ForEach(p => p.ResetDefault());
            if (GUILayout.Button("Reset", EditorStyles.miniButton)) CurrentObject.Params.ForEach(p => p.ResetState());
            if (GUILayout.Button("Rollback", EditorStyles.miniButton)) foreach(var obj in sceneAssistantManager.ObjectList.Values) obj.Params.ForEach(p => p.ResetState());

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
