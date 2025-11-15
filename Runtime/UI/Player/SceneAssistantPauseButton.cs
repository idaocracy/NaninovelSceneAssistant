using Naninovel;

namespace NaninovelSceneAssistant
{
    public class SceneAssistantPauseButton : SceneAssistantScriptPlayerButton
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            ScriptPlayer.OnAwaitInput += HandleWaitingForInput;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ScriptPlayer.OnAwaitInput -= HandleWaitingForInput;
        }

        private void HandleWaitingForInput(IScriptTrack track) => SetColor(track.AwaitingInput);

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            SyncAndExecuteAsync(() => ScriptPlayer.MainTrack.SetAwaitInput(true));
        }
    }
}