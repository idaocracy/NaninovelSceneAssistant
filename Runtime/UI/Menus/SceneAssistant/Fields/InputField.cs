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
			
			ValueComponent.onSelect.AddListener(DisableInputProcessing);
			ValueComponent.onDeselect.AddListener(EnableInputProcessing);
		}

		protected override void OnDestroy() 
		{
			base.OnDestroy();
			ValueComponent.onSelect.RemoveListener(DisableInputProcessing);
			ValueComponent.onDeselect.RemoveListener(EnableInputProcessing);
		}
		
		private void DisableInputProcessing(string value) => SceneAssistantUI.ToggleInputProcessing(false);
		private void EnableInputProcessing(string value) => SceneAssistantUI.ToggleInputProcessing(true);
	}
}
