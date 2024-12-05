using TMPro;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public class StringVariableField : VariableField<string>, IVariableField
    {
        private TMP_InputField inputField => GetComponentInChildren<TMP_InputField>();

        public void InitializeStringValueField(VariableData<string> data)
        {
            Data = data;
            Label.text = data.Name;

            inputField.text = data.Value.ToString();
            inputField.onSubmit.AddListener(SetStringValue);
            OnDestroyed = () => inputField.onSubmit.RemoveListener(SetStringValue);
        }

        private void SetStringValue(string value) => Data.Value = value;
    }
}
