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

        protected INaninovelObject CurrentObject { get; set; }
        protected string[] ObjectDropdown { get => sceneAssistantManager.ObjectList.Select(p => p.Id).ToArray(); }
        //protected string ClipboardString { get => clipboardString; set { clipboardString = value; EditorGUIUtility.systemCopyBuffer = value; if (logResults) Debug.Log(value); } }

        private int objectIndex = 0;
        private int tabIndex = 0;
        //private string clipboardString = string.Empty;
        private Vector2 scrollPos = default;
        private bool logResults;
        private ISceneAssistantLayout layout;

        //[SerializeField] private SliderField sliderField;
        public void InitializeObject(INaninovelObject obj)
        {
            foreach(var param in obj.Params)
            {
                param.DisplayField(layout);
            }
        }


        protected override void Awake()
        {
            base.Awake();

            layout = GetComponent<ISceneAssistantLayout>();
            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
            RefreshList();

        }

        public void RefreshList()
        {
            objectDropdown.ClearOptions();
            var objList = new List<TMP_Dropdown.OptionData>();

            foreach (var obj in ObjectDropdown)
            {
                objList.Add(new TMP_Dropdown.OptionData() { text = obj });
            }

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
            CurrentObject = sceneAssistantManager.ObjectList[objectIndex];
            InitializeObject(CurrentObject);
        }

        public void SliderField(CommandParam param, float minValue, float maxValue)
        {
            var slider = Instantiate(sliderField, content.transform);
            slider.Init(param.Id, param);        }

        public void Vector3Field(CommandParam param)
        {
            throw new System.NotImplementedException();
        }

        public void BoolField(CommandParam param)
        {
            throw new System.NotImplementedException();
        }

        public void StringField(CommandParam value)
        {
            throw new System.NotImplementedException();
        }

        public void ColorField(CommandParam value)
        {
            throw new System.NotImplementedException();
        }

        public void FloatField(CommandParam param)
        {
            throw new System.NotImplementedException();
        }

        public void Vector2Field(CommandParam param)
        {
            throw new System.NotImplementedException();
        }

        public void StringListField(CommandParam param, string[] stringValues)
        {
            throw new System.NotImplementedException();
        }

        public void IntField(CommandParam param)
        {
            throw new System.NotImplementedException();
        }

        public void EnumField(CommandParam param)
        {
            throw new System.NotImplementedException();
        }

        public void Vector4Field(CommandParam param)
        {
            throw new System.NotImplementedException();
        }

        public void PosField(CommandParam param)
        {
            throw new System.NotImplementedException();
        }
    }


}