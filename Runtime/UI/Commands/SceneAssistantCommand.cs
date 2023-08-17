using Naninovel;

namespace NaninovelSceneAssistant 
{
	[CommandAlias("sceneAssistant")]
	public class SceneAssistantCommand : Command
	{
		public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
		{
			var sceneAssistantUI = Engine.GetService<IUIManager>().GetUI<ISceneAssistantUI>();
			if(!sceneAssistantUI.Visible) sceneAssistantUI.Show();
			return UniTask.CompletedTask;
		}
	}
}
