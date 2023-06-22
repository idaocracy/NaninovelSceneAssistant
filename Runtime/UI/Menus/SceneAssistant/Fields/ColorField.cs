using UnityEngine;
using Naninovel;
using Naninovel.UI;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;

namespace NaninovelSceneAssistant
{
	public class ColorField : SceneAssistantDataField<Button, Color>
	{
		protected Color ColorValue { get => ColorValue; set { colorImage.color = new Color(value.r, value.g, value.b); alphaImage.value = value.a; } }

		[SerializeField] private Button eyeDropperButton;
		[SerializeField] private Image colorImage;
		[SerializeField] private Slider alphaImage;
		[SerializeField] private ColorPicker colorPickerPrototype;
		private bool includeAlpha;
		private bool includeHDR;
		protected override UnityEvent<Color> Event => null;

		private IClickThroughPanel clickThroughPanel;
		private RenderTexture samplerTexture;
		private ICameraManager cameraManager;
		private bool isSampling;

		public virtual void Initialize(ICommandParameterData data, bool includeAlpha = true, bool includeHDR = false, params ICommandParameterData[] toggleGroup)
		{
			InitializeBaseData(data, toggleGroup);
			cameraManager = Engine.GetService<ICameraManager>();
			clickThroughPanel = Engine.GetService<IUIManager>().GetUI<IClickThroughPanel>();
			samplerTexture = new RenderTexture(Screen.width, Screen.height, 24);
			this.includeAlpha = includeAlpha;
			this.includeHDR = includeHDR;

			if(data is ICommandParameterData<Color> colorData)
			{
				getDataValue = () => ColorValue = colorData.Value;
				setDataValue = ColorValue => colorData.Value = ColorValue;
			}

			ValueComponent.onClick.AddListener(ShowColorPicker);
			eyeDropperButton.onClick.AddListener(ActivateEyeDropper);
			GetDataValue();
		}

		private void ShowColorPicker()
		{
			if (ColorPicker.IsOpen) return;
			if (Data is ICommandParameterData<Color> colorData)
			{
				var colorPicker = Instantiate(colorPickerPrototype, SceneAssistantUI.transform);
				colorPicker.Initialize(colorData.Value, includeAlpha, includeHDR, ActivateEyeDropper);
				colorPicker.onValueChanged.AddListener(SetDataValue);
			}
		}

		private void ActivateEyeDropper() 
		{
			clickThroughPanel.Show(true, SampleTexture);
		} 

		private void SampleTexture()
		{
			var initialTexture = cameraManager.Camera.targetTexture;
			cameraManager.Camera.targetTexture = samplerTexture;
			ForceTransitionalSpritesUpdate();
			cameraManager.Camera.Render();
			cameraManager.Camera.targetTexture = initialTexture;

			var initialUITexture = cameraManager.UICamera.targetTexture;
			cameraManager.UICamera.targetTexture = samplerTexture;
			cameraManager.UICamera.Render();
			cameraManager.UICamera.targetTexture = initialUITexture;

			var samplerImage = samplerTexture.ToTexture2D();

			var currentPosition = Input.mousePosition;
			Color sampledColor = samplerImage.GetPixel((int)currentPosition.x, (int)currentPosition.y);

			setDataValue(sampledColor);
			SceneAssistantMenu.UpdateDataValues();
		}

		void ForceTransitionalSpritesUpdate ()
		{
		var updateMethod = typeof(TransitionalSpriteRenderer).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
		var sprites = UnityEngine.Object.FindObjectsOfType<TransitionalSpriteRenderer>();
		foreach (var sprite in sprites)
			updateMethod.Invoke(sprite, null);
		}

	}

}
