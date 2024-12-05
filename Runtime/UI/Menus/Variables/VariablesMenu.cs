using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NaninovelSceneAssistant
{
	public class VariablesMenu : SceneAssistantWindowMenu, ICustomVariableLayout
	{
		[SerializeField] private Transform dataContainer;
		[SerializeField] private TMP_InputField searchField;

        [SerializeField] private StringVariableField stringFieldPrototype;
        [SerializeField] private BoolVariableField boolFieldProtoype;
        [SerializeField] private NumericVariableField numericFieldPrototype;

        protected override TMP_InputField SearchField => searchField;
		public List<IVariableField> DataFields => dataContainer.GetComponentsInChildren<IVariableField>().ToList();

		protected override void ClearMenu() => DataFields.ForEach(f => Destroy(f.GameObject));
		protected override void ResetMenu() 
		{
			GenerateLayout(Manager.VariableDataList);
		}

        public void GenerateLayout(SortedList<string, IVariableData> list)
        {
            foreach (var data in list.Values) data.DisplayField(this);
        }

        protected override void EvaluateSearch(string search)
		{
			base.EvaluateSearch(search);

			ClearMenu();
            foreach (IVariableData variable in Manager.VariableDataList.Values)
            {
                if (!string.IsNullOrEmpty(search) && variable.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                variable.DisplayField(this);
            }
        }

        public void StringVariableField(StringVariableData variable)
        {
            StringVariableField variableField = Instantiate(stringFieldPrototype, dataContainer);
            variableField.InitializeStringValueField(variable);
        }

        public void BooleanVariableField(BooleanVariableData variable)
        {
            BoolVariableField variableField = Instantiate(boolFieldProtoype, dataContainer);
            variableField.InitializeBoolValueField(variable);
        }

        public void NumericVariableField(NumericVariableData variable)
        {
            NumericVariableField variableField = Instantiate(numericFieldPrototype, dataContainer);
            variableField.InitializeNumericValueField(variable);
        }
    }
}
