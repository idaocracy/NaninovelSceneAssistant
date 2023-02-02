using Naninovel;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace NaninovelSceneAssistant
{
    public class CameraObject<TCameraManager> : NaninovelObject where TCameraManager : ICameraManager
    {
        public CameraObject(TCameraManager camera) : base() 
        { 
            Camera = camera; 
            AddParams();
        }

        protected TCameraManager Camera { get; private set; }
        public override string Id { get => nameof(Camera.Camera); }
        public override GameObject GameObject { get => Camera.Camera.gameObject; }
        protected override string GetCommandNameAndId() => "camera";

        protected override void AddParams()
        {
            Params.Add(new Param { Id = "Offset", Value = Camera.Offset,OnEditor = () => Camera.Offset = EditorGUILayout.Vector3Field("", Camera.Offset) });
            Params.Add(new Param { Id = "Rotation", Value = Camera.Rotation.eulerAngles, OnEditor = () => Camera.Rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("", Camera.Rotation.eulerAngles))});
            Params.Add(new Param { Id = "Zoom", Value = Camera.Zoom, OnEditor = () => Camera.Zoom = EditorGUILayout.Slider(Camera.Zoom, 0f, 1f) });
            Params.Add(new Param { Id = "Orthographic", Value = Camera.Orthographic, OnEditor = () => Camera.Orthographic = EditorGUILayout.Toggle(Camera.Orthographic) });

        }
    }
}
