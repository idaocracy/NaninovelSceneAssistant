using Naninovel;
using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NaninovelSceneAssistant
{
	public partial class SceneAssistantEditor : EditorWindow, ISceneAssistantLayout, ICustomVariableLayout,
		IUnlockableLayout, IScriptLayout
	{
		private static int buttonWidth = 150;
		private static int buttonSize = 12;
		private static float textAreaHeight;

		protected void GenerateLayout(ICommandParameterData data)
		{
			if (!data.FulfillsConditions()) return;
			EditorGUILayout.BeginHorizontal();
			DrawDataToggle(data);

			EditorGUI.BeginDisabledGroup(!data.Selected);
			DrawDataButton(data);
			data.DrawLayout(this);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();
		}

		public void WrapInLayout<T>(Func<T> layoutField, ICommandParameterData<T> data,
			params ToggleGroupData[] toggleGroup)
		{
			var value = layoutField();
			if (data.Selected) data.Value = value;
			CheckToggles(data, toggleGroup);
		}

		public void BoolField(ICommandParameterData<bool> data, params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(() => EditorGUILayout.Toggle(data.Value), data, toggleGroup);

		public void IntField(ICommandParameterData<int> data, int? min = null, int? max = null,
			params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(
				() => EditorGUILayout.IntField("\n", Mathf.Clamp(data.Value, min ?? int.MinValue, max ?? int.MaxValue),
					GUILayout.MinWidth(20)), data, toggleGroup);

		public void IntSliderField(ICommandParameterData<int> data, int min, int max,
			params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(() => EditorGUILayout.IntSlider(data.Value, min, max), data, toggleGroup);

		public void StringField(ICommandParameterData<string> data, params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(() => EditorGUILayout.DelayedTextField(data.Value), data, toggleGroup);

		public void ColorField(ICommandParameterData<Color> data, bool includeAlpha = true, bool includeHDR = false,
			params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(
				() => EditorGUILayout.ColorField(GUIContent.none, data.Value, true, includeAlpha, includeHDR), data,
				toggleGroup);

		public void FloatField(ICommandParameterData<float> data, float? min = null, float? max = null,
			params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(
				() => EditorGUILayout.FloatField("\n",
					Mathf.Clamp(data.Value, min ?? float.MinValue, max ?? float.MaxValue), GUILayout.MinWidth(20)),
				data, toggleGroup);

		public void FloatSliderField(ICommandParameterData<float> data, float min, float max,
			params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(() => EditorGUILayout.Slider((float)data.Value, min, max), data, toggleGroup);

		public void Vector2Field(ICommandParameterData<Vector2> data, params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(() => EditorGUILayout.Vector2Field("", data.Value), data, toggleGroup);

		public void Vector3Field(ICommandParameterData<Vector3> data, params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(() => EditorGUILayout.Vector3Field("", data.Value), data, toggleGroup);

		public void Vector4Field(ICommandParameterData<Vector4> data, params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(() => EditorGUILayout.Vector4Field("", data.Value), data, toggleGroup);

		public void EnumDropdownField(ICommandParameterData<Enum> data, params ToggleGroupData[] toggleGroup)
			=> WrapInLayout(() => EditorGUILayout.EnumPopup((Enum)data.Value), data, toggleGroup);

		public void StringDropdownField(ICommandParameterData<string> data, string[] stringValues,
			params ToggleGroupData[] toggleGroup)
		{
			var stringIndex = stringValues.IndexOf(data.Value);
			stringIndex = EditorGUILayout.Popup(stringValues.IndexOf(data.Value),
				stringValues.Select(s => s.Replace("/", "\u2215")).ToArray());
			CheckToggles(data, toggleGroup);

			if (data.Selected) data.Value = stringValues[stringIndex];
		}

		public void TypeDropdownField<T>(ICommandParameterData<T> data, Dictionary<string, T> values,
			params ToggleGroupData[] toggleGroup)
		{
			var stringIndex = values.Values.Contains(data.Value)
				? Array.IndexOf(values.Values.ToArray(), data.Value)
				: Array.IndexOf(values.Keys.ToArray(), "None");
			stringIndex = EditorGUILayout.Popup(stringIndex, values.Keys.ToArray());
			CheckToggles(data, toggleGroup);
			if (data.Selected)
				data.Value = (T)values.FirstOrDefault(s => s.Key == values.Keys.ToArray()[stringIndex]).Value;
		}

		public void PosField(ICommandParameterData<Vector3> data, CameraConfiguration cameraConfiguration,
			params ToggleGroupData[] toggleGroup)
		{
			var position = cameraConfiguration.WorldToSceneSpace((Vector3)data.Value);
			position.x *= 100;
			position.y *= 100;
			position = EditorGUILayout.Vector3Field("", position);
			position.x /= 100;
			position.y /= 100;
			position = cameraConfiguration.SceneToWorldSpace(position);
			CheckToggles(data, toggleGroup);
			if (data.Selected) data.Value = position;
		}

		public void ListField(IListCommandParameterData data, params ToggleGroupData[] toggleGroup)
		{
			EditorGUILayout.BeginHorizontal();
			DrawDataToggle(data);

			EditorGUI.BeginDisabledGroup(!data.Selected);
			DrawDataButton(data);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(25);
			EditorGUILayout.BeginVertical();

			var defaultButtonWidth = buttonWidth;
			var defaultButtonSize = buttonSize;
			buttonSize = 9;
			buttonWidth = 128;
			foreach (var value in data.Values)
			{
				GenerateLayout(value);
			}

			buttonWidth = defaultButtonWidth;
			buttonSize = defaultButtonSize;

			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

		public void VariableField(VariableData var)
		{
			EditorGUILayout.BeginHorizontal();

			EditorStyles.label.normal.textColor = var.Changed ? Color.yellow : Color.white;

			if (float.TryParse(var.Value, out var floatValue))
			{
				EditorGUI.BeginChangeCheck();
				if (numberTypeIndex == 0)
					var.Value = EditorGUILayout.FloatField(var.Name, floatValue, GUILayout.MinWidth(20)).ToString();
				else var.Value = EditorGUILayout.IntField(var.Name, (int)floatValue, GUILayout.MinWidth(20)).ToString();
				if (EditorGUI.EndChangeCheck())
				{
					var.Changed = true;
				}

				numberTypeIndex = EditorGUILayout.Popup(numberTypeIndex, numberTypes, GUILayout.Width(26));
			}
			else
			{
				EditorGUILayout.LabelField(var.Name, GUILayout.Width(148));

				if (bool.TryParse(var.Value, out var boolValue))
				{
					EditorGUI.BeginChangeCheck();
					var.Value = EditorGUILayout.Toggle("", boolValue).ToString();

					if (EditorGUI.EndChangeCheck())
					{
						var.Changed = true;
					}
				}
				else
				{
					EditorGUI.BeginChangeCheck();
					var.Value = EditorGUILayout.DelayedTextField(var.Value);
					if (EditorGUI.EndChangeCheck())
					{
						var.Changed = true;
					}
				}
			}

			if (var.Changed)
			{
				// if (GUILayout.Button(resetIcon, GUILayout.Width(25), GUILayout.Height(18)))
				// {
				// 	var.Value = var.State;
				// 	var.Changed = false;
				// }
			}

			EditorGUILayout.EndHorizontal();

			EditorStyles.label.normal.textColor = Color.white;
		}

		public void UnlockableField(UnlockableData unlockable)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(unlockable.Name, GUILayout.Width(150));
			unlockable.EnumValue = (UnlockableData.UnlockableState)EditorGUILayout.EnumPopup(unlockable.EnumValue);
			if (unlockable.EnumValue == UnlockableData.UnlockableState.Unlocked) unlockable.Value = true;
			else unlockable.Value = false;
			EditorGUILayout.EndHorizontal();
		}

		public void ScriptField(string scriptName, List<string> labels)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(" \u25B6",
				    GetButtonStyle(Color.green,
					    scriptPlayer.PlayedScript != null && scriptPlayer.PlayedScript.name == scriptName),
				    GUILayout.Width(20), GUILayout.Height(18)))
				PlayScriptAsync(scriptName);
			
			var foldoutIndex = sceneAssistantManager.ScriptDataList.Keys.IndexOf(scriptName);
			scriptFoldouts[foldoutIndex] = EditorGUILayout.Foldout(scriptFoldouts[foldoutIndex], scriptName);
			GUILayout.EndHorizontal();
			
			GUILayout.Space(5);

			if (scriptFoldouts[foldoutIndex])
			{
				if (labels.Count == 0)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(45);
					EditorGUILayout.LabelField("No labels found");
					GUILayout.EndHorizontal();
				}
				else
				{
					foreach (var label in labels)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space(45);
						if (GUILayout.Button(" \u25B6", GUILayout.Width(20)))
						{
							PlayScriptAsync(scriptName, label);
						}
						EditorGUILayout.LabelField(label);
						GUILayout.EndHorizontal();
					}
				}
			}
			
			GUILayout.Space(5);

			async void PlayScriptAsync(string script, string label = null)
			{
				Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
				await stateManager.ResetStateAsync(async () => await scriptPlayer.PreloadAndPlayAsync(script));
				if (!string.IsNullOrEmpty(label))
					await scriptPlayer.RewindAsync(scriptPlayer.PlayedScript.GetLineIndexForLabel(label));
			}
		}

		private static void CheckToggles(ICommandParameterData data, params ToggleGroupData[] toggleGroup)
		{
			if (!data.Selected && toggleGroup.Length == 0) data.ResetState();
			else if (toggleGroup != null && data.Selected && toggleGroup.Any(c => c.Data.Selected))
			{
				var selected = toggleGroup.FirstOrDefault(c => c.Data.Selected);	
				if (selected != null && selected.ResetOnToggle) selected.Data.ResetState();
                
				selected.Data.Selected = false;
			}
			else if (toggleGroup != null && !data.Selected && toggleGroup.Any(c => !c.Data.Selected))
			{
				data.ResetState();
				foreach (var toggleData in toggleGroup) toggleData.Data.ResetState();
			}
		}

		public static bool DrawButton(string label)
		{
			if (GUILayout.Button(label, new GUIStyle(GUI.skin.button) { fontSize = buttonSize }, GUILayout.Width(buttonWidth), GUILayout.Height(20f)))
			{
				//This will take away focus from the clipoard text field if selected at the time. 
				GUI.FocusControl(null);
				return true;
			}
			else return false;
		}

		private static void DrawSearchField()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Search: ", GUILayout.Width(50));
			search = GUILayout.TextField(search);
			GUILayout.EndHorizontal();
		}
		
		public void DrawDataToggle(ICommandParameterData data) => data.Selected = EditorGUILayout.Toggle(data.Selected, GUILayout.Width(15f));
		
		public void DrawDataButton(ICommandParameterData data)
		{
			if (DrawButton(data.Name)) ClipboardString = data.GetCommandValue(paramOnly:true);
		}
	}
}
