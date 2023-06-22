using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using TMPro;

namespace NaninovelSceneAssistant
{
	public class SavePanel : ScriptableUIBehaviour
	{
		[SerializeField] private LabeledButton closeButton;
		[SerializeField] private Button saveButton;
		[SerializeField] private TMP_InputField inputField;
		
		protected string CommandString;
		private Action onClose;
		
		public void Initialize(string commandString, Action onClose)
		{
			CommandString = commandString;
			this.onClose = onClose;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			closeButton.onClick.AddListener(CloseWindow);
			saveButton.onClick.AddListener(SaveOnClick);
			inputField.onSubmit.AddListener(SaveOnSubmit);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			closeButton.onClick.AddListener(CloseWindow);
			saveButton.onClick.RemoveListener(SaveOnClick);
			inputField.onSubmit.RemoveListener(SaveOnSubmit);
		}

		private void CloseWindow() => onClose();

		private void SaveOnClick()
		{
			var sceneAssistantDirectory =  "/SceneAssistant/";
			var sceneAssistantFileName = "SceneAssistant";
			var sceneAssistantFilePath = Application.streamingAssetsPath + sceneAssistantDirectory + sceneAssistantFileName + ".nani";
			
			var result = (!String.IsNullOrEmpty(inputField.text) ? "\n" + inputField.text : string.Empty) + "\n" + CommandString;
			
			if(!File.Exists(sceneAssistantFilePath))
			{
				File.WriteAllText(sceneAssistantFilePath, result);
			}
			else File.AppendAllText(sceneAssistantFilePath, result);
			
			CloseWindow();
		}

		private void SaveOnSubmit(string value) => SaveOnClick();
	}
}

