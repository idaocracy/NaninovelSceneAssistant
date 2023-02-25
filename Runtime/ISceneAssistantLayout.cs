using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;

namespace NaninovelSceneAssistant {

    public interface ISceneAssistantLayout
    {
        void StringField(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null);
        void StringListField(ParameterValue param, string[] stringValues, Func<bool> condition = null, ParameterValue toggleWith = null);
        void BoolField(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null);
        void IntField(ParameterValue param, int? minValue, int? maxValue, Func<bool> condition = null, ParameterValue toggleWith = null);
        void FloatField(ParameterValue param, float? minValue = null, float? maxValue = null, Func<bool> condition = null, ParameterValue toggleWith = null);
        void SliderField(ParameterValue param, float minValue, float maxValue, Func<bool> condition = null, ParameterValue toggleWith = null);
        void ColorField(ParameterValue param, bool includeAlpha = true, Func<bool> condition = null, ParameterValue toggleWith = null);
        void EnumField(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null);
        void Vector2Field(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null);
        void Vector3Field(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null);
        void Vector4Field(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null);
        void PosField(ParameterValue param, Func<bool> condition = null, ParameterValue toggleWith = null);
        void CustomVarField(VariableValue var);
        void UnlockableField(UnlockableValue unlockable, int stateIndex, string[] states);
    }



}