using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using Naninovel.UI;

namespace NaninovelSceneAssistant
{
    public interface IUIData
    {
        string Name { get; }
        Func<bool, UniTask> ChangeVisibility { get; }
        void DisplayField(IUILayout layout);
        GameObject GameObject { get; }
        string ModalGroup { get; }
        bool Visible { get; }
    }

    public class UIData<TUI> : IUIData where TUI : CustomUI
    {
        public virtual string Name { get; set; }
        public string ModalGroup { get; }
        public Func<bool, UniTask> ChangeVisibility { get; }
        public bool Visible => UI.Visible;
        public GameObject GameObject => UI.gameObject;
        protected TUI UI { get; }

        private SceneAssistantManager sceneAssistantManager;

        public UIData(TUI ui)
        {
            Name = ui.ToString().GetBefore(" (");
            UI = ui;
            ModalGroup = ui.ModalGroup;

            ChangeVisibility = ChangeVisibilityAsync;

            sceneAssistantManager = Engine.GetService<SceneAssistantManager>();
        }

        protected virtual async UniTask ChangeVisibilityAsync(bool visible)
        {
            await UI.ChangeVisibility(visible, null).ContinueWith(ClearAndResetUIData);
        }

        protected virtual void ClearAndResetUIData()
        {
            sceneAssistantManager.UIDataList.Clear();
            sceneAssistantManager.ModalUIDataList.Clear();

            sceneAssistantManager.ResetUIDataList();
        }

        protected virtual void Show() => UI.ChangeVisibility(true, null).Forget();
        protected virtual void Hide() => UI.ChangeVisibility(false, null).Forget();

        public virtual void DisplayField(IUILayout layout) => layout.UIField(this);
    }

    public class SaveUIData : UIData<SaveLoadMenu>
    {
        public SaveUIData(SaveLoadMenu ui) : base(ui) 
        {
            Name = "SaveLoadUI (Save)";
        }

        protected override async UniTask ChangeVisibilityAsync(bool visible)
        {
            UI.PresentationMode = SaveLoadUIPresentationMode.Save;
            await base.ChangeVisibilityAsync(visible);
        }
    }

    public class LoadUIData : UIData<SaveLoadMenu>
    {
        public LoadUIData(SaveLoadMenu ui) : base(ui)
        {
            Name = "SaveLoadUI (Load)";
        }

        protected override async UniTask ChangeVisibilityAsync(bool visible)
        {
            UI.PresentationMode = SaveLoadUIPresentationMode.Load;
            await base.ChangeVisibilityAsync(visible);
        }
    }

    public class QuickLoadUIData : UIData<SaveLoadMenu>
    {
        public QuickLoadUIData(SaveLoadMenu ui) : base(ui)
        {
            Name = "SaveLoadUI (QuickLoad)";
        }

        protected override async UniTask ChangeVisibilityAsync(bool visible)
        {
            UI.PresentationMode = SaveLoadUIPresentationMode.QuickLoad;
            await base.ChangeVisibilityAsync(visible);
        }
    }


    public class ToastUIData : UIData<ToastUI>
    {
        public ToastUIData(ToastUI ui) : base(ui) {}

        protected override async UniTask ChangeVisibilityAsync(bool visible)
        {
            UI.Show("Sample text");
            await base.ChangeVisibilityAsync(visible);
        }
    }



    public class TextPrinterUIData : UIData<UITextPrinterPanel>
    {
        public TextPrinterUIData(UITextPrinterPanel ui) : base(ui) { }
    }

    public class ChoiceHandlerUIData : UIData<ChoiceHandlerPanel>
    {
        public ChoiceHandlerUIData(ChoiceHandlerPanel ui) : base(ui) { }

    }

}

