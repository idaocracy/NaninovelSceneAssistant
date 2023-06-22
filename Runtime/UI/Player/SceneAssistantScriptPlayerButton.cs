using System;
using Naninovel;

namespace NaninovelSceneAssistant
{
	public class SceneAssistantScriptPlayerButton : ScriptableButton
	{
		protected IScriptPlayer ScriptPlayer;
		protected IInputManager InputManager;
		protected SceneAssistantManager SceneAssistantManager;
		
		protected override void Awake()
		{
			base.Awake();
			ScriptPlayer = Engine.GetService<IScriptPlayer>();
			InputManager = Engine.GetService<IInputManager>();
			SceneAssistantManager = Engine.GetService<SceneAssistantManager>();
		}
		protected async void SyncAndExecuteAsync(Action action)
		{
			await ScriptPlayer.SynchronizeAndDoAsync(() => UniTaskify(action));

			UniTask UniTaskify(Action task)
			{
				task();
				return UniTask.CompletedTask;
			}
		}

		protected void HandleModeChange(bool enabled)
		{
			UIComponent.targetGraphic.color = enabled ? UIComponent.colors.selectedColor : UIComponent.colors.normalColor;
		}
	}
}
	
