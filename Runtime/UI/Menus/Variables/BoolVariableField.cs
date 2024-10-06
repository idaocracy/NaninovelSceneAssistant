using UnityEngine.UI;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public class BoolVariableField : VariableField<bool>, IVariableField
    {
        private Toggle toggle => GetComponentInChildren<Toggle>();

        public void InitializeBoolValueField(VariableData<bool> data)
        {
            Data = data;
            Label.text = data.Name;

            toggle.isOn = data.Value;
            toggle.onValueChanged.AddListener(SetBoolValue);
            OnDestroyed = () => toggle.onValueChanged.RemoveListener(SetBoolValue);
        }

        private void SetBoolValue(bool value) => Data.Value = value;
    }
}
