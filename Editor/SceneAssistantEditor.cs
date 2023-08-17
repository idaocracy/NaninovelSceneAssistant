using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NaninovelSceneAssistant
{
	[Serializable]
	public partial class SceneAssistantEditor : EditorWindow
	{
		private static SceneAssistantEditor sceneAssistantEditor;
		public static string[] Tabs { get; protected set; } = new string[] { "Objects", "Variables", "Unlockables", "Scripts" };
		protected static INaninovelObjectData CurrentObject => sceneAssistantManager.ObjectList.Values?.ToArray()[objectIndex];
		protected static string ClipboardString { get => clipboardString; set { clipboardString = value; EditorGUIUtility.systemCopyBuffer = value; if (logResults) Debug.Log(value); } }
		protected static ScriptImporterEditor[] VisualEditors => Resources.FindObjectsOfTypeAll<ScriptImporterEditor>();
		protected static bool ExcludeState { get => CommandParameterData.ExcludeState; set => CommandParameterData.ExcludeState = value; }
		protected static bool ExcludeDefault { get => CommandParameterData.ExcludeDefault; set => CommandParameterData.ExcludeDefault = value; }
		
		private static SceneAssistantManager sceneAssistantManager;
		private static IScriptPlayer scriptPlayer;
		private static IStateManager stateManager;
		private static IInputManager inputManager;
		private static ScriptsConfiguration scriptsConfiguration;

		private static int objectIndex;
		private static int tabIndex;
		private static int poseIndex;
		private static int numberTypeIndex;
		private static string clipboardString;
		private static string search;
		private static string poseName;
		private static Vector2 scrollPos;
		private static bool logResults;
		private static bool defaultRollbackValue;
		private static bool disableRollback;
		private static string[] numberTypes = new string[]{"f", "i"};
		
		private static int lastIndex = 0;
		private static string lastObject;

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

		private static void SetupAndInitializeSceneAssistant()
		{
			sceneAssistantEditor = GetWindow<SceneAssistantEditor>();
			sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
			scriptPlayer = Engine.GetService<IScriptPlayer>();
			stateManager = Engine.GetService<StateManager>();
			inputManager = Engine.GetService<InputManager>();
			scriptsConfiguration = Engine.GetConfiguration<ScriptsConfiguration>();

			if(sceneAssistantManager.Initialized) return;
			sceneAssistantManager.InitializeSceneAssistant();
			sceneAssistantManager.OnSceneAssistantCleared += HandleSceneAssistantCleared;
			sceneAssistantManager.OnSceneAssistantReset += HandleSceneAssistantReset;

			defaultRollbackValue = inputManager.GetRollback().Enabled;
		}

		private void OnDestroy()
		{
			if (sceneAssistantManager != null && sceneAssistantManager.Initialized)
			{
				sceneAssistantManager.DestroySceneAssistant();
				sceneAssistantManager.OnSceneAssistantCleared -= HandleSceneAssistantCleared;
				sceneAssistantManager.OnSceneAssistantReset -= HandleSceneAssistantReset;
			}
		}

		private static void HandleSceneAssistantCleared() 
		{
			lastObject = CurrentObject.Id;
			lastIndex = objectIndex;
			objectIndex = 0;
			sceneAssistantEditor.Repaint();
		} 
		
		private static void HandleSceneAssistantReset() 
		{
			if(!string.IsNullOrEmpty(lastObject)) 
			{
				if(sceneAssistantManager.ObjectList.Keys.ElementAt(lastIndex) == lastObject) objectIndex = lastIndex;
				else if(sceneAssistantManager.ObjectList.Keys.Contains(lastObject)) 
				{
					var newIndex = sceneAssistantManager.ObjectList.Keys.ToArray().IndexOf(lastObject);
					objectIndex = newIndex;
				}
				else objectIndex = 0;
			}
			else objectIndex = 0;
			
			sceneAssistantEditor.Repaint();
		} 

		public void OnGUI()
		{
			if (Engine.Initialized && sceneAssistantManager.Initialized)
			{
				ShowTabs();
			}
			else EditorGUILayout.LabelField("Naninovel is not initialized.");
		}

		protected virtual void ShowTabs()
		{
			GUILayout.Space(10f);
			EditorGUI.BeginChangeCheck();
			tabIndex = GUILayout.Toolbar(tabIndex, Tabs, EditorStyles.toolbarButton);
			if (EditorGUI.EndChangeCheck()) search = string.Empty;

			GUILayout.Space(5f);

			DrawScriptPlayerOptions();

			switch (tabIndex)
			{
				case 0:
					DrawSceneAssistant();
					break;
				case 1:
					DrawCustomVariables(sceneAssistantManager.CustomVarList);
					break;
				case 2:
					DrawUnlockables(sceneAssistantManager.UnlockablesList);
					break;
				case 3:
					DrawScripts(sceneAssistantManager.ScriptsList);
					break;
			}
		}

		protected virtual void DrawSceneAssistant()
		{
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginVertical();

			EditorGUI.BeginDisabledGroup(!sceneAssistantManager.IsAvailable);

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			DrawCommandOptions();
			GUILayout.Space(5);
			DrawTypeOptions();

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			DrawIdField();

			EditorGUILayout.Space(10);

			if(sceneAssistantManager.IsAvailable) DrawCommandParameters(CurrentObject.CommandParameters);

			EditorGUILayout.Space(5);
			DrawCommandParameterOptions();
			DrawActorPoseOptions();
			DrawCommandTextArea();

			GUILayout.EndVertical();
			GUILayout.Space(10);
			GUILayout.EndHorizontal();

			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndScrollView();
		}

		private GUIStyle GetButtonStyle(Color color, bool condition, int fontSize = 10)
		{
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.normal.textColor = condition ? color : Color.white;
			buttonStyle.active.textColor = condition ? color : Color.white;
			buttonStyle.hover.textColor = condition ? color : Color.white;
			buttonStyle.fontStyle = FontStyle.Bold;
			buttonStyle.fontSize = fontSize;
			return buttonStyle;
		}

		public void DrawScriptPlayerOptions()
		{
			EditorGUILayout.LabelField("Script Player", EditorStyles.centeredGreyMiniLabel);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (DrawScriptPlayerButton("\u25B6", Color.green, scriptPlayer.Playing))
			{
				if (!scriptPlayer.Playing) scriptPlayer.Play(); 
				inputManager.GetContinue().Activate(1);
				inputManager.GetRollback().Enabled = defaultRollbackValue;
			}

			if (DrawScriptPlayerButton("\u2161", Color.yellow, scriptPlayer.Playing && scriptPlayer.WaitingForInput))
			{
				SyncAndExecuteAsync(() => scriptPlayer.SetWaitingForInputEnabled(true));
			}

			if (DrawScriptPlayerButton("\uFFED", Color.red, !scriptPlayer.Playing, 18))
			{
				SyncAndExecuteAsync(scriptPlayer.Stop);
				if (disableRollback) inputManager.GetRollback().Enabled = false;
				if (scriptPlayer.WaitingForInput) scriptPlayer.SetWaitingForInputEnabled(false);
			}

			if (stateManager.Configuration.EnableStateRollback)
			{
				if (GUILayout.Button("\u25AE" + " \u25C0", new GUIStyle(GUI.skin.button) { fontSize = 7, fontStyle = FontStyle.Bold }, GUILayout.Height(20), GUILayout.Width(25)))
				{
					inputManager.GetRollback().Activate(1);
					SyncAndExecuteAsync(() => scriptPlayer.SetWaitingForInputEnabled(true));
				}
			}

			if (GUILayout.Button("\u25B6" + "\u25AE", new GUIStyle(GUI.skin.button) { fontSize = 8, fontStyle = FontStyle.Bold }, GUILayout.Height(20), GUILayout.Width(25)))
			{
				foreach (var obj in sceneAssistantManager.ObjectList.Values) obj.CommandParameters.ForEach(p => p.ResetState());
				inputManager.GetContinue().Activate(1);
				SyncAndExecuteAsync(() => scriptPlayer.SetWaitingForInputEnabled(true));
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			bool DrawScriptPlayerButton(string content, Color color, bool condition, int fontSize = 10)
			{
				if (GUILayout.Button(content, GetButtonStyle(color, condition, fontSize), GUILayout.Height(20), GUILayout.Width(20)))
				{
					return true;
				}
				else return false;
			}
		}

		public async void RollbackAsync() => await stateManager.RollbackAsync(s => s.PlayerRollbackAllowed);

		public async void SyncAndExecuteAsync(Action action)
		{
			await scriptPlayer.SynchronizeAndDoAsync(() => UniTaskify(action));

			UniTask UniTaskify(Action task)
			{
				task();
				return UniTask.CompletedTask;
			}
		}

		protected virtual void DrawCommandOptions()
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			DrawVisualEditorOptions();

			GUILayout.Space(20);
			DrawClipboardOptions();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		private void DrawVisualEditorOptions()
		{
			if (!scriptsConfiguration.EnableVisualEditor || scriptPlayer.PlayedScript == null) return;
			
			GUILayout.BeginVertical();
			
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
				if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) ClipboardString = InsertGenericLine(CurrentObject.GetCommandLine(inlined: true));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Insert all @", GUILayout.Width(130), GUILayout.Height(20))) DrawInsertCommandsButton(false, false);
				if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) DrawInsertCommandsButton(true, false);
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Insert selected @", GUILayout.Width(130), GUILayout.Height(20))) DrawInsertCommandsButton(false, true);
				if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) DrawInsertCommandsButton(true, true);
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();

			void DrawInsertCommandsButton(bool inlined, bool selected)
			{
				foreach (var command in CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined, selected)) InsertGenericLine(command);
				ClipboardString = string.Join("", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined, selected));
			} 
		}

		protected string InsertCommandLine(string content)
		{
			var contents = content.Split('\n') ?? new string[] { content };

			foreach(var str in contents)
			{
				var command = CommandLineView.CreateDefault(scriptPlayer.PlayedIndex, str.Remove("@"), new VisualElement(), true);
				VisualEditors[0].VisualEditor.InsertLine(command, scriptPlayer.PlayedIndex);
			}

			return content;
		}

		protected string InsertGenericLine(string content)
		{
			var genericTextLine = new GenericTextLineView(scriptPlayer.PlayedIndex, content, new VisualElement());
			VisualEditors[0].VisualEditor.InsertLine(genericTextLine, scriptPlayer.PlayedIndex);
			return content;
		}

		private static void DrawClipboardOptions()
		{
			GUILayout.BeginVertical();

			EditorGUILayout.LabelField("Clipboard", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(160));

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Copy command @", GUILayout.Width(130), GUILayout.Height(20))) ClipboardString = CurrentObject.GetCommandLine();
			if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) ClipboardString = CurrentObject.GetCommandLine(inlined: true);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Copy all @", GUILayout.Width(130), GUILayout.Height(20))) ClipboardString = string.Join("\n", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList));
			if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) ClipboardString = string.Join("", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined: true));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Copy selected @", GUILayout.Width(130), GUILayout.Height(20))) ClipboardString = string.Join("\n", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, selected: true));
			if (GUILayout.Button("[]", GUILayout.Width(25), GUILayout.Height(20))) ClipboardString = string.Join("", CurrentObject.GetAllCommands(sceneAssistantManager.ObjectList, sceneAssistantManager.ObjectTypeList, inlined: true, selected: true));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
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
			if (sceneAssistantManager.IsAvailable)
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
		}

		protected virtual void DrawCommandParameters(List<ICommandParameterData> parameters)
		{
			if (parameters == null || parameters.Count == 0) return;

			EditorGUI.BeginChangeCheck();
			for (int i = 0; i < parameters.Count; i++)
			{
				if(parameters[i] is IListCommandParameterData) parameters[i].DrawLayout(this);
				else GenerateLayout(parameters[i]);
			}
			if(EditorGUI.EndChangeCheck()) poseIndex = 0;
		}
		
		protected virtual void DrawActorPoseOptions()
		{
			if(sceneAssistantManager.IsAvailable && (CurrentObject is IOrthoActorData orthoData)) 
			{
				EditorGUILayout.Space(5);
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("Pose options", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(160));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Load:", GUILayout.Width(40));
				EditorGUI.BeginChangeCheck();
				poseIndex = EditorGUILayout.Popup(poseIndex, orthoData.GetPoses(), new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleCenter });
				if(EditorGUI.EndChangeCheck())
				{
					if(orthoData.GetPoses()[poseIndex] != "None") 
					{
						var appliedPose = orthoData.GetPoses()[poseIndex].Split(":".ToCharArray()).Last();
						orthoData.ApplyPose(appliedPose);
					}
				}
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Save:", GUILayout.Width(40));
				poseName = GUILayout.TextField(poseName);
				
				EditorGUI.BeginDisabledGroup(String.IsNullOrEmpty(poseName));
				EditorGUI.BeginChangeCheck();
					if (GUILayout.Button("Save Pose", EditorStyles.miniButton, GUILayout.Width(75))) orthoData.AddPose(poseName);
					if (GUILayout.Button("Save Shared", EditorStyles.miniButton, GUILayout.Width(90))) orthoData.AddSharedPose(poseName);
				if(EditorGUI.EndChangeCheck())
				{
					var poseValue = orthoData.GetPoses().LastOrDefault(p => p.EndsWith(":" + poseName));
					poseIndex = Array.IndexOf(orthoData.GetPoses().ToArray(), poseValue);
					poseName = String.Empty;
				}
				EditorGUI.EndDisabledGroup();				
				GUILayout.EndHorizontal();

				EditorGUILayout.Space(5);
			}
		}
		
		protected virtual void DrawCommandParameterOptions()
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Select", EditorStyles.miniButton)) CurrentObject.CommandParameters.ForEach(p => p.Selected = true);
			if (GUILayout.Button("Deselect", EditorStyles.miniButton)) CurrentObject.CommandParameters.ForEach(p => p.Selected = false);
			if (GUILayout.Button("Default", EditorStyles.miniButton)) CurrentObject.CommandParameters.ForEach(p => p.ResetDefault());
			if (GUILayout.Button("Reset", EditorStyles.miniButton)) CurrentObject.CommandParameters.ForEach(p => p.ResetState());
			if (GUILayout.Button("Rollback", EditorStyles.miniButton)) foreach(var obj in sceneAssistantManager.ObjectList.Values) obj.CommandParameters.ForEach(p => p.ResetState());

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
			if(!sceneAssistantManager.IsAvailable) clipboardString = string.Empty;
			else clipboardString = EditorGUILayout.TextArea(clipboardString, style, GUILayout.Height(textAreaHeight + 10));

			GUILayout.BeginHorizontal();
			logResults = EditorGUILayout.ToggleLeft("Log Results", logResults, EditorStyles.miniLabel, GUILayout.Width(80));
			ExcludeState = EditorGUILayout.ToggleLeft("Exclude State", ExcludeState, EditorStyles.miniLabel, GUILayout.Width(90));
			ExcludeDefault = EditorGUILayout.ToggleLeft("Exclude Defaults", ExcludeDefault, EditorStyles.miniLabel, GUILayout.Width(105));
			disableRollback = EditorGUILayout.ToggleLeft("Disable Rollback on Stop", disableRollback, EditorStyles.miniLabel, GUILayout.Width(140));
			GUILayout.EndHorizontal();
		}

		protected virtual void DrawCustomVariables(SortedList<string, VariableData> variables)
		{
			if (variables == null && variables.Count == 0) return;
			GUILayout.Space(5);

			DrawSearchField();

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			foreach (VariableData variable in variables.Values)
			{
				if (!string.IsNullOrEmpty(search) && variable.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;

				EditorGUILayout.BeginHorizontal();
				variable.DisplayField(this);
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
		}

		protected virtual void DrawUnlockables(SortedList<string, UnlockableData> unlockables)
		{
			if (unlockables == null) return;
			GUILayout.Space(5);

			DrawSearchField();

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			foreach (UnlockableData unlockable in unlockables.Values)
			{
				if (!string.IsNullOrEmpty(search) && unlockable.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;

				EditorGUILayout.BeginHorizontal();
				unlockable.DisplayField(this);
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
