using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NaninovelSceneAssistant
{
	public class ScrollableFloatValueField : TMP_InputField
	{
		public float FloatValue { get => float.Parse(text); set => text = value.ToString("0.###"); }
		protected TextMeshProUGUI Label => GetComponentInChildren<TextMeshProUGUI>();
		private Action getDataValue;
		private Action<float> setDataValue;
		private Vector2 hotSpot = Vector2.zero;
		private SceneAssistantMenu sceneAssistantMenu;
		private SceneAssistantUI sceneAssistantUI;
		private ScrollableInputField inputField;
		private CanvasGroup mainCanvasGroup;
		
		private float? minValue;
		private float? maxValue;

		public void Initialize(Action getDataValue, Action<float> setDataValue, float? min = null, float? max = null, bool wholeNumbers = false, string label = "")
		{
			Label.text = label;
			minValue = min;
			maxValue = max;
			contentType = wholeNumbers ? ContentType.IntegerNumber : ContentType.DecimalNumber;
			this.getDataValue = getDataValue;
			this.setDataValue = setDataValue;
			
			getDataValue();
		}
		
		protected override void Awake() 
		{
			base.Awake();
			sceneAssistantMenu = GetComponentInParent<SceneAssistantMenu>();
			sceneAssistantUI = GetComponentInParent<SceneAssistantUI>();
			inputField = GetComponentInParent<ScrollableInputField>();
			mainCanvasGroup = inputField.GetComponent<CanvasGroup>();
			
			onSubmit.AddListener(SetDataValue);
		}
		
		protected override void OnDestroy() 
		{
			base.OnDestroy();
			onSubmit.RemoveListener(SetDataValue);	
		}
		
		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			if(!mainCanvasGroup.interactable) return;
			Cursor.SetCursor(sceneAssistantUI.CursorTexture, hotSpot, CursorMode.ForceSoftware);
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			Cursor.SetCursor(default, hotSpot, CursorMode.ForceSoftware);
		}

		public override void OnDrag(PointerEventData eventData)
		{
			base.OnDrag(eventData);
			if(!mainCanvasGroup.interactable) return;

			var currentValue = FloatValue;
			var newValue = currentValue + (float)(eventData.delta.x * 0.1);
			FloatValue = newValue;
			SendOnSubmit();
		}

		private void SetDataValue(string value) 
		{ 
			var floatValue = float.Parse(value);
			if(minValue != null && floatValue < minValue) floatValue = (float)minValue;
			if(maxValue != null && floatValue > maxValue) floatValue = (float)maxValue;
			setDataValue(floatValue);
			sceneAssistantMenu.UpdateDataValues();
		}

		public void GetDataValue() 
		{
			getDataValue();
		} 
	
	}
}

