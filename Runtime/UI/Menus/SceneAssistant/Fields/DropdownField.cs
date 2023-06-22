using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

namespace NaninovelSceneAssistant
{
	public class DropdownField : SceneAssistantDataField<TMP_Dropdown, int>
	{
		protected int DropdownIndex { get => ValueComponent.value; set => ValueComponent.value = value; }
		protected List<TMP_Dropdown.OptionData> DropdownOptions => ValueComponent.options;
		protected override UnityEvent<int> Event => ValueComponent?.onValueChanged;

		public virtual void Initialize<T>(ICommandParameterData<T> data, string[] stringValues = null, T[] typeValues = null,  params ICommandParameterData[] toggleGroup)
		{
			InitializeBaseData(data, toggleGroup);
			ValueComponent.ClearOptions();

			if (data is ICommandParameterData<string> stringData) SetupStringData(stringData, stringValues);
			else if(data is ICommandParameterData<Enum> enumData) SetupEnumData(enumData);
			else if(data is ICommandParameterData<T> typeData) SetupTypeList(typeData, stringValues, typeValues);

			GetDataValue();
		}

		private void SetupStringData(ICommandParameterData<string> stringData, string[] stringValues)
		{
			ValueComponent.AddOptions(stringValues.Select(v => new TMP_Dropdown.OptionData(v)).ToList());
			
			getDataValue = () => DropdownIndex = Array.IndexOf(stringValues, stringData.Value);
 			setDataValue = DropdownIndex => stringData.Value = DropdownOptions[DropdownIndex].text; 
		}
		
		private void SetupEnumData<Enum>(ICommandParameterData<Enum> enumData)
		{
			var enumType = enumData.Value.GetType();
			var stringValues = enumType.GetEnumNames();
			var enumValues = enumType.GetEnumValues();
			ValueComponent.AddOptions(stringValues.Select(e => e).ToList());
			
			getDataValue = () => DropdownIndex = Array.IndexOf(enumValues, enumData.Value);
 			setDataValue = DropdownIndex => enumData.Value = (Enum)enumValues.GetValue(DropdownIndex);
		}
		
		private void SetupTypeList<T>(ICommandParameterData<T> typeData, string[] stringValues, T[] typeValues)
		{
			ValueComponent.AddOptions(stringValues.Select(v => new TMP_Dropdown.OptionData(v)).ToList());
			int? nullIndex = Array.IndexOf(stringValues, "None");
			
			getDataValue = () => 
			{
				var dataIndex = Array.IndexOf(typeValues, typeData.Value);
				if (dataIndex == -1) DropdownIndex = (int)nullIndex;
				else DropdownIndex = dataIndex;
			}; 
			
 			setDataValue = DropdownIndex => typeData.Value = typeValues[DropdownIndex];
		}
	}
}
