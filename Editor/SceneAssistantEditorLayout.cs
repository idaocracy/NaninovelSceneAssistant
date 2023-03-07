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
            private ISceneAssistantLayout sceneAssistantLayout { get => this; }

            public void BoolField(ParameterValue param, ParameterValue toggleWith = null)
                => WrapInLayout(() => param.Value = EditorGUILayout.Toggle((bool)param.Value), param, toggleWith);

            public void IntField(ParameterValue param, int? min = null, int? max = null, ParameterValue toggleWith = null)
            => WrapInModifiedLayout(() => param.Value = EditorGUILayout.IntField(param.IsParameter ? "" : param.Name, Mathf.Clamp((int)param.Value, min ?? int.MinValue, max ?? int.MaxValue)), param, toggleWith);

            public void IntSliderField(ParameterValue param, int min, int max, ParameterValue toggleWith = null)
                => WrapInLayout(() => param.Value = EditorGUILayout.IntSlider((int)param.Value, min, max), param, toggleWith);

            public void StringField(ParameterValue param, ParameterValue toggleWith = null)
                => WrapInLayout(() => param.Value = EditorGUILayout.DelayedTextField((string)param.Value), param, toggleWith);

            public void ColorField(ParameterValue param, bool includeAlpha = true, ParameterValue toggleWith = null)
            => WrapInLayout(() => param.Value = EditorGUILayout.ColorField(GUIContent.none, (Color)param.Value, true, includeAlpha, false), param, toggleWith);

            //FloatField needs to be setup differently to support sliding when displaying label only.
            public void FloatField(ParameterValue param, float? min = null, float? max = null, ParameterValue toggleWith = null)
            => WrapInModifiedLayout(() => param.Value = EditorGUILayout.FloatField(param.IsParameter ? "" : param.Name, Mathf.Clamp((float)param.Value, min ?? float.MinValue, max ?? float.MaxValue)), param, toggleWith);
            public void FloatSliderField(ParameterValue param, float min, float max, ParameterValue toggleWith = null)
                => WrapInLayout(() => param.Value = EditorGUILayout.Slider((float)param.Value, min, max), param, toggleWith);
            public void Vector2Field(ParameterValue param, ParameterValue toggleWith = null)
                => WrapInLayout(() => param.Value = EditorGUILayout.Vector2Field("", (Vector2)param.Value), param, toggleWith);
            public void Vector3Field(ParameterValue param, bool includeZPos = true, ParameterValue toggleWith = null)
                => WrapInLayout(() => { 
                    if (includeZPos) param.Value = EditorGUILayout.Vector3Field("", (Vector3)param.Value); 
                    else param.Value = (Vector3)EditorGUILayout.Vector2Field("", (Vector3)param.Value); 
                }, param, toggleWith);
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
                CheckToggles(param, toggleWith);
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
                CheckToggles(param, toggleWith);
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

                if (includeZPos) position = EditorGUILayout.Vector3Field("", position);
                else position = EditorGUILayout.Vector2Field("", position);
                CheckToggles(param, toggleWith);

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
                if (layoutField != null) layoutField();
                CheckToggles(param, toggleWith);
                EditorGUI.EndDisabledGroup();
            }

            public void WrapInModifiedLayout(Action layoutField, ParameterValue param, ParameterValue toggleWith = null)
            {
                if (param.Condition != null && !param.Condition()) return;

                if (param.IsParameter)
                {
                    DrawValueInfo(param);
                    layoutField();
                    CheckToggles(param, toggleWith);
                }
                else
                {
                    GUILayout.Space(25f);
                    EditorGUI.BeginDisabledGroup(!param.Selected);
                    TextAnchor orginalAlignment = EditorStyles.label.alignment;
                    EditorStyles.label.alignment = TextAnchor.MiddleCenter;
                    layoutField();
                    EditorStyles.label.alignment = orginalAlignment;
                    EditorGUI.EndDisabledGroup();
                }
            }

            private static void CheckToggles(ParameterValue param, ParameterValue toggleWith)
            {
                if (toggleWith != null && param.Selected == toggleWith.Selected == true) toggleWith.Selected = false;
            }

            public static bool DrawButton(string label)
            {
                if (GUILayout.Button(label, GUILayout.Width(150)))
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
                    GUILayout.Label(param.Name, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(150));
                }
            }
        }
}
