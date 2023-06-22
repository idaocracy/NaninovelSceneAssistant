using Naninovel;

namespace NaninovelSceneAssistant
{
	public class SceneAssistantStopButton : SceneAssistantScriptPlayerButton
	{
		private IInputManager inputManager;

		protected override void Awake()
		{
			base.Awake();
			inputManager = Engine.GetService<IInputManager>();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			ScriptPlayer.OnPlay += HandlePlayModeChange;
			ScriptPlayer.OnStop += HandlePlayModeChange;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			ScriptPlayer.OnPlay -= HandlePlayModeChange;
			ScriptPlayer.OnStop -= HandlePlayModeChange;
		}

		private void HandlePlayModeChange(Script script) 
		{
			inputManager.GetRollback().Enabled = ScriptPlayer.Playing ? true : false;
			HandleModeChange(ScriptPlayer.Playing ? false : true);
		} 

		protected override void OnButtonClick()
		{
			base.OnButtonClick();
			SyncAndExecuteAsync(ScriptPlayer.Stop);
			if (ScriptPlayer.WaitingForInput) ScriptPlayer.SetWaitingForInputEnabled(false);
		}


	}

}
