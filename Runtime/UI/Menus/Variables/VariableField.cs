using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Naninovel;

namespace NaninovelSceneAssistant
{
	public class VariableField : ScriptableUIBehaviour
	{
		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private TMP_InputField stringField;
		[SerializeField] private Toggle boolField;
		[SerializeField] private ScrollableFloatValueField floatField;

		protected VariableData Data;
		
		public virtual void Initialize(VariableData data)
		{
			Data = data;
			label.text = data.Name;
			
			if(float.TryParse(data.Value, out var floatValue))
			{
				floatField.gameObject.SetActive(true);
				floatField.text = floatValue.ToString();
				floatField.onSubmit.AddListener(SetStringValue);
			}
			
			else if(bool.TryParse(data.Value, out var boolValue))
			{
				boolField.gameObject.SetActive(true);
				boolField.isOn = boolValue;
				boolField.onValueChanged.AddListener(SetBoolValue);
			}
			else
			{
				stringField.gameObject.SetActive(true);
				stringField.text = data.Value.ToString();
				stringField.onSubmit.AddListener(SetStringValue);
			}
		}

		private void SetStringValue(string value) => Data.Value = value;
		private void SetBoolValue(bool value) => Data.Value = value.ToString();
	}
}
