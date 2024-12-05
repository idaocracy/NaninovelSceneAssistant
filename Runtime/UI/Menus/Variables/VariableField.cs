using UnityEngine;
using System;
using TMPro;
using Naninovel;

namespace NaninovelSceneAssistant
{
    public interface IVariableField
    {
        GameObject GameObject { get; }
    }

	public abstract class VariableField<T> : ScriptableUIBehaviour, IVariableField where T : IConvertible
    {
        [SerializeField] private Transform container;
        [SerializeField] private TextMeshProUGUI label;

        public GameObject GameObject => gameObject;
        protected TextMeshProUGUI Label => label;
        protected Transform Container => container;

        protected VariableData<T> Data;
        protected Action OnDestroyed;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroyed();
        }
    }
}
