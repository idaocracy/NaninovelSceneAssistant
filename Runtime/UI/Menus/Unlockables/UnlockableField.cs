using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public class UnlockableField : ScriptableUIControl<TMP_Dropdown>
    {
        protected UnlockableData Data;

        [SerializeField] private TextMeshProUGUI label;
        public override TMP_Dropdown UIComponent => GetComponentInChildren<TMP_Dropdown>();
        protected int DropdownIndex { get => UIComponent.value; set => UIComponent.value = value; }

        public virtual void Initialize(UnlockableData data)
        {
            Data = data;            
            label.text = data.Name;

            UIComponent.ClearOptions();
            UIComponent.AddOptions(new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("Unlocked"),  new TMP_Dropdown.OptionData("Locked") });

            DropdownIndex = Data.Value ? 0 : 1;
        }

        public void OnScroll(PointerEventData eventData)
        {
            var currentValue = DropdownIndex;
            var newValue = currentValue + (int)eventData.scrollDelta.y;
            DropdownIndex = newValue;
        }

        protected override void BindUIEvents() => UIComponent.onValueChanged.AddListener(SetValue);

        protected override void UnbindUIEvents() => UIComponent.onValueChanged.RemoveListener(SetValue);

        private void SetValue(int index) => Data.Value = DropdownIndex == 0 ? true : false;
    }
}
