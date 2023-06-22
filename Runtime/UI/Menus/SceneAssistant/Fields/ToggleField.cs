using UnityEngine.Events;
using UnityEngine.UI;

namespace NaninovelSceneAssistant
{
    public class ToggleField : SceneAssistantDataField<Toggle, bool>
    {
        protected bool ToggleValue { get => ValueComponent.isOn; set => ValueComponent.isOn = value; }
        protected override UnityEvent<bool> Event => ValueComponent?.onValueChanged;

        public virtual void Initialize(ICommandParameterData data, params ICommandParameterData[] toggleGroup)
        {
            InitializeBaseData(data, toggleGroup);

            if (Data is ICommandParameterData<bool> boolData)
            {
                getDataValue = () => ValueComponent.isOn = boolData.Value;
                setDataValue = v => boolData.Value = v;
            }

            GetDataValue();
        }

    }
}
