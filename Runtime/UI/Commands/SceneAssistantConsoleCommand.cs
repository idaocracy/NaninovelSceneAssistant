using Naninovel;

namespace NaninovelSceneAssistant
{
	public static class SceneAssistantConsoleCommand
	{
		[ConsoleCommand("scn")]
		public static void ShowSceneAssistantUI() 
		{
			var sceneAssistantUI = Engine.GetService<IUIManager>().GetUI<ISceneAssistantUI>();
			if(!sceneAssistantUI.Visible) sceneAssistantUI.Show();
		} 
	}
}


