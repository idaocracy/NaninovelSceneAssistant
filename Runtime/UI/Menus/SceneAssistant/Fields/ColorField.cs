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
		protected Color ColorValue { get => ColorValue; set => UpdateColorDisplay(value); }
		public ColorPicker ColorPicker { get; private set; }

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

		public virtual void Initialize(ICommandParameterData data, bool includeAlpha = true, bool includeHDR = false, params ToggleGroupData[] toggleGroup)
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

			GetDataValue();
		}
		
		protected override void BindUIEvents()
		{
			base.BindUIEvents();
			ValueComponent.onClick.AddListener(ShowColorPicker);
			eyeDropperButton.onClick.AddListener(ActivateEyeDropper);
		}

		protected override void UnbindUIEvents()
		{
			base.UnbindUIEvents();
			ValueComponent.onClick.RemoveListener(ShowColorPicker);
			eyeDropperButton.onClick.RemoveListener(ActivateEyeDropper);
		}
		
		protected override void OnDestroy() 
		{
			DestroyColorPicker();
		}
		
		private void DestroyColorPicker()
		{
			if (ColorPicker.IsOpen && !ColorPicker.isMouseOverWindow) Destroy(ColorPicker.gameObject);
		}
		
		private void UpdateColorDisplay(Color value)
		{
			colorImage.color = new Color(value.r, value.g, value.b); 
			alphaImage.value = value.a;
		}
		
		private void ShowColorPicker()
		{
			if (ColorPicker.IsOpen) return;
			if (Data is ICommandParameterData<Color> colorData)
			{
				ColorPicker = Instantiate(colorPickerPrototype, SceneAssistantUI.transform);
				ColorPicker.Initialize(colorData.Value, includeAlpha, includeHDR, SetDataValue, ActivateEyeDropper);
			}
		}

		private void ActivateEyeDropper() 
		{
			DestroyColorPicker();
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

			SetDataValue(sampledColor);
			SceneAssistantMenu.UpdateDataValues();
		}

		void ForceTransitionalSpritesUpdate ()
		{
			var updateMethod = typeof(TransitionalSpriteRenderer).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
			var sprites = UnityEngine.Object.FindObjectsByType<TransitionalSpriteRenderer>(FindObjectsSortMode.None);
			foreach (var sprite in sprites)
				updateMethod.Invoke(sprite, null);
		}
	}
}
