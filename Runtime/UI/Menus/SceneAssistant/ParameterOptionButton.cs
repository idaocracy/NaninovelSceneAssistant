using System.Collections.Generic;
using UnityEngine;
using Naninovel;

namespace NaninovelSceneAssistant
{
	public class ParameterOptionButton : ScriptableButton
	{
		[SerializeField] private ParameterOption buttonAction;
		
		enum ParameterOption { Select, Deselect, Default, Reset, Rollback}

		private SceneAssistantManager sceneAssistantManager;
		private SceneAssistantMenu sceneAssistantMenu;
		
		private List<ISceneAssistantUIField> dataFields => sceneAssistantMenu?.DataFields; 

		protected override void Awake() 
		{
			sceneAssistantMenu = GetComponentInParent<SceneAssistantMenu>();
			sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
		}

		protected override void OnButtonClick()
		{
			base.OnButtonClick();
			
			switch (buttonAction)
			{
				case ParameterOption.Select: 
				dataFields.ForEach(f => f.ToggleInteractability(true));
				break;
				
				case ParameterOption.Deselect: 
				dataFields.ForEach(f => f.ToggleInteractability(false));
				break;
				
				case ParameterOption.Default: 
				dataFields.ForEach(f => f.Data.ResetDefault());
				sceneAssistantMenu.UpdateDataValues();
				break;
				
				case ParameterOption.Reset: 
				dataFields.ForEach(f => f.Data.ResetState());
				sceneAssistantMenu.UpdateDataValues();
				break;
				
				case ParameterOption.Rollback: 
				foreach(var obj in sceneAssistantManager.ObjectList.Values)
				{
					obj.CommandParameters.ForEach(p => p.ResetState());
				}
				sceneAssistantMenu.UpdateDataValues();
				break;
			}
			
		}


	


	}
}
