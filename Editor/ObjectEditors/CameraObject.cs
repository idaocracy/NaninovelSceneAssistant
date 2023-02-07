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
            Params.Add(new CommandParam("Offset", () => Camera.Offset, p => Camera.Offset = (Vector3)p, () => Camera.Offset = EditorGUILayout.Vector3Field("", Camera.Offset)));;
            Params.Add(new CommandParam ("Rotation", () => Camera.Rotation, p => Camera.Rotation = (Quaternion)p, () => Camera.Rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("", Camera.Rotation.eulerAngles))));
            Params.Add(new CommandParam("Zoom", () => Camera.Zoom, p => Camera.Zoom = (float)p, () => Camera.Zoom = EditorGUILayout.Slider(Camera.Zoom, 0f, 1f)));
            Params.Add(new CommandParam("Orthographic", () => Camera.Orthographic, p => Camera.Orthographic = (bool)p, () => Camera.Orthographic = EditorGUILayout.Toggle(Camera.Orthographic)) { DefaultValue = true } );


            
        }
    }
}
