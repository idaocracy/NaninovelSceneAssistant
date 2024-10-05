using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace NaninovelSceneAssistant
{
	public class VariablesMenu : SceneAssistantWindowMenu, ICustomVariableLayout
	{
		[SerializeField] private VariableField variableFieldPrototype;
		[SerializeField] private Transform dataContainer;
		[SerializeField] private TMP_InputField searchField;
		protected override TMP_InputField SearchField => searchField;
		public List<VariableField> DataFields => dataContainer.GetComponentsInChildren<VariableField>().ToList();

		protected override void ClearMenu() => DataFields.ForEach(f => Destroy(f.gameObject));
		protected override void ResetMenu() 
		{
			//GenerateLayout(Manager.VariableDataList);
		}
		
		//public void GenerateLayout(SortedList<string, VariableData> list)
		//{
		//	foreach(var data in list.Values) data.DisplayField(this);
		//}

		//public void VariableField(VariableData data)
		//{
		//	VariableField variableField = Instantiate(variableFieldPrototype, dataContainer);
		//	//variableField.Initialize(data);
		//}

		protected override void EvaluateSearch(string search)
		{
			base.EvaluateSearch(search);

			ClearMenu();
			//foreach (VariableData variable in Manager.VariableDataList.Values)
			//{
			//	if (!string.IsNullOrEmpty(search) && variable.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
			//	variable.DisplayField(this);
			//}
		}

        public void StringVariableField(StringVariableData variable)
        {
            throw new NotImplementedException();
        }

        public void BooleanVariableField(BooleanVariableData variable)
        {
            throw new NotImplementedException();
        }

        public void NumericVariableField(NumericVariableData variable)
        {
            throw new NotImplementedException();
        }
    }
}
