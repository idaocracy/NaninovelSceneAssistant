using System;
using Naninovel;
using UnityEngine;
using UnityEngine.UI;

namespace NaninovelSceneAssistant
{
	public class SceneAssistantPlayButton : SceneAssistantScriptPlayerButton
	{
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

		protected override void OnButtonClick()
		{
			base.OnButtonClick();
			ScriptPlayer.Play();
			InputManager.GetContinue().Activate(1);
		}

		private void HandlePlayModeChange(Script script) => HandleModeChange(ScriptPlayer.Playing ? true : false);
	}
}
