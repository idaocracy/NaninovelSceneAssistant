using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;

namespace NaninovelSceneAssistant {

    public interface ISceneAssistantLayout
    {
        void StringField(CommandParam param);
        void StringListField(CommandParam param, string[] stringValues);
        void BoolField(CommandParam param);
        //void IntField(int value);
        void FloatField(CommandParam param);
        //void OptionlessFloatField(float value);
        void SliderField(CommandParam param, float minValue, float maxValue);
        void ColorField(CommandParam param);
        void Vector2Field(CommandParam param);
        void Vector3Field(CommandParam param);
        //void Vector4Field(Vector4 value);
    }



}