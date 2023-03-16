using System.Collections.Generic;
using System;
using System.Linq;

namespace NaninovelSceneAssistant
{
    public interface IListCommandData : ICommandData
    {
        List<ICommandData> Values { get; }
    }

    public class ListCommandData : CommandData, IListCommandData
    {
        public ListCommandData(string name, List<ICommandData> list, Action<ISceneAssistantLayout, IListCommandData> getLayout, Func<bool> getCondition = null) : base(name, getCondition)
        {
            Values = list;
            this.getLayout = getLayout;
        }

        public List<ICommandData> Values { get; }
        private Action<ISceneAssistantLayout, IListCommandData> getLayout;

        public override string GetCommandValue(bool paramOnly = false)
        {
            return (paramOnly ? string.Empty : this.FormatName() + ":") + string.Join(",", Values.Select(c => c.GetCommandValue(paramOnly:true)));
        }

        public override void GetLayout(ISceneAssistantLayout layout) => getLayout(layout, this);
        public override void ResetDefault() => Values.ForEach(c => c.ResetDefault());
        public override void ResetState() => Values.ForEach(c => c.ResetState());
    }
}
