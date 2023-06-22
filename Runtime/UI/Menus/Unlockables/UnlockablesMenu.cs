using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace NaninovelSceneAssistant
{
    public class UnlockablesMenu : SceneAssistantWindowMenu, IUnlockableLayout
    {
        [SerializeField] private UnlockableField unlockableFieldPrototype;
        [SerializeField] private Transform dataContainer;
        [SerializeField] private TMP_InputField searchField;

        protected override TMP_InputField SearchField => searchField;

        public List<UnlockableField> DataFields => dataContainer.GetComponentsInChildren<UnlockableField>().ToList();

        protected override void ClearMenu() => DataFields.ForEach(f => Destroy(f.gameObject));

        protected override void ResetMenu() => GenerateLayout(Manager.UnlockablesList);
        
        public void GenerateLayout(SortedList<string, UnlockableData> list)
        {
            foreach(var data in list.Values) data.DisplayField(this);
        }

        public void UnlockableField(UnlockableData data)
        {
            UnlockableField unlockableField = Instantiate(unlockableFieldPrototype, dataContainer);
            unlockableField.Initialize(data);
        }

        protected override void EvaluateSearch(string search)
        {
            base.EvaluateSearch(search);

            ClearMenu();
            foreach (UnlockableData unlockable in Manager.UnlockablesList.Values)
            {
                if (!string.IsNullOrEmpty(search) && unlockable.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                unlockable.DisplayField(this);
            }
        }
    }
}
