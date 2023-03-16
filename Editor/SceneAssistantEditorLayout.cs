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
        public void BoolField(ICommandData<bool> param, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Toggle(param.Value), param, toggleWith);
        public void IntField(ICommandData<int> param, int? min = null, int? max = null, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.IntField(Mathf.Clamp(param.Value, min ?? int.MinValue, max ?? int.MaxValue)), param, toggleWith);
        public void IntSliderField(ICommandData<int> param, int min, int max, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.IntSlider(param.Value, min, max), param, toggleWith);
        public void StringField(ICommandData<string> param, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.DelayedTextField(param.Value), param, toggleWith);
        public void ColorField(ICommandData<Color> param, bool includeAlpha = true, bool includeHDR = false, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.ColorField(GUIContent.none, param.Value, true, includeAlpha, false), param, toggleWith);
        public void FloatField(ICommandData<float> param, float? min = null, float? max = null, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.FloatField("value", Mathf.Clamp(param.Value, min ?? float.MinValue, max ?? float.MaxValue), GUILayout.MinWidth(20)), param, toggleWith);
        public void FloatSliderField(ICommandData<float> param, float min, float max, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Slider((float)param.Value, min, max), param, toggleWith);
        public void Vector2Field(ICommandData<Vector2> param, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector2Field("", param.Value), param, toggleWith);
        public void Vector3Field(ICommandData<Vector3> param, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector3Field("", param.Value), param, toggleWith);
        public void Vector4Field(ICommandData<Vector4> param, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.Vector4Field("", param.Value), param, toggleWith);
        public void EnumField(ICommandData<Enum> param, ICommandData toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.EnumPopup((Enum)param.Value), param, toggleWith);
        public void EmptyField(ICommandData param, ICommandData toggleWith = null)
            => WrapInLayout(null, param, toggleWith);

        public void StringListField(ICommandData<string> param, string[] stringValues, ICommandData toggleWith = null)
        {
            EditorGUILayout.BeginHorizontal();
            if (param.HasCondition != null && param.HasCondition() == false) return;
            var stringIndex = stringValues.IndexOf(param.Value.ToString());
            DrawValueInfo(param);

            EditorGUI.BeginDisabledGroup(!param.Selected);
            stringIndex = EditorGUILayout.Popup(stringIndex, stringValues);
            CheckToggles(param, toggleWith);
            EditorGUI.EndDisabledGroup();

            param.Value = stringValues[stringIndex];
            if (!param.Selected && toggleWith == null) param.ResetState();
            EditorGUILayout.EndHorizontal();
        }

        public void TypeListField<T>(ICommandData<T> param, Dictionary<string, T> values, ICommandData toggleWith = null)
        {
            EditorGUILayout.BeginHorizontal();
            if (param.HasCondition != null && param.HasCondition() == false) return;

            var stringIndex = values.Values.Contains(param.Value) ? Array.IndexOf(values.Values.ToArray(), param.Value) : Array.IndexOf(values.Keys.ToArray(), "None");
            DrawValueInfo(param);

            EditorGUI.BeginDisabledGroup(!param.Selected);
            stringIndex = EditorGUILayout.Popup(stringIndex, values.Keys.ToArray());
            CheckToggles(param, toggleWith);
            EditorGUI.EndDisabledGroup();

            param.Value = (T)values.FirstOrDefault(s => s.Key == values.Keys.ToArray()[stringIndex]).Value;
            if (!param.Selected && toggleWith == null) param.ResetState();
            EditorGUILayout.EndHorizontal();
        }

        public void PosField(ICommandData<Vector3> param, CameraConfiguration cameraConfiguration, ICommandData toggleWith = null)
        {
            EditorGUILayout.BeginHorizontal();
            if (param.HasCondition != null && param.HasCondition() == false) return;
            DrawValueInfo(param);
            var position = cameraConfiguration.WorldToSceneSpace((Vector3)param.Value);
            position.x *= 100;
            position.y *= 100;

            EditorGUI.BeginDisabledGroup(!param.Selected);
            position = EditorGUILayout.Vector3Field("", position);
            CheckToggles(param, toggleWith);
            EditorGUI.EndDisabledGroup();

            position.x /= 100;
            position.y /= 100;
            position = cameraConfiguration.SceneToWorldSpace(position);
            param.Value = position;
            if (!param.Selected && toggleWith == null) param.ResetState();
            EditorGUILayout.EndHorizontal();
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

        public void WrapInLayout(Action layoutField, ICommandData param, ICommandData toggleWith = null)
        {
            EditorGUILayout.BeginHorizontal();
            if (param.HasCondition != null && param.HasCondition() == false) return;
            DrawValueInfo(param);
            EditorGUI.BeginDisabledGroup(!param.Selected);
            layoutField();
            if (!param.Selected && toggleWith == null) param.ResetState();
            CheckToggles(param, toggleWith);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private static void CheckToggles(ICommandData param, ICommandData toggleWith)
        {
            if (toggleWith != null && param.Selected && toggleWith.Selected) toggleWith.Selected = false;
            else if (toggleWith != null && !param.Selected && !toggleWith.Selected)
            {
                param.ResetState();
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
        public void DrawValueInfo(ICommandData param)
        {
            param.Selected = EditorGUILayout.Toggle(param.Selected, GUILayout.Width(15f));
            EditorGUI.BeginDisabledGroup(!param.Selected);
            if (DrawButton(param.Name)) ClipboardString = param.GetCommandValue(paramOnly:true);
            EditorGUI.EndDisabledGroup();
        }

        public void ListButtonField(IListCommandData param, ICommandData toggleWith = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            param.Selected = EditorGUILayout.Toggle(param.Selected, GUILayout.Width(15));
            if (EditorGUI.EndChangeCheck())
            {
                if (!param.Selected) param.Values.ForEach(s => s.Selected = false);
                else param.Values.ForEach(s => s.Selected = true);
            }

            if (DrawButton(param.Name))
            {
                ClipboardString = param.GetCommandValue(paramOnly: true);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(25);

            EditorGUILayout.BeginVertical();
            var defaultButtonWidth = buttonWidth;
            var defaultButtonSize = buttonSize;
            buttonSize = 9;
            buttonWidth = 128;
            foreach (var value in param.Values)
            {
                value.GetLayout(sceneAssistantLayout);
            }
            buttonWidth = defaultButtonWidth;
            buttonSize = defaultButtonSize;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
