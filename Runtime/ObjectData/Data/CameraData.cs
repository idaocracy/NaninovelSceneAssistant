using Naninovel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NaninovelSceneAssistant
{
	public class CameraData : NaninovelObjectData<ICameraManager, CameraConfiguration>, INaninovelObjectData
	{
		public CameraData() => Initialize();
		
		protected const string CameraName = "Camera", CameraCommandName = "camera";
		public static string TypeId => CameraName;
		protected override string CommandNameAndId => CameraCommandName;
		public override string Id => GameObject.name.Replace("(Clone)", "");
		protected ICameraManager CameraManager => Service;
		public override GameObject GameObject => CameraManager.Camera.gameObject; 
		protected List<MonoBehaviour> CameraComponents => CameraManager.Camera.GetComponents<MonoBehaviour>().Where(c => c.GetType() != typeof(Camera)).ToList();
		protected override void AddCommandParameters()
		{
			ICommandParameterData rotationData = null;
			ICommandParameterData rollData = null;

			CommandParameters.Add(new CommandParameterData<Vector3>(Offset, () => CameraManager.Offset, v => CameraManager.Offset = v, (i,p) => i.Vector3Field(p)));
			CommandParameters.Add(rotationData = new CommandParameterData<Vector3>(Rotation, () => CameraManager.Rotation.eulerAngles, v => CameraManager.Rotation = Quaternion.Euler(v), (i,p) => i.Vector3Field(p, toggleGroup: rollData)));
			CommandParameters.Add(rollData = new CommandParameterData<float>(Roll, () => CameraManager.Rotation.eulerAngles.z, v => CameraManager.Rotation = Quaternion.Euler(CameraManager.Rotation.eulerAngles.x, CameraManager.Rotation.eulerAngles.y, v), (i,p) => i.FloatField(p, toggleGroup:rotationData)));
			CommandParameters.Add(new CommandParameterData<float>(Zoom, () => CameraManager.Zoom, v => CameraManager.Zoom = (float)v, (i,p) => i.FloatSliderField(p, 0f, 1f), defaultValue:0f));
			CommandParameters.Add(new CommandParameterData<bool>(Orthographic, () => CameraManager.Orthographic, v => CameraManager.Orthographic = (bool)v, (i,p) => i.BoolField(p), defaultValue:true));
			AddCameraComponentParams();
		}

		private void AddCameraComponentParams()
		{
			if (CameraComponents.Count <= 0) return;

			var componentsData = new List<ICommandParameterData>();

			foreach (var component in CameraComponents)
			{
				//Ignore all built-in spawn effects 
				if(component.GetType().Namespace == "Naninovel.FX") continue; 
				else componentsData.Add(new NamedCommandParameterData<bool>(component.GetType().Name, component.GetType().Name, () => component.enabled, v => component.enabled = v, (i, p) => i.BoolField(p), defaultValue:component.enabled));
			}

			CommandParameters.Add(new ListCommandData("Set", componentsData, (i, p) => i.ListField(p)));
		}
	}
}
