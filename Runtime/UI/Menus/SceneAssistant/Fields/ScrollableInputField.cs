using Naninovel;
using UnityEngine;

namespace NaninovelSceneAssistant
{
	public class ScrollableInputField : SceneAssistantDataField<ScrollableFloatValueField>
	{
		[SerializeField] private ScrollableFloatValueField valueFieldPrototype;
		[SerializeField] private Transform container;
		private bool isPos;
		private CameraConfiguration cameraConfiguration;

		protected ScrollableFloatValueField[] FloatFields => GetComponentsInChildren<ScrollableFloatValueField>();

		public void Initialize<T>(ICommandParameterData<T> data, float? min = null, float? max = null, CameraConfiguration cameraConfiguration = null, bool isPos = false, params ToggleGroupData[] toggleGroup)
		{
			InitializeBaseData(data, toggleGroup);
			this.cameraConfiguration = cameraConfiguration;
			this.isPos = isPos;

			if (data is ICommandParameterData<float> floatData)
			{
				var floatField = Instantiate(valueFieldPrototype, container);
				floatField.Initialize(() => floatField.text = floatData.Value.ToString("0.###"), v => floatData.Value = v, min:min, max:max);
			}
			else if (data is ICommandParameterData<int> intData)
			{
				var intField = Instantiate(valueFieldPrototype, container);
				intField.Initialize(() => intField.text = intData.Value.ToString(), v => intData.Value = (int)v, wholeNumbers: true, min:min, max:max);
			}
			else if (data is ICommandParameterData<Vector2> vector2Data) InitializeVector2Fields(vector2Data);
			else if (data is ICommandParameterData<Vector3> vector3Data) InitializeVector3Fields(vector3Data);
			else if (data is ICommandParameterData<Vector4> vector4Data) InitializeVector4Fields(vector4Data);
		}

		private void InitializeVector2Fields(ICommandParameterData<Vector2> data)
		{
			var vectorXField = Instantiate(valueFieldPrototype, container);
			var vectorYField = Instantiate(valueFieldPrototype, container);

			vectorXField.Initialize(() => vectorXField.FloatValue = GetVectorValue(data.Value).x, v => data.Value = SetVectorValue(new Vector2(v, data.Value.y), true), label:"x");
			vectorYField.Initialize(() => vectorYField.FloatValue = GetVectorValue(data.Value).y, v => data.Value = SetVectorValue(new Vector2(data.Value.x, v), false), label:"y");
		}

		private void InitializeVector3Fields(ICommandParameterData<Vector3> data)
		{   
			var vectorXField = Instantiate(valueFieldPrototype, container);
			var vectorYField = Instantiate(valueFieldPrototype, container);
			var vectorZField = Instantiate(valueFieldPrototype, container);

			vectorXField.Initialize(() => vectorXField.FloatValue = GetVectorValue(data.Value).x, v => data.Value = SetVectorValue(new Vector3(v, data.Value.y, data.Value.z), true), label:"x");
			vectorYField.Initialize(() => vectorYField.FloatValue = GetVectorValue(data.Value).y, v => data.Value = SetVectorValue(new Vector3(data.Value.x, v, data.Value.z), false), label:"y");
			vectorZField.Initialize(() => vectorZField.FloatValue = GetVectorValue(data.Value).z, v => data.Value = new Vector3(data.Value.x, data.Value.y, v), label:"z");
		}
		private void InitializeVector4Fields(ICommandParameterData<Vector4> data)
		{
			var vectorXField = Instantiate(valueFieldPrototype, container);
			var vectorYField = Instantiate(valueFieldPrototype, container);
			var vectorZField = Instantiate(valueFieldPrototype, container);
			var vectorWField = Instantiate(valueFieldPrototype, container);

			vectorXField.Initialize(() => vectorXField.FloatValue = GetVectorValue(data.Value).x, v => data.Value = SetVectorValue(new Vector4(v, data.Value.y, data.Value.z, data.Value.w), true), label:"x");
			vectorYField.Initialize(() => vectorYField.FloatValue = GetVectorValue(data.Value).y, v => data.Value = SetVectorValue(new Vector4(data.Value.x, v, data.Value.z, data.Value.w), false), label:"y");
			vectorZField.Initialize(() => vectorZField.FloatValue = GetVectorValue(data.Value).z, v => data.Value = new Vector4(data.Value.x, data.Value.y, v, data.Value.w), label:"z");
			vectorWField.Initialize(() => vectorWField.FloatValue = GetVectorValue(data.Value).w, v => data.Value = new Vector4(data.Value.x, data.Value.y, data.Value.z, v), label:"w");
		}

		private Vector4 GetVectorValue(Vector4 value)
		{
			if (isPos)
			{
				return new Vector4
				{
					x = (cameraConfiguration.WorldToSceneSpace(value).x * 100),
					y = (cameraConfiguration.WorldToSceneSpace(value).y * 100),
					z = value.z,
					w = value.w
				};
			}
			else return value;
		}

		private Vector4 SetVectorValue(Vector4 value, bool XorY)
		{
			if (isPos)
			{
				return new Vector4
				{
					x = XorY ? (cameraConfiguration.SceneToWorldSpace(value / 100).x) : value.x,
					y = XorY ? value.y : (cameraConfiguration.SceneToWorldSpace(value / 100).y),
					z = value.z,
					w = value.w
				};
			}
			else return value;
		}

		public override void GetDataValue()
		{
			foreach (var field in FloatFields) field.GetDataValue();
			CheckConditions();
		}
	}
}