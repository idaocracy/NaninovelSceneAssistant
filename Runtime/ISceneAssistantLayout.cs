using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;

namespace NaninovelSceneAssistant {

    public interface ISceneAssistantLayout
    {
        void StringField(CommandParam param, Func<bool> condition = null);
        void StringListField(CommandParam param, string[] stringValues, Func<bool> condition = null);
        void BoolField(CommandParam param, Func<bool> condition = null);
        void IntField(CommandParam param, int? minValue, int? maxValue, Func<bool> condition = null);
        void FloatField(CommandParam param, float? minValue = null, float? maxValue = null, Func<bool> condition = null);
        void SliderField(CommandParam param, float minValue, float maxValue, Func<bool> condition = null);
        void ColorField(CommandParam param, Func<bool> condition = null);
        void EnumField(CommandParam param, Func<bool> condition = null);
        void Vector2Field(CommandParam param, Func<bool> condition = null);
        void Vector3Field(CommandParam param, Func<bool> condition = null);
        void Vector4Field(CommandParam param, Func<bool> condition = null);
        void PosField(CommandParam param, Func<bool> condition = null);
        void CustomVarField(CustomVar var);
        void UnlockableField(Unlockable unlockable, int stateIndex, string[] states);
    }



}