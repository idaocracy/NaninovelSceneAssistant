using Naninovel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NaninovelSceneAssistant
{
    public partial class SceneAssistantEditor : EditorWindow, ISceneAssistantLayout
    {
        private static int buttonWidth = 150;
        private static int buttonSize = 12;
        private static float textAreaHeight;

        private ISceneAssistantLayout sceneAssistantLayout { get => this; }
        public void BoolField(ICommandParameterData<bool> data, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.Toggle(data.Value), data, toggleWith);
        public void IntField(ICommandParameterData<int> data, int? min = null, int? max = null, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.IntField(Mathf.Clamp(data.Value, min ?? int.MinValue, max ?? int.MaxValue)), data, toggleWith);
        public void IntSliderField(ICommandParameterData<int> data, int min, int max, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.IntSlider(data.Value, min, max), data, toggleWith);
        public void StringField(ICommandParameterData<string> data, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.DelayedTextField(data.Value), data, toggleWith);
        public void ColorField(ICommandParameterData<Color> data, bool includeAlpha = true, bool includeHDR = false, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.ColorField(GUIContent.none, data.Value, true, includeAlpha, false), data, toggleWith);
        public void FloatField(ICommandParameterData<float> data, float? min = null, float? max = null, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.FloatField("\n", Mathf.Clamp(data.Value, min ?? float.MinValue, max ?? float.MaxValue), GUILayout.MinWidth(20)), data, toggleWith);
        public void FloatSliderField(ICommandParameterData<float> data, float min, float max, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.Slider((float)data.Value, min, max), data, toggleWith);
        public void Vector2Field(ICommandParameterData<Vector2> data, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.Vector2Field("", data.Value), data, toggleWith);
        public void Vector3Field(ICommandParameterData<Vector3> data, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.Vector3Field("", data.Value), data, toggleWith);
        public void Vector4Field(ICommandParameterData<Vector4> data, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.Vector4Field("", data.Value), data, toggleWith);
        public void EnumField(ICommandParameterData<Enum> data, ICommandParameterData toggleWith = null)
            => WrapInLayout(() => EditorGUILayout.EnumPopup((Enum)data.Value), data, toggleWith);

        public void StringListField(ICommandParameterData<string> data, string[] stringValues, ICommandParameterData toggleWith = null)
        {
            if (data.HasCondition != null && data.HasCondition() == false) return;
            EditorGUILayout.BeginHorizontal();
            var stringIndex = stringValues.IndexOf(data.Value.ToString());
            DrawValueInfo(data);
            EditorGUI.BeginDisabledGroup(!data.Selected);
            stringIndex = EditorGUILayout.Popup(stringIndex, stringValues);
            CheckToggles(data, toggleWith);
            EditorGUI.EndDisabledGroup();

            data.Value = stringValues[stringIndex];
            EditorGUILayout.EndHorizontal();
        }

        public void TypeListField<T>(ICommandParameterData<T> data, Dictionary<string, T> values, ICommandParameterData toggleWith = null)
        {
            if (data.HasCondition != null && data.HasCondition() == false) return;
            EditorGUILayout.BeginHorizontal();

            var stringIndex = values.Values.Contains(data.Value) ? Array.IndexOf(values.Values.ToArray(), data.Value) : Array.IndexOf(values.Keys.ToArray(), "None");
            DrawValueInfo(data);

            EditorGUI.BeginDisabledGroup(!data.Selected);
            stringIndex = EditorGUILayout.Popup(stringIndex, values.Keys.ToArray());
            CheckToggles(data, toggleWith);
            EditorGUI.EndDisabledGroup();

            data.Value = (T)values.FirstOrDefault(s => s.Key == values.Keys.ToArray()[stringIndex]).Value;
            EditorGUILayout.EndHorizontal();
        }

        public void PosField(ICommandParameterData<Vector3> data, CameraConfiguration cameraConfiguration, ICommandParameterData toggleWith = null)
        {
            if (data.HasCondition != null && data.HasCondition() == false) return;
            EditorGUILayout.BeginHorizontal();
            DrawValueInfo(data);
            var position = cameraConfiguration.WorldToSceneSpace((Vector3)data.Value);
            position.x *= 100;
            position.y *= 100;

            EditorGUI.BeginDisabledGroup(!data.Selected);
            position = EditorGUILayout.Vector3Field("", position);
            CheckToggles(data, toggleWith);
            EditorGUI.EndDisabledGroup();

            position.x /= 100;
            position.y /= 100;
            position = cameraConfiguration.SceneToWorldSpace(position);
            data.Value = position;
            EditorGUILayout.EndHorizontal();
        }

        public void VariableField(VariableData var)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(var.Name, GUILayout.Width(150));
            var.Value = EditorGUILayout.DelayedTextField(var.Value);
            EditorGUILayout.EndHorizontal();
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

        public void WrapInLayout<T>(Func<T> layoutField, ICommandParameterData<T> data, ICommandParameterData toggleWith = null)
        {
            if (data.HasCondition != null && data.HasCondition() == false) return;
            EditorGUILayout.BeginHorizontal();
            DrawValueInfo(data);
            EditorGUI.BeginDisabledGroup(!data.Selected);
            data.Value = layoutField();
            CheckToggles(data, toggleWith);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private static void CheckToggles(ICommandParameterData data, ICommandParameterData toggleWith)
        {
            if (!data.Selected && toggleWith == null) data.ResetState();
            else if (toggleWith != null && data.Selected && toggleWith.Selected) toggleWith.Selected = false;
            else if (toggleWith != null && !data.Selected && !toggleWith.Selected)
            {
                data.ResetState();
                toggleWith.ResetState();
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
        public void DrawValueInfo<T>(ICommandParameterData<T> data)
        {
            data.Selected = EditorGUILayout.Toggle(data.Selected, GUILayout.Width(15f));
            EditorGUI.BeginDisabledGroup(!data.Selected);
            if (DrawButton(data.Name)) ClipboardString = data.FormatValue();
            EditorGUI.EndDisabledGroup();
        }

        public void ListButtonField(IListCommandParameterData data, ICommandParameterData toggleWith = null)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            data.Selected = EditorGUILayout.Toggle(data.Selected, GUILayout.Width(15));
            if (EditorGUI.EndChangeCheck())
            {
                data.Values.ForEach(s => s.Selected = data.Selected);
            }

            EditorGUI.BeginDisabledGroup(!data.Selected);
            if (DrawButton(data.Name))
            {
                var stateValue = CommandParameterData.ExcludeState;
                CommandParameterData.ExcludeState = false;
                ClipboardString = data.GetCommandValue(paramOnly: true);
                CommandParameterData.ExcludeState = stateValue;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(25);

            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(!data.Selected);
            var defaultButtonWidth = buttonWidth;
            var defaultButtonSize = buttonSize;
            buttonSize = 9;
            buttonWidth = 128;
            foreach (var value in data.Values)
            {
                value.GetLayout(sceneAssistantLayout);
            }
            buttonWidth = defaultButtonWidth;
            buttonSize = defaultButtonSize;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }
    }
}
