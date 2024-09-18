using System;
using System.Collections.Generic;
using System.Linq;
using Naninovel;
using UnityEngine;

namespace  NaninovelSceneAssistant
{
    public class ScriptData
    {
        public string Name { get; }
        public Script Value { get; }

        public List<string> Labels { get => getLabels(); } 

        protected IScriptManager ScriptManager = Engine.GetService<IScriptManager>();
        protected IResourceProviderManager ResourceProviderManager = Engine.GetService<IResourceProviderManager>();

        private readonly Func<List<string>> getLabels;

        public ScriptData(Script value)
        {
            Name = value.name;
            Value = value;

            getLabels = () => Value.Lines
                .OfType<LabelScriptLine>()
                .Where(l => !string.IsNullOrWhiteSpace(l.LabelText))
                .Select(l => l.LabelText.Trim()).ToList();
        }
        
        public void DisplayField(IScriptLayout layout) => layout.ScriptField(Name, Labels);
    
    }
}

