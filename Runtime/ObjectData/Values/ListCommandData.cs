using System.Collections.Generic;
using System;
using System.Linq;

namespace NaninovelSceneAssistant
{
    public interface IListCommandParameterData : ICommandParameterData
    {
        List<ICommandParameterData> Values { get; }
    }

    public class ListCommandData : CommandParameterData, IListCommandParameterData
    {
        public ListCommandData(string name, List<ICommandParameterData> list, Action<ISceneAssistantLayout, IListCommandParameterData> getLayout, Func<bool> getCondition = null) : base(name, getCondition)
        {
            Values = list;
            this.getLayout = getLayout;
        }

        public override bool Selected { get => base.Selected; set { base.Selected = value;  } }

        public List<ICommandParameterData> Values { get; }

        private Action<ISceneAssistantLayout, IListCommandParameterData> getLayout;

        public override string GetCommandValue(bool paramOnly = false) => CommandParameterDataExtensions.GetListValue(this, paramOnly);
        public override void GetLayout(ISceneAssistantLayout layout) => getLayout(layout, this);
        public override void ResetDefault() => Values.ForEach(c => c.ResetDefault());
        public override void ResetState() => Values.ForEach(c => c.ResetState());


    }
}
