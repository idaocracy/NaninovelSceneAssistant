namespace NaninovelSceneAssistant
{
    public class SceneAssistantPauseButton : SceneAssistantScriptPlayerButton
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            ScriptPlayer.OnWaitingForInput += HandleWaitingForInput;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ScriptPlayer.OnWaitingForInput -= HandleWaitingForInput;
        }

        private void HandleWaitingForInput(bool enabled) => HandleModeChange(enabled);

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            SyncAndExecuteAsync(() => ScriptPlayer.SetWaitingForInputEnabled(true));
        }
    }
}