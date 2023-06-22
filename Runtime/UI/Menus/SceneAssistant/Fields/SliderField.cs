using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NaninovelSceneAssistant
{
	public class SliderField : SceneAssistantDataField<Slider, float>
	{
		protected float SliderValue { get => ValueComponent.value; set { ValueComponent.value = value; inputField.text = value.ToString("0.###"); } }
		protected override UnityEvent<float> Event => ValueComponent.onValueChanged;
		[SerializeField] private TMP_InputField inputField;

		public virtual void Initialize(ICommandParameterData data, float min, float max, params ICommandParameterData[] toggleGroup)
		{
			InitializeBaseData(data, toggleGroup);

			ValueComponent.minValue = min;
			ValueComponent.maxValue = max;

			if (data is ICommandParameterData<float> floatData)
			{
				inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
				getDataValue = () => SliderValue = floatData.Value;
				setDataValue = SliderValue => floatData.Value = SliderValue;
				ValueComponent.wholeNumbers = false;
			}

			if (data is ICommandParameterData<int> intData)
			{
				inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
				getDataValue = () => SliderValue = intData.Value;
				setDataValue = SliderValue => intData.Value = (int)SliderValue;
				ValueComponent.wholeNumbers = true;
			}
			GetDataValue();
		}

		protected override void BindUIEvents()
		{
			base.BindUIEvents();
			inputField.onSubmit.AddListener(SetSliderValue);
		}

		protected override void UnbindUIEvents()
		{
			base.UnbindUIEvents();
			inputField.onSubmit.RemoveListener(SetSliderValue);
		}

		private void SetSliderValue(string value) 
		{
			var floatValue = float.Parse(value);
			SliderValue = floatValue;
			// needed for clamping the value 
			inputField.text = SliderValue.ToString();
		} 
	}
}
