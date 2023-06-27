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
			if (!stateManager.Configuration.EnableStateRollback) Interactable = false;
		}

		protected override void OnButtonClick()
		{
			base.OnButtonClick();
			InputManager.GetRollback().Activate(1);
			SyncAndExecuteAsync(() => ScriptPlayer.SetWaitingForInputEnabled(true));
		}
	}
}

