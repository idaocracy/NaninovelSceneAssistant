using Naninovel;

namespace NaninovelSceneAssistant
{
    public class SceneAssistantRewindButton : SceneAssistantScriptPlayerButton
    {
        private IStateManager stateManager;

        protected override void Awake()
        {
            base.Awake();
            stateManager = Engine.GetService<IStateManager>();
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            if (stateManager.Configuration.EnableStateRollback)
            {
                InputManager.GetRollback().Activate(1);
                SyncAndExecuteAsync(() => ScriptPlayer.SetWaitingForInputEnabled(true));
            }
        }
    }
}

