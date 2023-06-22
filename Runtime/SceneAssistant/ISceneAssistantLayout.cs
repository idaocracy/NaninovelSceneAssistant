using System;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;

namespace NaninovelSceneAssistant {
	public interface ISceneAssistantLayout
	{
		void StringField(ICommandParameterData<string> data, params ICommandParameterData[] toggleGroup);
		void StringDropdownField(ICommandParameterData<string> data, string[] stringValues, params ICommandParameterData[] toggleGroup);
		void EnumDropdownField(ICommandParameterData<Enum> data, params ICommandParameterData[] toggleGroup);
		void TypeDropdownField<T>(ICommandParameterData<T> data, Dictionary<string, T> values, params ICommandParameterData[] toggleGroup);
		void BoolField(ICommandParameterData<bool> data, params ICommandParameterData[] toggleGroup);
		void IntField(ICommandParameterData<int> data, int? min = null, int? max = null, params ICommandParameterData[] toggleGroup);
		void FloatField(ICommandParameterData<float> data, float? min = null, float? max = null, params ICommandParameterData[] toggleGroup);
		void FloatSliderField(ICommandParameterData<float> data, float min, float max, params ICommandParameterData[] toggleGroup);
		void IntSliderField(ICommandParameterData<int> data, int min, int max, params ICommandParameterData[] toggleGroup);
		void ColorField(ICommandParameterData<Color> data, bool includeAlpha = true, bool includeHDR = false, params ICommandParameterData[] toggleGroup);
		void Vector2Field(ICommandParameterData<Vector2> data, params ICommandParameterData[] toggleGroup);
		void Vector3Field(ICommandParameterData<Vector3> data, params ICommandParameterData[] toggleGroup);
		void Vector4Field(ICommandParameterData<Vector4> data, params ICommandParameterData[] toggleGroup);
		void PosField(ICommandParameterData<Vector3> data, CameraConfiguration cameraConfiguration, params ICommandParameterData[] toggleGroup);
		void ListField(IListCommandParameterData list, params ICommandParameterData[] toggleGroup);
	}

	public interface ICustomVariableLayout 
	{
		void VariableField(VariableData variable);
	}

	public interface IUnlockableLayout 
	{
		void UnlockableField(UnlockableData unlockable);
	}
	
	public interface IScriptLayout
	{
		void ScriptField(string scriptName);
	}
}