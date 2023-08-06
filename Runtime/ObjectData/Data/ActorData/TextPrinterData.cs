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
	}
}

