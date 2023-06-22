using Naninovel;

namespace NaninovelSceneAssistant
{
    public class SceneAssistantSkipButton : SceneAssistantScriptPlayerButton
    {
        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            InputManager.GetContinue().Activate(1);
            SyncAndExecuteAsync(() => ScriptPlayer.SetWaitingForInputEnabled(true));
        }
    }
}

