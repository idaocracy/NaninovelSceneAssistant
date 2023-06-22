using Naninovel;
using System;

namespace NaninovelSceneAssistant
{
    public interface INamedCommandParameterData<T> : ICommandParameterData<T>
    {
        string NamedKey { get; }
    }

    public class NamedCommandParameterData<T> : CommandParameterData<T>, INamedCommandParameterData<T>
    {
        public NamedCommandParameterData(string name, string namedKey, Func<T> getValue, Action<T> setValue,
        Action<ISceneAssistantLayout, ICommandParameterData<T>> getLayout, T defaultValue = default, params Func<bool>[] conditions) : base(name, getValue, setValue, getLayout, defaultValue, conditions)
        {
            NamedKey = namedKey;
        }

        public string NamedKey { get; }
        public override string GetCommandValue(bool paramOnly) => this.GetNamedValue(paramOnly);
    }
}