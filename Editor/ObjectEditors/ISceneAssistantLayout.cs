using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using UnityEditor;

namespace NaninovelSceneAssistant {

    public interface ISceneAssistantLayout 
    {
        void StringField(string value);
        void StringListField(string value);
        void BoolField(bool value);
        void IntField(int value);
        void FloatField(float value);
        void OptionlessFloatField(float value);
        void SliderField(float value);
        void ColorField(Color value);
        void Vector2Field(Vector2 value);
        void Vector3Field(Vector3 value);
        void Vector4Field(Vector4 value);
    }

    public class EditorLayout : ISceneAssistantLayout
    {
        public void BoolField(bool value)
        {
            value = EditorGUILayout
        }

        public void ColorField(Color value)
        {
            throw new System.NotImplementedException();
        }

        public void FloatField(float value)
        {
            throw new System.NotImplementedException();
        }

        public void IntField(int value)
        {
            throw new System.NotImplementedException();
        }

        public void OptionlessFloatField(float value)
        {
            throw new System.NotImplementedException();
        }

        public void SliderField(float value)
        {
            throw new System.NotImplementedException();
        }

        public void StringField(string value)
        {
            throw new System.NotImplementedException();
        }

        public void StringListField(string value)
        {
            throw new System.NotImplementedException();
        }

        public void Vector2Field(Vector2 value)
        {
            throw new System.NotImplementedException();
        }

        public void Vector3Field(Vector3 value)
        {
            throw new System.NotImplementedException();
        }

        public void Vector4Field(Vector4 value)
        {
            throw new System.NotImplementedException();
        }
    }



}