using System;
using UnityEngine.UI;
using Naninovel;
using TMPro;
using System.Collections.Generic;

namespace NaninovelSceneAssistant
{
    public class ObjectTypeToggle : ScriptableUIControl<Toggle>
    {
        private TextMeshProUGUI label => GetComponentInChildren<TextMeshProUGUI>();
        private SceneAssistantManager sceneAssistantManager;
        private KeyValuePair<Type, bool> kv;

        public void Initialize(KeyValuePair<Type, bool> kv)
        {
            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();

            this.kv = kv;
            this.label.text = kv.Key.GetProperty("TypeId").GetValue(null).ToString() ?? kv.Key.Name;
            UIComponent.isOn = kv.Value;
        }

        private void OnValueChanged(bool value) => sceneAssistantManager.ObjectTypeList[kv.Key] = value;

        protected override void BindUIEvents() => UIComponent.onValueChanged.AddListener(OnValueChanged);

        protected override void UnbindUIEvents() => UIComponent.onValueChanged.RemoveListener(OnValueChanged);
    }
}
