using TMPro;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public class NumericVariableField : VariableField<float>, IVariableField
    {
        private ScrollableFloatValueField inputField => GetComponentInChildren<ScrollableFloatValueField>();

        public void InitializeNumericValueField(VariableData<float> data)
        {
            Data = data;
            Label.text = data.Name;

            inputField.Initialize(() => inputField.FloatValue = data.Value, v => data.Value = new CustomVariableValue(v).Number);

            var floatDropDown = inputField.GetComponentInChildren<TMP_Dropdown>();
            floatDropDown.onValueChanged.AddListener(ChangeFloatValueContentType);
            OnDestroyed = () => floatDropDown.onValueChanged.RemoveListener(ChangeFloatValueContentType);

            void ChangeFloatValueContentType(int value)
            {
                inputField.contentType = value == 0 ? TMP_InputField.ContentType.DecimalNumber : TMP_InputField.ContentType.IntegerNumber;
            }
        }
    }
}
