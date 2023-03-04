using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Naninovel.UI;
using Naninovel;
using TMPro;

namespace NaninovelSceneAssistant
{
    public class SceneAssistantUI : CustomUI, ISceneAssistantLayout
    {
        [SerializeField] private GameObject content;
        [SerializeField] private TMP_Dropdown objectDropdown;
        [SerializeField] private SliderField sliderField;

        private SceneAssistantManager sceneAssistantManager;

        protected INaninovelObjectData CurrentObject { get; set; }
        //protected string[] ObjectDropdown { get => sceneAssistantManager.ObjectList.Select(p => p.Id).ToArray(); }
        //protected string ClipboardString { get => clipboardString; set { clipboardString = value; EditorGUIUtility.systemCopyBuffer = value; if (logResults) Debug.Log(value); } }

        private int objectIndex = 0;
        private int tabIndex = 0;
        //private string clipboardString = string.Empty;
        private Vector2 scrollPos = default;
        private bool logResults;
        private ISceneAssistantLayout layout;

        //[SerializeField] private SliderField sliderField;
        public void InitializeObject(INaninovelObjectData obj)
        {
            //foreach(var param in obj.Params)
            //{
            //    param.DisplayField(layout);
            //}
        }


        protected override void Awake()
        {
            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();

            layout = GetComponent<ISceneAssistantLayout>();
            RefreshList();

            base.Awake();


        }

        public void RefreshList()
        {
            objectDropdown.ClearOptions();
            var objList = new List<TMP_Dropdown.OptionData>();

            //foreach (var obj in ObjectDropdown)
            //{
            //    objList.Add(new TMP_Dropdown.OptionData() { text = obj });
            //}

            objectDropdown.AddOptions(objList);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            objectDropdown.onValueChanged.AddListener(SetObjectIndex);
        }

        public void SetObjectIndex(int index)
        {
            objectIndex = index;
            Debug.Log(index);
            //CurrentObject = sceneAssistantManager.ObjectList.Values[objectIndex];
            InitializeObject(CurrentObject);
        }

        public void FloatSliderField(ParameterValue param, float minValue, float maxValue, ParameterValue toggleWith = null)
        {
            var slider = Instantiate(sliderField, content.transform);
            slider.Init(param.Name, param);        }

        public void Vector3Field(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void BoolField(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void StringField(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void ColorField(ParameterValue param, bool includeAlpha = true, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void FloatField(ParameterValue param, float? minValue = null, float? maxValue = null, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void Vector2Field(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void StringListField(ParameterValue param, string[] stringValues, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void IntField(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void EnumField(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void Vector4Field(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void PosField(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new System.NotImplementedException();
        }

        public void VariableField(VariableValue var)
        {
            throw new System.NotImplementedException();
        }

        public void UnlockableField(UnlockableValue unlockable)
        {
            throw new System.NotImplementedException();
        }

        public void IntField(ParameterValue param, int? minValue, int? maxValue, ParameterValue toggleWith = null)
        {
            throw new NotImplementedException();
        }

        public void ShowValueOptions(ParameterValue param)
        {
            throw new NotImplementedException();
        }

        public void EmptyField(ParameterValue param, ParameterValue toggleWith = null)
        {
            throw new NotImplementedException();
        }

        public void IntSliderField(ParameterValue param, int minValue, int maxValue, ParameterValue toggleWith = null)
        {
            throw new NotImplementedException();
        }

        public void TypeListField<T>(ParameterValue param, string[] stringValues, T[] typeValues, ParameterValue toggleWith = null)
        {
            throw new NotImplementedException();
        }

        public void TypeListField<T>(ParameterValue param, Dictionary<string, T> values, ParameterValue toggleWith = null)
        {
            throw new NotImplementedException();
        }

        public void PosField(ParameterValue param, CameraConfiguration cameraConfiguration, ParameterValue toggleWith = null)
        {
            throw new NotImplementedException();
        }

        public void Vector3Field(ParameterValue param, bool ignoreZPos = false, ParameterValue toggleWith = null)
        {
            throw new NotImplementedException();
        }

        public void PosField(ParameterValue param, CameraConfiguration cameraConfiguration, bool includeZPos = true, ParameterValue toggleWith = null)
        {
            throw new NotImplementedException();
        }
    }


}