﻿using Naninovel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace NaninovelSceneAssistant
{
    public class CameraData : NaninovelObjectData<ICameraManager, CameraManager.GameState, CameraConfiguration>, INaninovelObjectData
    {
        public CameraData() => Initialize();
        protected ICameraManager CameraManager => Service;
        public override string Id => GameObject.name.Replace("(Clone)", "");
        public static string TypeId => "Camera";
        public override GameObject GameObject => CameraManager.Camera.gameObject; 
        protected override string CommandNameAndId => "camera";
        protected List<MonoBehaviour> CameraComponents => CameraManager.Camera.GetComponents<MonoBehaviour>().Where(c => c.GetType() != typeof(UnityEngine.Camera)).ToList();

        protected override void AddParams()
        {
            ParameterValue rotation = null;
            ParameterValue roll = null;

            Params.Add(new ParameterValue("Offset", () => CameraManager.Offset, v => CameraManager.Offset = (Vector3)v, () => State.Offset, (i,p) => i.Vector3Field(p)));
            Params.Add(rotation = new ParameterValue("Rotation", () => CameraManager.Rotation.eulerAngles, v => CameraManager.Rotation = Quaternion.Euler((Vector3)v), () => State.Rotation.eulerAngles, (i,p) => i.Vector3Field(p, toggleWith: roll)));
            Params.Add(roll = new ParameterValue("Roll", () => CameraManager.Rotation.eulerAngles.z, v => CameraManager.Rotation = Quaternion.Euler(0, 0, (float)v), () => State.Rotation.eulerAngles, (i,p) => i.FloatField(p, toggleWith:rotation)));
            Params.Add(new ParameterValue("Zoom", () => CameraManager.Zoom, v => CameraManager.Zoom = (float)v, () => State.Zoom, (i,p) => i.FloatSliderField(p, 0f, 1f), defaultValue:0f));
            Params.Add(new ParameterValue("Orthographic", () => CameraManager.Orthographic, v => CameraManager.Orthographic = (bool)v, () => State.Orthographic, (i,p) => i.BoolField(p), defaultValue:true));
            AddCameraComponentParams();
        }

        private void AddCameraComponentParams()
        {
            if (CameraComponents.Count <= 0) return;

            Params.Add(new ParameterValue("Toggle", () => CameraComponents.ToDictionary(c => c.GetType().Name, e => e.enabled.ToString().ToLower()), null, null, (i, p) => i.EmptyField(p)));
            int a = 0;

            foreach (var component in CameraComponents)
            {
                Params.Add(new ParameterValue(component.GetType().Name, 
                    () => component.enabled, v => component.enabled = (bool)v, () => State.CameraComponents[a], (l, p) => l.BoolField(p), isParameter:false));
                a++;
            }
        }

    }
}
