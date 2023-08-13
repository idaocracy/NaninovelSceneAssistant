using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using Naninovel.UI;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace NaninovelSceneAssistant
{
	public class TextPrinterData : ActorData<TextPrinterManager, ITextPrinterActor, TextPrinterMetadata, TextPrintersConfiguration>
	{
		public TextPrinterData(string id) : base(id) { }
		public static string TypeId => "Text Printer";
		protected override string CommandNameAndId => $"printer {Id}";
				
		protected override void AddCommandParameters()
		{
			AddBaseParameters(includeZPos:false);
		}
				
		#if UNITY_EDITOR
		protected string[] GetPrinterAppearances() 
		{
			var printerPanel = GetGameObject().GetComponent<UITextPrinterPanel>();
			var printerType = printerPanel.GetType();
			
			FieldInfo fieldInfo = printerType.GetField("appearances", BindingFlags.NonPublic | BindingFlags.Instance);
			if(fieldInfo == null) return null;
			
			List<CanvasGroup> appearanceList = (List<CanvasGroup>)fieldInfo.GetValue(GetGameObject().GetComponent(printerType));
			return appearanceList.Select(c => c.name).ToArray();
		}	
		#endif
		
		protected override void GetAppearanceData()
		{
			#if UNITY_EDITOR
			var appearances = GetPrinterAppearances();
			if(appearances != null && appearances.Length > 0) 				
			CommandParameters.Add(new CommandParameterData<string>(Appearance, () => Actor.Appearance ?? appearances[0], v => Actor.Appearance = (string)v, (i, p) => i.StringDropdownField(p, appearances), defaultValue: appearances[0]));			
			#else
			CommandParameters.Add(new CommandParameterData<string>(Appearance, () => Actor.Appearance, v => Actor.Appearance = (string)v, (i, p) => i.StringField(p)));
			#endif
		}
	}
}

