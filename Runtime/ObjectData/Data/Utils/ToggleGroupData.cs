namespace NaninovelSceneAssistant
{
    public class ToggleGroupData
    {
        public ICommandParameterData Data;
        public bool ResetOnToggle;

        public ToggleGroupData(ICommandParameterData data, bool resetOnToggle = true)
        {
            this.Data = data;
            this.ResetOnToggle = resetOnToggle;
        }
    }
}
