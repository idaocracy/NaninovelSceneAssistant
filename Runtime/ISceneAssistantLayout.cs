using System;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;

namespace NaninovelSceneAssistant {

    public interface ISceneAssistantLayout
    {
        void StringField(ICommandParameterData<string> data, ICommandParameterData toggleWith = null);
        void StringListField(ICommandParameterData<string> data, string[] stringValues, ICommandParameterData toggleWith = null);
        void TypeListField<T>(ICommandParameterData<T> data, Dictionary<string, T> values, ICommandParameterData toggleWith = null);
        void BoolField(ICommandParameterData<bool> data, ICommandParameterData toggleWith = null);
        void IntField(ICommandParameterData<int> data, int? min, int? max, ICommandParameterData toggleWith = null);
        void FloatField(ICommandParameterData<float> data, float? min = null, float? max = null, ICommandParameterData toggleWith = null);
        void FloatSliderField(ICommandParameterData<float> data, float min, float max, ICommandParameterData toggleWith = null);
        void IntSliderField(ICommandParameterData<int> data, int min, int max, ICommandParameterData toggleWith = null);
        void ColorField(ICommandParameterData<Color> data, bool includeAlpha = true, bool includeHDR = false, ICommandParameterData toggleWith = null);
        void EnumField(ICommandParameterData<Enum> data, ICommandParameterData toggleWith = null);
        void Vector2Field(ICommandParameterData<Vector2> data, ICommandParameterData toggleWith = null);
        void Vector3Field(ICommandParameterData<Vector3> data, ICommandParameterData toggleWith = null);
        void Vector4Field(ICommandParameterData<Vector4> data, ICommandParameterData toggleWith = null);
        void PosField(ICommandParameterData<Vector3> data, CameraConfiguration cameraConfiguration, ICommandParameterData toggleWith = null);
        void ListButtonField(IListCommandParameterData list, ICommandParameterData toggleWith = null);
        void VariableField(VariableData variable);
        void UnlockableField(UnlockableData unlockable);
    }



}