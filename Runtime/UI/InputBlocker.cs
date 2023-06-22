using UnityEngine;
using UnityEngine.EventSystems;
using Naninovel;

public class InputBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	private IInputManager inputManager;
	
	private void Awake() 
	{
		inputManager = Engine.GetService<IInputManager>();
	}

	public void ToggleInputProcessing(bool value) => inputManager.ProcessInput = value;

	public void OnPointerEnter(PointerEventData eventData) => ToggleInputProcessing(false);
	public void OnPointerExit(PointerEventData eventData) 
	{
		if(!EventSystem.current.currentSelectedGameObject == this) ToggleInputProcessing(true);
	}

	public void OnSelect(BaseEventData eventData) => ToggleInputProcessing(false);
	public void OnDeselect(BaseEventData eventData)  => ToggleInputProcessing(true);
}
