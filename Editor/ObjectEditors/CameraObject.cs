using Naninovel;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace NaninovelSceneAssistant
{
    public class CameraObject<TCameraManager> : NaninovelObject, INaninovelObject where TCameraManager : ICameraManager
    {
        private TCameraManager camera;
        private List<Param> paramList;

        public override string Id { get; set; }
        public override List<Param> Params => paramList;

        public CameraObject(TCameraManager cameraManager) : base()
        {
            Id = nameof(cameraManager.Camera);
            camera = cameraManager;
            InitializeParams();
        }

        public override GameObject GetGameObject() => camera.Camera.gameObject;

        public override string GetCommandNameAndId() => "camera";

        protected override void InitializeParams()
        {
            paramList = new List<Param>()
            {

                    new Param
                    {
                        Id = "Offset",
                        Value = camera.Offset,
                        OnEditor = () => camera.Offset = EditorGUILayout.Vector3Field("", camera.Offset)
                    },
                    new Param
                    {
                        Id = "Rotation",
                        Value = camera.Rotation.eulerAngles,
                        OnEditor = () => camera.Rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("", camera.Rotation.eulerAngles))
                    },
                    new Param
                    {
                        Id = "Zoom",
                        Value = camera.Zoom,
                        OnEditor = () => camera.Zoom = EditorGUILayout.Slider(camera.Zoom, 0f, 1f)
                    },
                    new Param
                    {
                        Id = "Orthographic",
                        Value = camera.Orthographic,
                        OnEditor = () => camera.Orthographic = EditorGUILayout.Toggle(camera.Orthographic)
                    },


            };
        }
    }
}
