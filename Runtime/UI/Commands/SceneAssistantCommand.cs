using Naninovel;
using UnityEngine;

namespace NaninovelSceneAssistant 
{
	[Alias("sceneAssistant")]
	public class SceneAssistantCommand : Command
	{
		public override Awaitable Execute (ExecutionContext ctx)
		{
			var sceneAssistantUI = Engine.GetService<IUIManager>().GetUI<ISceneAssistantUI>();
			if(!sceneAssistantUI.Visible) sceneAssistantUI.Show();
			return Async.Completed;
		}
	}
}