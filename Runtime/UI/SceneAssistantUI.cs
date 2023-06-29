using UnityEngine;
using Naninovel;
using Naninovel.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace NaninovelSceneAssistant
{
	public interface ISceneAssistantUI : IManagedUI
	{
		void Show();
	}
	
	public class SceneAssistantUI : CustomUI, ISceneAssistantUI
	{
		private SceneAssistantManager sceneAssistantManager;
		public enum SceneAssistantTab { SceneAssistant, Variables, Scripts }
		public SceneAssistantWindowMenu CurrentMenu { get; private set; }

		[Header("Main elements")]
		[SerializeField] private Toggle rollbackToggle;
		[SerializeField] private ScriptableLabeledButton closeButton;
		
		[SerializeField] private TextMeshProUGUI scriptPlayerInfoBox;

		[Header("Menus")]
		[SerializeField] private SceneAssistantMenu sceneAssistantMenu;
		[SerializeField] private VariablesMenu variablesMenu;
		[SerializeField] private ScriptsMenu scriptsMenu;

		public Texture2D CursorTexture;
		
		private IInputManager inputManager;
		private IScriptPlayer scriptPlayer;
		private IStateManager stateManager;
		private bool defaultRollbackValue;

		protected override void Awake()
		{
			base.Awake();
			sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
			inputManager = Engine.GetService<IInputManager>();
			scriptPlayer = Engine.GetService<IScriptPlayer>();
			stateManager = Engine.GetService<IStateManager>();
			CurrentMenu = sceneAssistantMenu;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			closeButton.OnButtonClicked += Hide;
			rollbackToggle.onValueChanged.AddListener(SetRollbackEnabled);
			sceneAssistantManager.OnSceneAssistantReset += CheckIndex;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			closeButton.OnButtonClicked -= Hide;
			rollbackToggle.onValueChanged.RemoveListener(SetRollbackEnabled);
		}

		protected void CheckIndex()
		{	
			if(scriptPlayer.Playing) 
			{
				if(!scriptPlayer.Playlist.IsIndexValid(scriptPlayer.PlayedIndex+1))
				{
					scriptPlayerInfoBox.text = "End of script";
				}
				else if(!stateManager.CanRollbackTo(s => s.PlayerRollbackAllowed))
				{
					scriptPlayerInfoBox.text = "Rollback stack is empty";
				}
				else
				{
					scriptPlayerInfoBox.text = null;
				}
			}
		}

		protected override void HandleVisibilityChanged(bool visible)
		{
			base.HandleVisibilityChanged(visible);

			if (visible)
			{
				defaultRollbackValue = inputManager.GetRollback().Enabled;
			}
			else
			{
				if(sceneAssistantManager != null && sceneAssistantManager.Initialized)
				{
					CurrentMenu.DestroyMenu();
					sceneAssistantManager.DestroySceneAssistant();
					inputManager.GetRollback().Enabled = defaultRollbackValue;
				}
			}
		}

		private void SetRollbackEnabled(bool toggle)
		{
			if(toggle) inputManager.GetRollback().Enabled = false;
			else inputManager.GetRollback().Enabled = defaultRollbackValue;
		}

		public void ChangeTab(SceneAssistantTab sceneAssistantTab)
		{
			switch(sceneAssistantTab)
			{
				case SceneAssistantTab.SceneAssistant:
					sceneAssistantMenu.gameObject.SetActive(true);
					variablesMenu.gameObject.SetActive(false);
					scriptsMenu.gameObject.SetActive(false);

					CurrentMenu = sceneAssistantMenu;
				break;

				case SceneAssistantTab.Variables:
					sceneAssistantMenu.gameObject.SetActive(false);
					variablesMenu.gameObject.SetActive(true);
					scriptsMenu.gameObject.SetActive(false);

					CurrentMenu = variablesMenu;
				break;

				case SceneAssistantTab.Scripts:
					sceneAssistantMenu.gameObject.SetActive(false);
					variablesMenu.gameObject.SetActive(false);
					scriptsMenu.gameObject.SetActive(true);
					
					CurrentMenu = scriptsMenu;
				break;
			}

			if(sceneAssistantManager.IsAvailable) CurrentMenu.InitializeMenu();
		} 
		
		public override void Show()
		{
			if(!sceneAssistantManager.Initialized) sceneAssistantManager.InitializeSceneAssistant();
			CurrentMenu.InitializeMenu();
			base.Show();
		}
	}
	
	public static class SceneAssistantConsoleCommand
	{
		[ConsoleCommand("scn")]
		public static void ShowSceneAssistantUI() 
		{
			var sceneAssistantUI = Engine.GetService<IUIManager>().GetUI<ISceneAssistantUI>();
			if(!sceneAssistantUI.Visible) sceneAssistantUI.Show();
		} 
	}
	
	[CommandAlias("sceneAssistant")]
	public class SceneAssistantCommand : Command
	{
		public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
		{
			var sceneAssistantUI = Engine.GetService<IUIManager>().GetUI<ISceneAssistantUI>();
			if(!sceneAssistantUI.Visible) sceneAssistantUI.Show();
			return UniTask.CompletedTask;
		}
	}
}