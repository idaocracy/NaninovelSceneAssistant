using UnityEngine;
using UnityEngine.UI;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public class SceneAssistantWindowTab : ScriptableUIControl<Toggle>
    {
        private SceneAssistantUI sceneAssistantUI;
        [SerializeField] private SceneAssistantUI.SceneAssistantTab tab;

        private bool hasClicked;
        protected override void Awake() 
        {   
            base.Awake();
            sceneAssistantUI = GetComponentInParent<SceneAssistantUI>();
        }

        protected override void BindUIEvents()
        {
            UIComponent.onValueChanged.AddListener(ChangeValue);
        }

        protected override void UnbindUIEvents()
        {
            UIComponent.onValueChanged.RemoveListener(ChangeValue);
        }

        protected void ChangeValue(bool selected)
        {
            if(selected) 
            {
                if(hasClicked) return;
                sceneAssistantUI.ChangeTab(tab);
                hasClicked = true;
            }
            else
            {
                hasClicked = false;
            }

        }
    }
}

