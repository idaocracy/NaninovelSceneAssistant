﻿using Naninovel;
using Naninovel.Commands;
using System;

namespace NaninovelSceneAssistant
{
    public class VariableData : IDisposable
    {
        public string Name { get; }
        public string Value { get => getValue(); set { if (SceneAssistantManager.IsAvailable) setValue(value); } }
        public bool Changed { get; set; }
        //public string State { get; set; }

        private readonly Func<string> getValue;
        private readonly Action<string> setValue;

        protected SceneAssistantManager SceneAssistantManager => Engine.GetService<SceneAssistantManager>();

        public VariableData(string name)
        {
            Name = name;

            var customVariableManager = Engine.GetService<ICustomVariableManager>();

            // 1.20 workaround
            getValue = () => Naninovel.ExpressionEvaluator.Evaluate<string>(Name);
            setValue = (value) => new SetCustomVariable { Expression = Name + "=" + (IsStringValue(value) ? "\"" + value + "\"" : value) }.ExecuteAsync().Forget();

            // 1.19 legacy
            //getValue = () => customVariableManager.GetVariableValue(Name);
            //setValue = (value) => customVariableManager.SetVariableValue(Name, value);

            //State = Value;

            SceneAssistantManager.OnSceneAssistantReset += HandleReset;

            bool IsStringValue(string value)
            {
                if (float.TryParse(value, out _)) return false;
                else if (int.TryParse(value, out _)) return false;
                else if (bool.TryParse(value, out _)) return false;
                else return true;
            }
        }

        private void HandleReset()
        {
            //todo get the previous state, not initial value
            Changed = false;
        }

        public void DisplayField(ICustomVariableLayout layout) => layout.VariableField(this);
        public void Dispose()
        {
            SceneAssistantManager.OnSceneAssistantReset -= HandleReset;
        }
    }
}
