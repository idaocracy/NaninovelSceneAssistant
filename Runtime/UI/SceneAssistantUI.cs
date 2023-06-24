using UnityEngine;
using Naninovel;
using Naninovel.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NaninovelSceneAssistant
{
	public class SceneAssistantUI : CustomUI, IPointerDownHandler
	{
		private SceneAssistantManager sceneAssistantManager;
		public enum SceneAssistantTab { SceneAssistant, Variables, Scripts };
		public SceneAssistantWindowMenu CurrentMenu { get; private set; }

		[Header("Toolbar")]
		[SerializeField] private ScriptableLabeledButton closeButton;
		[SerializeField] private Image background;

		[Header("Menus")]
		[SerializeField] private SceneAssistantMenu sceneAssistantMenu;
		[SerializeField] private VariablesMenu variablesMenu;
		[SerializeField] private ScriptsMenu scriptsMenu;

		public Texture2D CursorTexture;

		protected override void Awake()
		{
			base.Awake();
			sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
			CurrentMenu = sceneAssistantMenu;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			closeButton.OnButtonClicked += Hide;
		}

		protected override void OnDisable()
		{
			base.OnEnable();
			closeButton.OnButtonClicked -= Hide;
		}

		protected override void HandleVisibilityChanged(bool visible)
		{
			base.HandleVisibilityChanged(visible);

			if (sceneAssistantManager != null && visible)
			{
				CurrentMenu.InitializeMenu();
			}
			else
			{
				if(sceneAssistantManager != null && sceneAssistantManager.IsAvailable)
				{
					CurrentMenu.DestroyMenu();
					sceneAssistantManager.DestroySceneAssistant();
				}
			}
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

		private void SetWindowOpacity(float value)
		{
			var currentColor = background.color;
			currentColor.a = value;
			background.color = currentColor;
		}
		
		public void OnPointerDown(PointerEventData eventData)
		{
			sceneAssistantMenu.DestroyColorPicker();
		}
	}
	
	public static class SceneAssistantConsoleCommand
	{
		[ConsoleCommand("scn")]
		public static void ShowSceneAssistantUI() 
		{
			var sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
			var sceneAssistantUI = Engine.GetService<IUIManager>().GetUI<SceneAssistantUI>();

			sceneAssistantManager.InitializeSceneAssistant();
			sceneAssistantUI.Show();
		} 
	}
}