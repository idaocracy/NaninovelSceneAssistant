using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace NaninovelSceneAssistant
{
	public class ScriptsMenu : SceneAssistantWindowMenu, IScriptLayout
	{
		[SerializeField] private ScriptField scriptFieldPrototype;
		[SerializeField] private Transform dataContainer;
		[SerializeField] private TMP_InputField searchField;

		protected override TMP_InputField SearchField => searchField;

		public List<ScriptField> ScriptFields => dataContainer.GetComponentsInChildren<ScriptField>().ToList();

		protected override void ClearMenu() => ScriptFields.ForEach(f => Destroy(f.gameObject));

		protected override void ResetMenu() 
		{
			foreach(var script in Manager.ScriptDataList) ScriptField(script.Key, script.Value.Labels);
		}
		
		public void ScriptField(string name, List<string> labels)
		{
			var scriptField = Instantiate(scriptFieldPrototype, dataContainer);
			scriptField.Initialize(name);
		}

		protected override void EvaluateSearch(string search)
		{
			base.EvaluateSearch(search);

			ClearMenu();
			foreach (var script in Manager.ScriptDataList.Values)
			{
				if (!string.IsNullOrEmpty(search) && script.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
				script.DisplayField(this);
			}
		}
	}
}
