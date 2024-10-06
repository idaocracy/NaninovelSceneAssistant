using Naninovel;

namespace NaninovelSceneAssistant 
{
	[CommandAlias("sceneAssistant")]
	public class SceneAssistantCommand : Command
	{
		public override UniTask Execute (AsyncToken asyncToken = default)
		{
			var sceneAssistantUI = Engine.GetService<IUIManager>().GetUI<ISceneAssistantUI>();
			if(!sceneAssistantUI.Visible) sceneAssistantUI.Show();
			return UniTask.CompletedTask;
		}
	}
}