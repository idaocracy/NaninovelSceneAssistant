using UnityEngine;
using TMPro;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public class VariableField : ScriptableUIControl<TMP_InputField>
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TMP_InputField inputField;

        protected VariableData Data;

        public override TMP_InputField UIComponent => GetComponentInChildren<TMP_InputField>();

        protected string InputValue { get => UIComponent.text; set => UIComponent.text = value; }
        
        public virtual void Initialize(VariableData data)
        {
            Data = data;
            label.text = data.Name;
            inputField.text = data.Value;
        }

        protected override void BindUIEvents() => UIComponent.onSubmit.AddListener(SetVariableValue);
        protected override void UnbindUIEvents() => UIComponent.onSubmit.RemoveListener(SetVariableValue);

        private void SetVariableValue(string value) => Data.Value = InputValue;
    }
}
