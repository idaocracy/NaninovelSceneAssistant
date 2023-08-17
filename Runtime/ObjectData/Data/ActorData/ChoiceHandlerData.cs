using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Naninovel;
using Naninovel.UI;

namespace NaninovelSceneAssistant
{
	public class ChoiceHandlerData : ActorData<ChoiceHandlerManager, IChoiceHandlerActor, ChoiceHandlerMetadata, ChoiceHandlersConfiguration>
	{
		public ChoiceHandlerData(string id) : base(id) { }
		
		protected const string ChoiceHandlerName = "ChoiceHandler", ChoiceHandlerCommandName = "choice";
		public static string TypeId => ChoiceHandlerName;
		protected override string CommandNameAndId => ChoiceHandlerCommandName;
		
		protected Dictionary<ChoiceState, ChoiceHandlerButton> ChoiceButtons;

		public override string GetCommandLine( bool inlined = false, bool paramsOnly = false)
		{
			var choiceList = new List<string>();

			foreach (var param in CommandParameters)
			{
				if(!param.Selected) continue;
				var choiceString = $"{CommandNameAndId} {param.GetCommandValue()} handler:{Id}";
				choiceList.Add(inlined ? $"[{choiceString}]" : $"@{choiceString}");
			}

			return string.Join("\n", choiceList);
		}

		protected override void AddCommandParameters()
		{
			ChoiceButtons = new Dictionary<ChoiceState, ChoiceHandlerButton>();

			foreach(var choiceState in Actor.Choices)
			{
				ChoiceButtons.Add(choiceState, GameObject.GetComponentsInChildren<ChoiceHandlerButton>().FirstOrDefault(c => c.ChoiceState == choiceState));
			}

			foreach (var button in ChoiceButtons)
			{
				CommandParameters.Add(new CommandParameterData<Vector2>($"{button.Value.ChoiceState.Summary} {Pos}", () => (Vector2)button.Value.transform.localPosition, v => button.Value.transform.localPosition = v, (i, p) => i.Vector2Field(p)));
			}
		}

		protected override void GetAppearanceData() {}
	}
}
	