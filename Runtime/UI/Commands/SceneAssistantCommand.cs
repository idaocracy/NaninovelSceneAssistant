using Naninovel;

namespace NaninovelSceneAssistant 
{
	[Alias("sceneAssistant")]
	public class SceneAssistantCommand : Command
	{
		public override UniTask Execute (ExecutionContext ext)
		{
			var sceneAssistantUI = Engine.GetService<IUIManager>().GetUI<ISceneAssistantUI>();
			if(!sceneAssistantUI.Visible) sceneAssistantUI.Show();
			return UniTask.CompletedTask;
		}
	}
}