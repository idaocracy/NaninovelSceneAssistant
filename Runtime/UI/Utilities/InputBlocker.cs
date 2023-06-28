using UnityEngine;
using UnityEngine.EventSystems;
using Naninovel;
using TMPro;

public class InputBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private IInputManager inputManager;
	
	private bool isInput;
	
	private void Awake() 
	{
		inputManager = Engine.GetService<IInputManager>();
		isInput = GetComponent<TMP_InputField>() != null;
	}

	public void ToggleInputProcessing(bool value) => inputManager.ProcessInput = value;

	public void OnPointerEnter(PointerEventData eventData) => ToggleInputProcessing(false);
	public void OnPointerExit(PointerEventData eventData) 
	{
		ToggleInputProcessing(true);
	}
}
