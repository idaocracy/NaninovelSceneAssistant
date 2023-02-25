using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;

namespace NaninovelSceneAssistant {

    public interface ISceneAssistantLayout
    {
        void StringField(CommandParam param, Func<bool> condition = null, CommandParam toggleWith = null);
        void StringListField(CommandParam param, string[] stringValues, Func<bool> condition = null, CommandParam toggleWith = null);
        void BoolField(CommandParam param, Func<bool> condition = null, CommandParam toggleWith = null);
        void IntField(CommandParam param, int? minValue, int? maxValue, Func<bool> condition = null, CommandParam toggleWith = null);
        void FloatField(CommandParam param, float? minValue = null, float? maxValue = null, Func<bool> condition = null, CommandParam toggleWith = null);
        void SliderField(CommandParam param, float minValue, float maxValue, Func<bool> condition = null, CommandParam toggleWith = null);
        void ColorField(CommandParam param, bool includeAlpha = true, Func<bool> condition = null, CommandParam toggleWith = null);
        void EnumField(CommandParam param, Func<bool> condition = null, CommandParam toggleWith = null);
        void Vector2Field(CommandParam param, Func<bool> condition = null, CommandParam toggleWith = null);
        void Vector3Field(CommandParam param, Func<bool> condition = null, CommandParam toggleWith = null);
        void Vector4Field(CommandParam param, Func<bool> condition = null, CommandParam toggleWith = null);
        void PosField(CommandParam param, Func<bool> condition = null, CommandParam toggleWith = null);
        void CustomVarField(CustomVar var);
        void UnlockableField(Unlockable unlockable, int stateIndex, string[] states);
    }



}