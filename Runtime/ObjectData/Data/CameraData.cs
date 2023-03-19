using Naninovel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NaninovelSceneAssistant
{
    public class CameraData : NaninovelObjectData<ICameraManager, CameraConfiguration>, INaninovelObjectData
    {
        public CameraData() => Initialize();
        protected ICameraManager CameraManager => Service;
        public override string Id => GameObject.name.Replace("(Clone)", "");
        public static string TypeId => "Camera";
        public override GameObject GameObject => CameraManager.Camera.gameObject; 
        protected override string CommandNameAndId => "camera";
        protected List<MonoBehaviour> CameraComponents => CameraManager.Camera.GetComponents<MonoBehaviour>().Where(c => c.GetType() != typeof(UnityEngine.Camera)).ToList();

        protected override void AddCommandParameters()
        {
            ICommandParameterData rotation = null;
            ICommandParameterData roll = null;

            CommandParameters.Add(new CommandParameterData<Vector3>("Offset", () => CameraManager.Offset, v => CameraManager.Offset = v, (i,p) => i.Vector3Field(p)));
            CommandParameters.Add(rotation = new CommandParameterData<Vector3>("Rotation", () => CameraManager.Rotation.eulerAngles, v => CameraManager.Rotation = Quaternion.Euler(v), (i,p) => i.Vector3Field(p, toggleWith: roll)));
            CommandParameters.Add(roll = new CommandParameterData<float>("Roll", () => CameraManager.Rotation.eulerAngles.z, v => CameraManager.Rotation = Quaternion.Euler(CameraManager.Rotation.eulerAngles.x, CameraManager.Rotation.eulerAngles.y, v), (i,p) => i.FloatField(p, toggleWith:rotation)));
            CommandParameters.Add(new CommandParameterData<float>("Zoom", () => CameraManager.Zoom, v => CameraManager.Zoom = (float)v, (i,p) => i.FloatSliderField(p, 0f, 1f), defaultValue:0f));
            CommandParameters.Add(new CommandParameterData<bool>("Orthographic", () => CameraManager.Orthographic, v => CameraManager.Orthographic = (bool)v, (i,p) => i.BoolField(p), defaultValue:true));
            AddCameraComponentParams();
        }

        private void AddCameraComponentParams()
        {
            if (CameraComponents.Count <= 0) return;

            var componentsData = new List<ICommandParameterData>();

            foreach (var component in CameraComponents)
            {
                componentsData.Add(new NamedCommandData<bool>(component.GetType().Name, component.GetType().Name, () => component.enabled, v => component.enabled = v, (i, p) => i.BoolField(p)));
            }

            CommandParameters.Add(new ListCommandData("Set", componentsData, (i, p) => i.ListButtonField(p)));
        }
    }
}
