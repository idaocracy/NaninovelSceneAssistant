using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using TMPro;

namespace NaninovelSceneAssistant
{
	public class SceneAssistantMenu : SceneAssistantWindowMenu, ISceneAssistantLayout
	{
		[Header("Main elements")]
		[SerializeField] private CanvasGroup mainWindow;
		[SerializeField] private Transform parameterContainer;
		[SerializeField] private TMP_InputField copyBufferField;
		[SerializeField] private TextMeshProUGUI saveInfoBox;
		[SerializeField] private Button saveButton;
		[SerializeField] private SavePanel savePanelPrototype;

		[Header("Object type section")]
		[SerializeField] private RectTransform objectTypeToggleContainer;
		[SerializeField] private ObjectTypeToggle objectTypeTogglePrototype;

		[Header("Id section")]
		[SerializeField] private Button idButton;
		[SerializeField] private TMP_Dropdown idDropdown;


		[Header("Field prototypes")]
		[SerializeField] private SliderField sliderFieldPrototype;
		[SerializeField] private InputField inputFieldPrototype;
		[SerializeField] private DropdownField dropdownFieldPrototype;
		[SerializeField] private ToggleField toggleFieldPrototype;
		[SerializeField] private ScrollableInputField scrollableFieldPrototype;
		[SerializeField] private ColorField colorFieldPrototype;
		[SerializeField] private ListField listFieldPrototype;

		private Transform targetContainer;
		public List<ISceneAssistantUIField> DataFields { get => parameterContainer.GetComponentsInChildren<ISceneAssistantUIField>().ToList(); }
		public INaninovelObjectData CurrentObject { get; protected set; }
		
		public override void InitializeMenu()
		{
			base.InitializeMenu();
			idDropdown.onValueChanged.AddListener(DisplayCurrentObject);
			idButton.onClick.AddListener(CopyIdString);
			saveButton.onClick.AddListener(ShowSavePanel);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			idDropdown.onValueChanged.RemoveListener(DisplayCurrentObject);
			idButton.onClick.RemoveListener(CopyIdString);
			saveButton.onClick.RemoveListener(ShowSavePanel);
		}
		private void ShowSavePanel()
		{
			var sceneAssistantDirectory = "/SceneAssistant/";
			var sceneAssistantFileName = "SceneAssistant";
			if(!Directory.Exists(Application.streamingAssetsPath + sceneAssistantDirectory))
			{
				saveInfoBox.color = Color.red;
				saveInfoBox.text = $"Error: Could not find <b>SceneAssistant</b> directory in {Application.streamingAssetsPath}";
				return;
			}
			
			if(String.IsNullOrEmpty(copyBufferField.text))
			{
				saveInfoBox.color = Color.red;
				saveInfoBox.text = $"String is empty";
				return;
			}
			
			SavePanel savePanel = Instantiate(savePanelPrototype, UI.transform);
			savePanel.Initialize(copyBufferField.text, CloseWindow());
			mainWindow.interactable = false;
			
			Action CloseWindow()
			{
				return () => 
				{
					Destroy(savePanel.gameObject);
					mainWindow.interactable = true;
					
					saveInfoBox.color = Color.green;
					saveInfoBox.text = $"Successfully saved string to <b>{sceneAssistantFileName}.nani<b> at {Application.streamingAssetsPath}";
				};
			}
		}

		protected override void ResetMenu()
		{
			copyBufferField.text = string.Empty;
			foreach (Transform child in parameterContainer) Destroy(child.gameObject);
			foreach (Transform child in objectTypeToggleContainer) Destroy(child.gameObject);
			idDropdown.AddOptions(Manager.ObjectList.Keys.Select(v => new TMP_Dropdown.OptionData(v)).ToList());
			DisplayCurrentObject(0);
			ResetToggles();
		}

		private void ResetToggles()
		{
			foreach (var kv in Manager.ObjectTypeList)
			{
				var toggle = Instantiate(objectTypeTogglePrototype, objectTypeToggleContainer);
				toggle.Initialize(kv);
			}
		}

		protected void DisplayCurrentObject(int index)
		{
			foreach (Transform child in parameterContainer) Destroy(child.gameObject);
			DestroyColorPicker();
			CurrentObject = Manager.ObjectList.ElementAt(index).Value;
			GenerateLayout(CurrentObject.CommandParameters, parameterContainer);
			saveInfoBox.text = String.Empty;
		}

		protected override void ClearMenu()
		{
			foreach (Transform child in parameterContainer) Destroy(child.gameObject);
			foreach (Transform child in objectTypeToggleContainer) Destroy(child.gameObject);
			DestroyColorPicker();
			Destroy(GetComponentInChildren<ColorPicker>()?.gameObject);
			idDropdown.ClearOptions();
			saveInfoBox.text = String.Empty;
		}

		private void CopyIdString() => CopyToBuffer(CurrentObject.Id);

		public void CopyToBuffer(string text)
		{
			GUIUtility.systemCopyBuffer = text;
			copyBufferField.text = text;
			saveInfoBox.text = String.Empty;
		}

		public void UpdateDataValues()
		{
			foreach (var field in DataFields) field.GetDataValue();
		}

		public void GenerateLayout(List<ICommandParameterData> list, Transform parent)
		{
			targetContainer = parent;
			foreach (var data in list) data.DrawLayout(this);
		}

		public void DestroyColorPicker()
		{
			if (ColorPicker.IsOpen && !ColorPicker.isMouseOverWindow) Destroy(UI.GetComponentInChildren<ColorPicker>().gameObject);
		} 

		public void StringField(ICommandParameterData<string> data, params ICommandParameterData[] toggleGroup) 
		{
			InputField inputField = Instantiate(inputFieldPrototype, targetContainer);
			inputField.Initialize(data, toggleGroup);
		}

		public void StringDropdownField(ICommandParameterData<string> data, string[] stringValues, params ICommandParameterData[] toggleGroup) 
		{
			DropdownField inputField = Instantiate(dropdownFieldPrototype, targetContainer);
			inputField.Initialize(data, stringValues:stringValues, toggleGroup:toggleGroup);
		}

		public void TypeDropdownField<T>(ICommandParameterData<T> data, Dictionary<string, T> values, params ICommandParameterData[] toggleGroup) 
		{
			DropdownField dropdownField = Instantiate(dropdownFieldPrototype, targetContainer);
			dropdownField.Initialize(data, stringValues:values.Keys.ToArray(), typeValues:values.Values.ToArray(), toggleGroup:toggleGroup);
		}

		public void BoolField(ICommandParameterData<bool> data, params ICommandParameterData[] toggleGroup) 
		{
			ToggleField toggleField = Instantiate(toggleFieldPrototype, targetContainer);
			toggleField.Initialize(data, toggleGroup);
		}

		public void IntField(ICommandParameterData<int> data, int? min, int? max, params ICommandParameterData[] toggleGroup) 
		{
			ScrollableInputField inputField = Instantiate(scrollableFieldPrototype, targetContainer);
			inputField.Initialize(data, min:min, max:max, toggleGroup: toggleGroup);
		}

		public void FloatField(ICommandParameterData<float> data, float? min = null, float? max = null, params ICommandParameterData[] toggleGroup) 
		{
			ScrollableInputField inputField = Instantiate(scrollableFieldPrototype, targetContainer);
			inputField.Initialize(data, min:min, max:max, toggleGroup:toggleGroup);
		}

		public void FloatSliderField(ICommandParameterData<float> data, float min, float max, params ICommandParameterData[] toggleGroup)
		{
			SliderField sliderField = Instantiate(sliderFieldPrototype, targetContainer);
			sliderField.Initialize(data, min, max, toggleGroup);
		}

		public void IntSliderField(ICommandParameterData<int> data, int min, int max, params ICommandParameterData[] toggleGroup) 
		{
			SliderField sliderField = Instantiate(sliderFieldPrototype, targetContainer);
			sliderField.Initialize(data, min, max, toggleGroup);
		}

		public void ColorField(ICommandParameterData<Color> data, bool includeAlpha = true, bool includeHDR = false, params ICommandParameterData[] toggleGroup) 
		{
			ColorField colorField = Instantiate(colorFieldPrototype, targetContainer);
			colorField.Initialize(data, includeAlpha, includeHDR, toggleGroup);
		}

		public void EnumDropdownField(ICommandParameterData<Enum> data, params ICommandParameterData[] toggleGroup) 
		{
			DropdownField dropdownField = Instantiate(dropdownFieldPrototype, targetContainer);
			dropdownField.Initialize(data, toggleGroup:toggleGroup);
		}

		public void Vector2Field(ICommandParameterData<Vector2> data, params ICommandParameterData[] toggleGroup) 
		{
			ScrollableInputField inputField = Instantiate(scrollableFieldPrototype, targetContainer);
			inputField.Initialize(data, toggleGroup:toggleGroup);
		}

		public void Vector3Field(ICommandParameterData<Vector3> data, params ICommandParameterData[] toggleGroup) 
		{
			ScrollableInputField inputField = Instantiate(scrollableFieldPrototype, targetContainer);
			inputField.Initialize(data, toggleGroup:toggleGroup);
		}

		public void Vector4Field(ICommandParameterData<Vector4> data, params ICommandParameterData[] toggleGroup) 
		{
			ScrollableInputField inputField = Instantiate(scrollableFieldPrototype, targetContainer);
			inputField.Initialize(data, toggleGroup:toggleGroup);
		}

		public void PosField(ICommandParameterData<Vector3> data, CameraConfiguration cameraConfiguration, params ICommandParameterData[] toggleGroup) 
		{
			ScrollableInputField inputField = Instantiate(scrollableFieldPrototype, targetContainer);
			inputField.Initialize(data, cameraConfiguration:cameraConfiguration, isPos:true, toggleGroup:toggleGroup);
		}

		public void ListField(IListCommandParameterData list, params ICommandParameterData[] toggleGroup) 
		{
			ListField listField = Instantiate(listFieldPrototype, targetContainer);
			listField.Initialize(list, toggleGroup);
		}
	}
}