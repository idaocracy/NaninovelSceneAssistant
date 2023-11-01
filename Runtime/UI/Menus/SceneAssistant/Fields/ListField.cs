using UnityEngine;
using UnityEngine.UI;

namespace NaninovelSceneAssistant
{
	public class ListField : SceneAssistantDataField<Toggle>
	{
		[SerializeField] private Transform parameterContainer;

		public virtual void Initialize(IListCommandParameterData list, params ToggleGroupData[] toggleGroup)
		{
			InitializeBaseData(list, toggleGroup);
			SceneAssistantMenu.GenerateLayout(list.Values, parameterContainer);
		}

		public override void GetDataValue()
		{
			foreach (var field in parameterContainer.GetComponentsInChildren<ISceneAssistantUIField>()) field.GetDataValue();
			CheckConditions();
		}

		public override void ToggleInteractability(bool interactable)
		{
			base.ToggleInteractability(interactable);
			foreach (var toggle in GetComponentsInChildren<Toggle>()) 
			{
				if(toggle != UIComponent) toggle.interactable = interactable; 
			}
		}
	}
}
