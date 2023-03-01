using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;

namespace NaninovelSceneAssistant {

    public interface ISceneAssistantLayout
    {
        void StringField(ParameterValue param,  ParameterValue toggleWith = null);
        void StringListField(ParameterValue param, string[] stringValues,  ParameterValue toggleWith = null);
        void TypeListField<T>(ParameterValue param, Dictionary<string, T> values,  ParameterValue toggleWith = null);
        void BoolField(ParameterValue param,  ParameterValue toggleWith = null);
        void IntField(ParameterValue param, int? minValue, int? maxValue,  ParameterValue toggleWith = null);
        void FloatField(ParameterValue param, float? minValue = null, float? maxValue = null,  ParameterValue toggleWith = null);
        void FloatSliderField(ParameterValue param, float minValue, float maxValue,  ParameterValue toggleWith = null);
        void IntSliderField(ParameterValue param, int minValue, int maxValue,  ParameterValue toggleWith = null);
        void ColorField(ParameterValue param, bool includeAlpha = true,  ParameterValue toggleWith = null);
        void EnumField(ParameterValue param,  ParameterValue toggleWith = null);
        void Vector2Field(ParameterValue param,  ParameterValue toggleWith = null);
        void Vector3Field(ParameterValue param,  ParameterValue toggleWith = null);
        void Vector4Field(ParameterValue param,  ParameterValue toggleWith = null);
        void PosField(ParameterValue param,  ParameterValue toggleWith = null);
        void EmptyField(ParameterValue param,  ParameterValue toggleWith = null);
        void VariableField(VariableValue var);
        void UnlockableField(UnlockableValue unlockable);
    }



}