using Naninovel;
using System;

namespace NaninovelSceneAssistant
{
    public interface INamedCommandParameterData<T> : ICommandParameterData<T>
    {
        string NamedKey { get; }
    }

    public class NamedCommandData<T> : CommandParameterData<T>, INamedCommandParameterData<T>
    {
        public NamedCommandData(string name, string namedKey, Func<T> getValue, Action<T> setValue,
        Action<ISceneAssistantLayout, ICommandParameterData<T>> getLayout, T defaultValue = default, Func<bool> getCondition = null) : base(name, getValue, setValue, getLayout, defaultValue, getCondition)
        {
            NamedKey = namedKey;
        }

        public string NamedKey { get; }
        public override string GetCommandValue(bool paramOnly) => this.GetNamedValue(paramOnly);
    }
}