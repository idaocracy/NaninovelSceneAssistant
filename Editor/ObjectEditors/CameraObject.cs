using Naninovel;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;


namespace NaninovelSceneAssistant
{
    public class CameraObject : NaninovelObject<ICameraManager>, INaninovelObject
    {
        public CameraObject() => Initialize();

        protected ICameraManager CameraManager { get => EngineService; }
        public override GameObject GameObject { get => CameraManager.Camera.gameObject; }
        public override string Id { get => GameObject.name.Replace("(Clone)",""); set => Id = value; }
        protected override string CommandNameAndId => "camera";
        protected List<MonoBehaviour> CameraComponents { get => CameraManager.Camera.GetComponents<MonoBehaviour>().Where(c => c.GetType() != typeof(Camera)).ToList(); }


        protected override void AddParams()
        {
            Params.Add(new CommandParam("Offset", () => CameraManager.Offset, v => CameraManager.Offset = (Vector3)v, (i,p) => i.Vector3Field(p)));
            Params.Add(new CommandParam("Rotation", () => CameraManager.Rotation.eulerAngles, v => CameraManager.Rotation = Quaternion.Euler((Vector3)v), (i,p) => i.Vector3Field(p)));
            Params.Add(new CommandParam("Zoom", () => CameraManager.Zoom, v => CameraManager.Zoom = (float)v, (i,p) => i.SliderField(p, 0f, 1f)));
            Params.Add(new CommandParam("Orthographic", () => CameraManager.Orthographic, v => CameraManager.Orthographic = (bool)v, (i,p) => i.BoolField(p)) { DefaultValue = true } );
            AddCameraComponentParams();
        }

        private void AddCameraComponentParams()
        {
            Params.Add(new CommandParam("Toggle", () => GetNamedValues(), null, (i, p) => { }));

            foreach (var component in CameraComponents)
            {
                Params.Add(new CommandParam(component.GetType().Name, 
                    () => component.enabled, v => component.enabled = (bool)v, (l, p) => l.BoolField(p)) { HasCommandOptions = false });
            }

        }

        private Dictionary<string,string> GetNamedValues() 
        {
            var namedBools = new Dictionary<string, string>();
            foreach (var component in CameraComponents) namedBools.Add(component.GetType().Name, component.enabled.ToString().ToLower());
            return namedBools;
        }

    }
}
