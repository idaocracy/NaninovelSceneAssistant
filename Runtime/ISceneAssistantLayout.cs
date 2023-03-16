using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;

namespace NaninovelSceneAssistant {

    public interface ISceneAssistantLayout
    {
        void StringField(ICommandData<string> param, ICommandData toggleWith = null);
        void StringListField(ICommandData<string> param, string[] stringValues, ICommandData toggleWith = null);
        void TypeListField<T>(ICommandData<T> param, Dictionary<string, T> values, ICommandData toggleWith = null);
        void BoolField(ICommandData<bool> param, ICommandData toggleWith = null);
        void IntField(ICommandData<int> param, int? minValue, int? maxValue, ICommandData toggleWith = null);
        void FloatField(ICommandData<float> param, float? minValue = null, float? maxValue = null, ICommandData toggleWith = null);
        void FloatSliderField(ICommandData<float> param, float minValue, float maxValue, ICommandData toggleWith = null);
        void IntSliderField(ICommandData<int> param, int minValue, int maxValue, ICommandData toggleWith = null);
        void ColorField(ICommandData<Color> param, bool includeAlpha = true, bool includeHDR = false, ICommandData toggleWith = null);
        void EnumField(ICommandData<Enum> param, ICommandData toggleWith = null);
        void Vector2Field(ICommandData<Vector2> param, ICommandData toggleWith = null);
        void Vector3Field(ICommandData<Vector3> param, ICommandData toggleWith = null);
        void Vector4Field(ICommandData<Vector4> param, ICommandData toggleWith = null);
        void PosField(ICommandData<Vector3> param, CameraConfiguration cameraConfiguration, ICommandData toggleWith = null);
        void EmptyField(ICommandData param, ICommandData toggleWith = null);
        void ListButtonField(IListCommandData list, ICommandData toggleWith = null);
        void VariableField(VariableValue var);
        void UnlockableField(UnlockableValue unlockable);
    }



}