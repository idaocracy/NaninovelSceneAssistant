using Naninovel;

namespace NaninovelSceneAssistant
{
	public class SceneAssistantStopButton : SceneAssistantScriptPlayerButton
	{
		private bool defaultRollbackValue;

		protected override void OnEnable()
		{
			base.OnEnable();
			ScriptPlayer.OnPlay += SetDefaultColor;
			ScriptPlayer.OnStop += SetActiveColor;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			ScriptPlayer.OnPlay -= SetDefaultColor;
			ScriptPlayer.OnStop -= SetActiveColor;
		}

		private void SetActiveColor(Script script) 
		{
			SetColor(true);
		} 
		private void SetDefaultColor(Script script) 
		{
			SetColor(false);
		} 

		protected override void OnButtonClick()
		{
			base.OnButtonClick();
			SyncAndExecuteAsync(ScriptPlayer.Stop);
			ScriptPlayer.SetWaitingForInputEnabled(false);
		}
	}
}
