using Naninovel;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace NaninovelSceneAssistant
{
	public class CopyCommandButton : ScriptableButton
	{
		[SerializeField] private CopyCommandType copyCommandType;
		[SerializeField] private bool inlined;
		private SceneAssistantMenu sceneAssistantMenu;
		private SceneAssistantManager sceneAssistantManager;
		
		private TMP_InputField copyBufferField;

		enum CopyCommandType { CopyCommand, CopySelected, CopyAll };

		protected override void Awake()
		{
			base.Awake();
			sceneAssistantMenu = GetComponentInParent<SceneAssistantMenu>();
			sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
			copyBufferField = sceneAssistantMenu.CopyBufferField;
			
			#if UNITY_WEBGL
			var label = GetComponentInChildren<TextMeshProUGUI>();
			label.text = label.text.Replace("Copy", "Generate");
			#endif 
		}
		protected override void OnButtonClick()
		{
			base.OnButtonClick();

			var currentObject = sceneAssistantMenu.CurrentObject;
			var objectList = sceneAssistantManager.ObjectList;
			var objectTypeList = sceneAssistantManager.ObjectTypeList;

			if (copyCommandType == CopyCommandType.CopyCommand) sceneAssistantMenu.CopyToBuffer(currentObject.GetCommandLine(inlined));
			else if (copyCommandType == CopyCommandType.CopySelected) sceneAssistantMenu.CopyToBuffer(string.Join(inlined ? "" : "\n", currentObject.GetAllCommands(objectList, objectTypeList, inlined, selected:true)));
			else if (copyCommandType == CopyCommandType.CopyAll) sceneAssistantMenu.CopyToBuffer(string.Join(inlined ? "" : "\n", currentObject.GetAllCommands(objectList, objectTypeList, inlined, selected:false)));
			
			#if UNITY_WEBGL && !UNITY_EDITOR
			EventSystem.current.SetSelectedGameObject(copyBufferField.gameObject);
			#endif
		}
	}
}