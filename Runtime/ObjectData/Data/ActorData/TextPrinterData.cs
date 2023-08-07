using Naninovel;

namespace NaninovelSceneAssistant
{
	public class TextPrinterData : ActorData<TextPrinterManager, ITextPrinterActor, TextPrinterMetadata, TextPrintersConfiguration>
	{
		public TextPrinterData(string id) : base(id) { }
		public static string TypeId => "Text Printer";
		protected override string CommandNameAndId => "printer " + Id;
		protected override void AddCommandParameters()
		{
			AddBaseParameters(includeZPos:false);
		}
		
		protected override void GetAppearanceData()
		{
			CommandParameters.Add(new CommandParameterData<string>("Appearance", () => Actor.Appearance, v => Actor.Appearance = (string)v, (i, p) => i.StringField(p)));
		}
	}
}

