using Naninovel;
using System;

namespace NaninovelSceneAssistant
{
    public interface INamedCommandData<T> : ICommandData<T>
    {
        string NamedKey { get; }
    }

    public class NamedCommandData<T> : CommandData<T>, INamedCommandData<T>
    {
        public NamedCommandData(string name, string namedKey, Func<T> getValue, Action<T> setValue,
        Action<ISceneAssistantLayout, ICommandData<T>> getLayout, T defaultValue = default, Func<bool> getCondition = null) : base(name, getValue, setValue, getLayout, defaultValue, getCondition)
        {
            NamedKey = namedKey;
        }

        public string NamedKey { get; }
        public override string GetCommandValue(bool paramOnly) => this.GetNamedValue(paramOnly);
    }
}