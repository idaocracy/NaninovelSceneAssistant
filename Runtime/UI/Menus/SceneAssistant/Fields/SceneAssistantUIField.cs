using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Naninovel;
using TMPro;
using System;
using System.Linq;

namespace NaninovelSceneAssistant
{
	public interface ISceneAssistantUIField
	{
		ICommandParameterData Data { get; }
		void ToggleInteractability(bool interactable);
		void GetDataValue();
	}

	public abstract class SceneAssistantDataField<TComponent> : ScriptableUIControl<Toggle>, ISceneAssistantUIField where TComponent : Selectable
	{
		[SerializeField] private Toggle toggle;
		[SerializeField] private Button button;

		public ICommandParameterData Data { get; protected set; }
		public override Toggle UIComponent => toggle;
		protected virtual TComponent ValueComponent { get; }
		protected ICommandParameterData[] toggleGroup { get; set; }

		protected virtual TextMeshProUGUI Label => button.GetComponentInChildren<TextMeshProUGUI>();
		protected SceneAssistantMenu SceneAssistantMenu;
		protected SceneAssistantUI SceneAssistantUI;
		
		protected override void Awake()
		{
			base.Awake();
			SceneAssistantMenu = GetComponentInParent<SceneAssistantMenu>();
			SceneAssistantUI = GetComponentInParent<SceneAssistantUI>();
		}

		protected override void BindUIEvents()
		{
			UIComponent.onValueChanged.AddListener(ToggleInteractability);
			button.onClick.AddListener(CopyValueToBuffer);
		}

		protected override void UnbindUIEvents()
		{
			UIComponent.onValueChanged.RemoveListener(ToggleInteractability);
			button.onClick.RemoveListener(CopyValueToBuffer);
		}

		protected void InitializeBaseData(ICommandParameterData data, ICommandParameterData[] toggleGroup)
		{
			Data = data;
			Label.text = Data.Name;
			this.toggleGroup = toggleGroup; 
			ToggleInteractability(Data.Selected);
		}

		public virtual void ToggleInteractability(bool interactable)
		{
			toggle.isOn = interactable;
			Data.Selected = interactable;

			CanvasGroup.interactable = interactable;

			if (interactable && toggleGroup.Length > 0)
			{
				foreach(var field in SceneAssistantMenu.DataFields)
				{
					foreach(var toggle in toggleGroup)
					{
						if(field.Data == toggle) field.ToggleInteractability(false);                    
					} 
				}
			}
			
			if (!interactable)
			{
				if(toggleGroup.Length == 0) Data.ResetState();
				else if (toggleGroup.Length > 0 && toggleGroup.Any(t => t.Selected)) return;
				else Data.ResetState();
				SceneAssistantMenu.UpdateDataValues();
			}
		}

		protected void CheckCondition()
		{
			if (!Data.FulfillsConditions()) transform.localScale = Vector3.zero;
			else transform.localScale = Vector3.one;
			LayoutRebuilder.MarkLayoutForRebuild(SceneAssistantMenu.GetComponent<RectTransform>());
		}

		protected virtual void CopyValueToBuffer() => SceneAssistantMenu.CopyToBuffer(Data.GetCommandValue(paramOnly: true));

		public abstract void GetDataValue();
	}

	public abstract class SceneAssistantDataField<TComponent, TValue> : SceneAssistantDataField<TComponent> where TComponent : Selectable
	{
		[SerializeField] private TComponent valueComponent;
		protected override TComponent ValueComponent => valueComponent;
		protected abstract UnityEvent<TValue> Event { get; }
		protected Action getDataValue;
		protected Action<TValue> setDataValue;

		protected override void BindUIEvents()
		{
			base.BindUIEvents();
			Event?.AddListener(SetDataValue);
		}

		protected override void UnbindUIEvents()
		{
			base.UnbindUIEvents();
			Event?.RemoveListener(SetDataValue);
		}

		protected virtual void SetDataValue(TValue value)
		{
			setDataValue(value);
			SceneAssistantMenu.UpdateDataValues();
		}

		public override void GetDataValue()
		{
			getDataValue();
			CheckCondition();
		}
	}
}