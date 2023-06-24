using UnityEngine;
using UnityEngine.UI;
using Naninovel;

namespace NaninovelSceneAssistant 
{
	public class OpacitySlider : ScriptableSlider
	{
		[SerializeField] private Image background;
		
		protected override void Awake() 
		{
			base.Awake();
			UIComponent.value = background.color.a;
		}
		

		protected override void OnValueChanged (float value)
		{
			var currentColor = background.color;
			currentColor.a = value;
			background.color = currentColor;
		}
	}
}
	

