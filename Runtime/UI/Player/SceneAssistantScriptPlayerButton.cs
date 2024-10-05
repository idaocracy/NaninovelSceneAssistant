using System;
using Naninovel;
using UnityEngine;
using UnityEngine.UI;

namespace NaninovelSceneAssistant
{
	public class SceneAssistantScriptPlayerButton : ScriptableButton
	{
		protected IScriptPlayer ScriptPlayer;
		protected IInputManager InputManager;
		protected SceneAssistantManager SceneAssistantManager;
		protected SceneAssistantUI SceneAssistantUI;
		
		[SerializeField] private Color activeColor;
		[SerializeField] private Image icon;
		private Color defaultColor;
		
		protected override void Awake()
		{
			base.Awake();
			ScriptPlayer = Engine.GetService<IScriptPlayer>();
			InputManager = Engine.GetService<IInputManager>();
			SceneAssistantManager = Engine.GetService<SceneAssistantManager>();
			SceneAssistantUI = GetComponentInParent<SceneAssistantUI>();
			defaultColor = icon.color; 
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

		protected void SetColor(bool enabled)
		{
			icon.color = enabled ? activeColor : defaultColor;
		}
	}
}