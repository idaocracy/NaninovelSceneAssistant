using System.Collections.Generic;
using System;
using Naninovel;
using UnityEngine;

namespace NaninovelSceneAssistant
{
    public interface IListCommandParameterData : ICommandParameterData
    {
        List<ICommandParameterData> Values { get; }
    }

    public class ListCommandData : CommandParameterData, IListCommandParameterData
    {
        public ListCommandData(string name, List<ICommandParameterData> list, Action<ISceneAssistantLayout, IListCommandParameterData> getLayout, params Func<bool>[] conditions) : base(name, conditions)
        {
            Values = list;
            this.getLayout = getLayout;
        }

        public List<ICommandParameterData> Values { get; }
        private readonly Action<ISceneAssistantLayout, IListCommandParameterData> getLayout;

        public override string GetCommandValue(bool paramOnly = false) => CommandParameterDataExtensions.GetListValue(this, paramOnly);
        public override void DrawLayout(ISceneAssistantLayout layout) => getLayout(layout, this);
        public override void ResetDefault() => Values.ForEach(c => c.ResetDefault());
        public override async Awaitable ResetState()
        {
            foreach (var value in Values)
            {
                await Async.NextFrame();
                await value.ResetState();
            }
        }
    }
}
