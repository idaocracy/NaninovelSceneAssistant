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
			SceneAssistantUI.OnVisibilityChanged += GetRollbackValue;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			ScriptPlayer.OnPlay -= SetDefaultColor;
			ScriptPlayer.OnStop -= SetActiveColor;
			SceneAssistantUI.OnVisibilityChanged -= GetRollbackValue;
		}

		private void GetRollbackValue(bool visible) 
		{
			if(visible) defaultRollbackValue = InputManager.GetRollback().Enabled;
			else InputManager.GetRollback().Enabled = defaultRollbackValue;
		} 
		private void SetActiveColor(Script script) 
		{
			SetColor(true);
			if(SceneAssistantUI.Visible) InputManager.GetRollback().Enabled = false;
		} 
		private void SetDefaultColor(Script script) 
		{
			SetColor(false);
			if(SceneAssistantUI.Visible) InputManager.GetRollback().Enabled = defaultRollbackValue;
		} 

		protected override void OnButtonClick()
		{
			base.OnButtonClick();
			SyncAndExecuteAsync(ScriptPlayer.Stop);
			ScriptPlayer.SetWaitingForInputEnabled(false);
		}
	}
}
