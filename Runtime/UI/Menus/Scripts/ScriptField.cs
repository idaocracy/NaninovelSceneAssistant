using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using Naninovel.UI;

namespace NaninovelSceneAssistant
{
	public class ScriptField : ScriptableUIControl<Button>
	{
		protected string ScriptName;
		[SerializeField] private TextMeshProUGUI label;
		public override Button UIComponent => GetComponentInChildren<Button>();
		
		private IStateManager stateManager;
		private IScriptPlayer scriptPlayer;

		public virtual void Initialize(string scriptName)
		{
			this.ScriptName = scriptName;
			label.text = scriptName;            
			stateManager = Engine.GetService<IStateManager>();
			scriptPlayer = Engine.GetService<IScriptPlayer>();
		}

		protected override void BindUIEvents() => UIComponent.onClick.AddListener(PlayScript);

		protected override void UnbindUIEvents() => UIComponent.onClick.RemoveListener(PlayScript);

		private void PlayScript() => PlayScriptAsync();
		
		private async void PlayScriptAsync()
		{
			Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
			await stateManager.ResetState(() => scriptPlayer.LoadAndPlay(ScriptName));
		}
	}
}
