using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Naninovel;

namespace NaninovelSceneAssistant
{
	public class VariableField : ScriptableUIBehaviour
	{
		[SerializeField] private Transform container;
		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private TMP_InputField stringValueFieldPrototype;
		[SerializeField] private Toggle boolValueFieldProtoype;
		[SerializeField] private ScrollableFloatValueField floatValueFieldPrototype;

		protected VariableData Data;
		private Action onDestroy;
		
		public virtual void Initialize(VariableData data)
		{
			Data = data;
			label.text = data.Name;
			
			if(float.TryParse(data.Value, out _)) InitializeFloatValueField(data);
            else if(bool.TryParse(data.Value, out _)) InitializeBoolValueField(data); 
            else InitializeStringValueField(data);
        }

        private void InitializeStringValueField(VariableData data)
        {
            var stringField = Instantiate(stringValueFieldPrototype, container);

            stringField.text = data.Value.ToString();
            stringField.onSubmit.AddListener(SetStringValue);
            onDestroy = () => stringField.onSubmit.RemoveListener(SetStringValue);
        }

        private void InitializeBoolValueField(VariableData data)
        {
            var boolField = Instantiate(boolValueFieldProtoype, container);

            boolField.isOn = bool.Parse(data.Value);
            boolField.onValueChanged.AddListener(SetBoolValue);
            onDestroy = () => boolField.onValueChanged.RemoveListener(SetBoolValue);
        }

        private void InitializeFloatValueField(VariableData data)
        {
            ScrollableFloatValueField floatField = Instantiate(floatValueFieldPrototype, container);
            floatField.Initialize(() => floatField.FloatValue = float.Parse(data.Value), v => data.Value = v.ToString());

            var floatDropDown = floatField.GetComponentInChildren<TMP_Dropdown>();
            floatDropDown.onValueChanged.AddListener(ChangeFloatValueContentType);
            onDestroy = () => floatDropDown.onValueChanged.RemoveListener(ChangeFloatValueContentType);

            void ChangeFloatValueContentType(int value)
            {
                floatField.contentType = value == 0 ? TMP_InputField.ContentType.DecimalNumber : TMP_InputField.ContentType.IntegerNumber;
            }
        }

        protected override void OnDestroy() 
		{
			base.OnDestroy();
			onDestroy();	
		}

		private void SetStringValue(string value) => Data.Value = value;
		private void SetBoolValue(bool value) => Data.Value = value.ToString();
	}
}
