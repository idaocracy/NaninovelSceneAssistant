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
			ScriptPlayer.OnPlay += SetActiveColor;
			ScriptPlayer.OnStop += SetDefaultColor;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			ScriptPlayer.OnPlay -= SetActiveColor;
			ScriptPlayer.OnStop -= SetDefaultColor;
		}

		protected override void OnButtonClick()
		{
			base.OnButtonClick();
			ScriptPlayer.Play();
			InputManager.GetContinue().Activate(1);
		}

		private void SetActiveColor(Script script) => SetColor(true);
		
		private void SetDefaultColor(Script script) => SetColor(false);
	}
}
