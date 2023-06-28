using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using TMPro;
using Naninovel;

namespace NaninovelSceneAssistant
{
	public class ColorPicker : ScriptableUIBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		protected Slider AlphaSlider => alphaSliderObject?.GetComponentInChildren<Slider>();
		protected Slider IntensitySlider => intensitySliderObject?.GetComponentInChildren<Slider>();

		[SerializeField] private RectTransform tab;
		[SerializeField] private Button eyeDropperButton;
		[SerializeField] private Image selectedColor;
		[SerializeField] private Image stateColor;
		[SerializeField] private ColorWheel colorWheel;
		[SerializeField] private TMP_InputField inputField;
		[SerializeField] private GameObject alphaSliderObject;
		[SerializeField] private GameObject intensitySliderObject;
		[SerializeField] private Image gradient;
		[SerializeField] private LabeledButton closeButton;

		public static bool IsOpen;
		public static bool isMouseOverWindow;
		
		private Action onEyeDropper;
		private Action<Color> setDataValue;

		private Color currentValue;

		public Color Value { get => currentValue; 
			set { 
				currentValue = value;
				selectedColor.color = value; 
				inputField.text = "#" + ColorUtility.ToHtmlStringRGBA(value); 
				gradient.color = new Color(value.r, value.g, value.b, 1); 
				SetDataValue(value);
			} 
		}

		public void Initialize(Color currentColor, bool includeAlpha, bool includeHDR, Action<Color> setDataValue, Action onEyeDropper)
		{
			Value = currentColor;
			colorWheel.color = currentColor;
			IsOpen = true;

			alphaSliderObject.SetActive(includeAlpha);
			intensitySliderObject.SetActive(includeHDR);
			stateColor.color = currentColor;
			selectedColor.color = currentColor;
			AlphaSlider.value = currentColor.a;
			this.onEyeDropper = onEyeDropper;
			this.setDataValue = setDataValue;

			closeButton.onClick.AddListener(DestroyColorPicker);
			inputField.onSubmit.AddListener(SetColorString);
			AlphaSlider?.onValueChanged.AddListener(SetAlpha);
			IntensitySlider?.onValueChanged.AddListener(SetIntensity);
			colorWheel.onColorChanged += GetColor;
			eyeDropperButton.onClick.AddListener(ActivateEyeDropper);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			AlphaSlider?.onValueChanged.RemoveListener(SetAlpha);
			IntensitySlider?.onValueChanged.RemoveListener(SetIntensity);
			closeButton?.onClick.RemoveListener(DestroyColorPicker);
			inputField?.onSubmit.RemoveListener(SetColorString);
			colorWheel.onColorChanged -= GetColor;
			eyeDropperButton.onClick.RemoveListener(ActivateEyeDropper);
			IsOpen = false;
		}

		private void SetDataValue(Color color) 
		{
			if(setDataValue != null) setDataValue(color);
		}

		private void ActivateEyeDropper() => onEyeDropper();

		private void DestroyColorPicker() => Destroy(this.gameObject);

		private void SetColorString(string value)
		{
			if (ColorUtility.TryParseHtmlString(value, out var color)) Value = color;
		}

		private void SetIntensity(float value)
		{
			float factor = Mathf.Pow(2, value);
			var currentValue = colorWheel.color;
			Value = new Color(currentValue.r * factor, currentValue.g * factor, currentValue.b * factor);
		}

		private void SetAlpha(float value)
		{
			var currentColor = Value;
			Value = new Color(currentColor.r, currentColor.g, currentColor.b, value);
		}

		public void GetColor(Color color)
		{
			var currentColor = Value;
			Value = new Color (color.r, color.g, color.b, currentColor.a);
		}

		public void OnPointerEnter(PointerEventData eventData) => isMouseOverWindow = true;
		public void OnPointerExit(PointerEventData eventData) => isMouseOverWindow = false; 

	}
	public class ColorPickerEvent : UnityEvent<Color> { }
}