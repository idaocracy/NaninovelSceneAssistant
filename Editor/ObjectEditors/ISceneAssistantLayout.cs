using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;

namespace NaninovelSceneAssistant {

    public interface ISceneAssistantLayout
    {
        //void StringField(string value);
        //void StringListField(string value);
        void BoolField(CommandParam param);
        //void IntField(int value);
        //void FloatField(float value);
        //void OptionlessFloatField(float value);
        void SliderField(CommandParam param, float minValue, float maxValue);
        //void ColorField(Color value);
        //void Vector2Field(Vector2 value);
        void Vector3Field(CommandParam param);
        //void Vector4Field(Vector4 value);
    }



}