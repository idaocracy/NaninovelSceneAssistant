using TMPro;
using UnityEngine.Events;

namespace NaninovelSceneAssistant
{
	public class InputField : SceneAssistantDataField<TMP_InputField, string>
	{
		protected override UnityEvent<string> Event => ValueComponent.onSubmit;
		protected string InputValue { get => ValueComponent.text; set => ValueComponent.text = value; }
		public virtual void Initialize(ICommandParameterData data, params ICommandParameterData[] toggleGroup)
		{
			InitializeBaseData(data, toggleGroup);

			if (data is ICommandParameterData<string> stringData)
			{
				getDataValue = () => InputValue = stringData.Value;
				setDataValue = InputValue => stringData.Value = InputValue;
			}

			GetDataValue();
		}
	}
}
