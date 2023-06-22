using Naninovel;
using TMPro;

namespace NaninovelSceneAssistant
{
    public abstract class SceneAssistantWindowMenu : ScriptableUIBehaviour
    {
        protected SceneAssistantManager Manager;
        protected SceneAssistantUI UI;

        protected virtual TMP_InputField SearchField {get; }

        protected override void Awake() 
        {
            Manager = Engine.GetService<SceneAssistantManager>();
            UI = GetComponentInParent<SceneAssistantUI>();
        }

        public virtual void InitializeMenu()
        {
            ResetMenu();
            Manager.OnSceneAssistantCleared += ClearMenu;
            Manager.OnSceneAssistantReset += ResetMenu;

            SearchField?.onValueChanged.AddListener(EvaluateSearch);
        }

        protected override void OnDisable() 
        {
            base.OnDisable();
            DestroyMenu();
        }

        public virtual void DestroyMenu()
        {
            ClearMenu();
            Manager.OnSceneAssistantCleared -= ClearMenu;
            Manager.OnSceneAssistantReset -= ResetMenu;

            SearchField?.onValueChanged.RemoveListener(EvaluateSearch);
        }

        protected override void OnDestroy() {
            DestroyMenu();
        }

        protected abstract void ClearMenu();
        protected abstract void ResetMenu();
        protected virtual void EvaluateSearch(string value) {}
    }

}