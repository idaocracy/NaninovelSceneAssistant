using Naninovel;
using UnityEngine;

namespace NaninovelSceneAssistant
{
    public class CopyCommandButton : ScriptableButton
    {
        [SerializeField] private CopyCommandType copyCommandType;
        [SerializeField] private bool inlined;
        private SceneAssistantMenu sceneAssistantMenu;
        private SceneAssistantManager sceneAssistantManager;

        enum CopyCommandType { CopyCommand, CopySelected, CopyAll };

        protected override void Awake()
        {
            base.Awake();
            sceneAssistantMenu = GetComponentInParent<SceneAssistantMenu>();
            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
        }
        protected override void OnButtonClick()
        {
            base.OnButtonClick();

            var currentObject = sceneAssistantMenu.CurrentObject;
            var objectList = sceneAssistantManager.ObjectList;
            var objectTypeList = sceneAssistantManager.ObjectTypeList;

            if (copyCommandType == CopyCommandType.CopyCommand) sceneAssistantMenu.CopyToBuffer(currentObject.GetCommandLine(inlined));
            else if (copyCommandType == CopyCommandType.CopySelected) sceneAssistantMenu.CopyToBuffer(string.Join(inlined ? "" : "\n", currentObject.GetAllCommands(objectList, objectTypeList, inlined, selected:true)));
            else if (copyCommandType == CopyCommandType.CopyAll) sceneAssistantMenu.CopyToBuffer(string.Join(inlined ? "" : "\n", currentObject.GetAllCommands(objectList, objectTypeList, inlined, selected:false)));

        }
    }
}