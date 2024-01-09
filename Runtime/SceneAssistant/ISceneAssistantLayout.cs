using System;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;

namespace NaninovelSceneAssistant {
	public interface ISceneAssistantLayout
	{
		void StringField(ICommandParameterData<string> data, params ToggleGroupData[] toggleGroup);
		void StringDropdownField(ICommandParameterData<string> data, string[] stringValues, params ToggleGroupData[] toggleGroup);
		void EnumDropdownField(ICommandParameterData<Enum> data, params ToggleGroupData[] toggleGroup);
		void TypeDropdownField<T>(ICommandParameterData<T> data, Dictionary<string, T> values, params ToggleGroupData[] toggleGroup);
		void BoolField(ICommandParameterData<bool> data, params ToggleGroupData[] toggleGroup);
		void IntField(ICommandParameterData<int> data, int? min = null, int? max = null, params ToggleGroupData[] toggleGroup);
		void FloatField(ICommandParameterData<float> data, float? min = null, float? max = null, params ToggleGroupData[] toggleGroup);
		void FloatSliderField(ICommandParameterData<float> data, float min, float max, params ToggleGroupData[] toggleGroup);
		void IntSliderField(ICommandParameterData<int> data, int min, int max, params ToggleGroupData[] toggleGroup);
		void ColorField(ICommandParameterData<Color> data, bool includeAlpha = true, bool includeHDR = false, params ToggleGroupData[] toggleGroup);
		void Vector2Field(ICommandParameterData<Vector2> data, params ToggleGroupData[] toggleGroup);
		void Vector3Field(ICommandParameterData<Vector3> data, params ToggleGroupData[] toggleGroup);
		void Vector4Field(ICommandParameterData<Vector4> data, params ToggleGroupData[] toggleGroup);
		void PosField(ICommandParameterData<Vector3> data, CameraConfiguration cameraConfiguration, params	ToggleGroupData[] toggleGroup);
		void ListField(IListCommandParameterData list, params ToggleGroupData[] toggleGroup);
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
		void ScriptField(string scriptName, List<string> labels);
	}
}