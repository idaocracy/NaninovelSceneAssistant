using UnityEngine;
using Naninovel;
using Naninovel.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NaninovelSceneAssistant
{
	public interface ISceneAssistantUI : IManagedUI
	{
		void Show();
	}
	
	public class SceneAssistantUI : CustomUI, ISceneAssistantUI, IPointerDownHandler
	{
		private SceneAssistantManager sceneAssistantManager;
		public enum SceneAssistantTab { SceneAssistant, Variables, Scripts };
		public SceneAssistantWindowMenu CurrentMenu { get; private set; }

		[Header("Toolbar")]
		[SerializeField] private Toggle rollbackToggle;
		[SerializeField] private ScriptableLabeledButton closeButton;
		[SerializeField] private Image background;

		[Header("Menus")]
		[SerializeField] private SceneAssistantMenu sceneAssistantMenu;
		[SerializeField] private VariablesMenu variablesMenu;
		[SerializeField] private ScriptsMenu scriptsMenu;

		public Texture2D CursorTexture;
		
		private IInputManager inputManager;
		private bool defaultRollbackValue;

		protected override void Awake()
		{
			base.Awake();
			sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
			inputManager = Engine.GetService<IInputManager>();
			CurrentMenu = sceneAssistantMenu;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			closeButton.OnButtonClicked += Hide;
			rollbackToggle.onValueChanged.AddListener(SetRollbackEnabled);
		}

		protected override void OnDisable()
		{
			base.OnEnable();
			closeButton.OnButtonClicked -= Hide;
			rollbackToggle.onValueChanged.RemoveListener(SetRollbackEnabled);
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
		
		public void OnPointerDown(PointerEventData eventData)
		{
			sceneAssistantMenu.DestroyColorPicker();
		}
		
		public override void Show()
		{
			sceneAssistantManager.InitializeSceneAssistant();
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
			sceneAssistantUI.Show();
		} 
	}
	
		[CommandAlias("sceneAssistant")]
		public class SceneAssistantCommand : Command
		{
			public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
			{
				var sceneAssistantUI = Engine.GetService<IUIManager>().GetUI<ISceneAssistantUI>();
				sceneAssistantUI.Show();
				return UniTask.CompletedTask;
			}
		}
}