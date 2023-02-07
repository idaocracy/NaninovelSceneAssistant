using Naninovel;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace NaninovelSceneAssistant
{
    public class CameraObject : NaninovelObject<ICameraManager>, INaninovelObject
    {
        public CameraObject() => Initialize();

        protected ICameraManager Camera { get => Engine.GetService<ICameraManager>(); }
        public override GameObject GameObject { get => Camera.Camera.gameObject; }
        public override string Id { get => GameObject.name; set => Id = value; }

        protected override string CommandNameAndId => "camera";

        protected override void AddParams()
        {
            //Params.Add(new CommandParam("Offset", () => Camera.Offset, v => Camera.Offset = (Vector3)v, (i,p) => i.Vector3Field(p)));
            //Params.Add(new CommandParam ("Rotation", () => Camera.Rotation.eulerAngles, v => Camera.Rotation = Quaternion.Euler((Vector3)v), (i,p) => i.Vector3Field(p)));
            Params.Add(new CommandParam("Zoom", () => Camera.Zoom, v => Camera.Zoom = (float)v, (i,p) => i.SliderField(p, 0f, 1f)));
            //Params.Add(new CommandParam("Orthographic", () => Camera.Orthographic, v => Camera.Orthographic = (bool)v, (i,p) => i.BoolField(p)) { DefaultValue = true } );


            
        }
    }
}
