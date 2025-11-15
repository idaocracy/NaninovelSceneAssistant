using System;
using System.Reflection;
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

			if (!ScriptPlayer.MainTrack.Playing)
			{
				Type type = typeof(IScriptPlayer);
				MethodInfo methodInfo = type.GetMethod("Resume");
				if (methodInfo != null) methodInfo.Invoke(ScriptPlayer, new object[] { null });
				else type.GetMethod("Play").Invoke(ScriptPlayer, new object[] { null });
			}
			InputManager.GetContinue().Activate(1);
		}

		private void SetActiveColor(IScriptTrack script) => SetColor(true);
		
		private void SetDefaultColor(IScriptTrack script) => SetColor(false);
	}
}
