using System.Collections.Generic;
using UnityEngine;
using Naninovel;
using System.Linq;

namespace NaninovelSceneAssistant
{
	public class SpawnData : NaninovelObjectData<ISpawnManager, SpawnConfiguration>, INaninovelObjectData
	{
		public SpawnData(string id)
		{
			this.id = id;
			Initialize();
		}

		protected const string SpawnName = "Spawn", SpawnCommandName = "spawn";
		protected SpawnedObject Spawned => Engine.GetService<SpawnManager>().GetSpawned(Id);

		public override string Id => id;
		public static string TypeId => SpawnName;
		protected Transform Transform => Spawned.Transform;
		public override GameObject GameObject => Spawned.GameObject;
		protected ISceneAssistantSpawn SpawnSceneAssistant => GameObject.GetComponent<ISceneAssistantSpawn>() ?? null;
		protected override string CommandNameAndId => $"{SpawnCommandName} {Id}";
		protected bool IsTransformable => SpawnSceneAssistant?.IsTransformable ?? true;

		private string id;

		public override string GetCommandLine(bool inlined = false, bool paramsOnly = false)
		{
			if (SpawnSceneAssistant != null && SpawnSceneAssistant.IsSpawnEffect) return GetSpawnEffectLine(inlined, paramsOnly);
			else return GetSpawnLine(inlined, paramsOnly);
		}

		public virtual string GetSpawnLine(bool inlined = false, bool paramsOnly = false)
		{
			if (CommandParameters == null)
			{
				Debug.LogWarning("No parameters found.");
				return null;
			}

			List<ICommandParameterData> tempParams;
			string transformString;
			GetTransformValues(out tempParams, out transformString);

			var paramsString = string.Join(",", tempParams.Where(p => p.GetCommandValue() != null).Select(p => p.GetCommandValue(paramOnly: true) ?? string.Empty));
			if ((CommandParameterData.ExcludeState || CommandParameterData.ExcludeDefault) && paramsString.Length == 0 && transformString.Length == 0) return null;
			if (paramsOnly) return paramsString;
			var commandString = $"{CommandNameAndId} {transformString}" + (paramsString.Length != 0 ? $"params:{paramsString}" : string.Empty);
			return inlined ? $"[{commandString}]" : $"@{commandString}";
		}

		public virtual string GetSpawnEffectLine(bool inlined = false, bool paramsOnly = false)
		{
			if (CommandParameters == null)
			{
				Debug.LogWarning("No parameters found.");
				return null;
			}

			string transformString;
			GetTransformValues(out _, out transformString);

			var list = CommandParameters.FirstOrDefault(d => d is IListCommandParameterData) as IListCommandParameterData;
			var paramString = string.Join(" ", list.Values.Where(p => p.GetCommandValue() != null).Select(p => p.GetCommandValue() ?? string.Empty));
			if ((CommandParameterData.ExcludeState || CommandParameterData.ExcludeDefault) && paramString.Length == 0) return null;
			if (paramsOnly) return paramString;
			var commandString = $"{this.GetFormattedCommandName()} {transformString}{paramString}";

			return inlined ? $"[{commandString}]" : $"@{commandString}";
		}

		private void GetTransformValues(out List<ICommandParameterData> tempParams, out string transformString)
		{
			tempParams = CommandParameters.ToList();
			transformString = string.Empty;
			if (IsTransformable)
			{
				foreach (var parameter in tempParams.ToList())
				{
					if (IsTransformData(parameter.Name))
					{
						if (parameter.GetCommandValue() != null) transformString = transformString + parameter.GetCommandValue() + " ";
						tempParams.Remove(parameter);
					}
				}
			}
		}

		protected bool IsTransformData(string name) => name == Position || name == Pos || name == Rotation || name == Scale;

		protected void AddTransformParams()
		{
			ICommandParameterData posData = null;
			ICommandParameterData positionData = null;
			ICommandParameterData vectorScaleData = null;
			ICommandParameterData uniformScaleData = null;

			CommandParameters.Add(positionData = new CommandParameterData<Vector3>(Position, () => Transform.localPosition, v => Transform.localPosition = v, (i, p) => i.Vector3Field(p, toggleGroup: new ToggleGroupData(posData, false)), defaultValue: new Vector3(0, 0, 99)));
			CommandParameters.Add(posData = new CommandParameterData<Vector3>(Pos, () => Transform.localPosition, v => Transform.localPosition = v, (i, p) => i.PosField(p, CameraConfiguration, new ToggleGroupData(positionData, false)), defaultValue: new Vector3(0, 0, 99)));
			CommandParameters.Add(new CommandParameterData<Vector3>(Rotation, () => Transform.localRotation.eulerAngles, v => Transform.localRotation = Quaternion.Euler(v), (i, p) => i.Vector3Field(p)));
			CommandParameters.Add(vectorScaleData = new CommandParameterData<Vector3>(Scale, () => Transform.localScale, v => Transform.localScale = v, (i, p) => i.Vector3Field(p, new ToggleGroupData(uniformScaleData)), defaultValue: Vector3.one));
			CommandParameters.Add(uniformScaleData = new CommandParameterData<float>(Scale, () => Transform.localScale.x, v => Transform.localScale = new Vector3(v, v, v), (i, p) => i.FloatField(p, toggleGroup: new ToggleGroupData(vectorScaleData)), defaultValue: 1f));
		}

		protected override void AddCommandParameters()
		{
			if (IsTransformable) AddTransformParams();
			if (SpawnSceneAssistant?.GetParams() != null) CommandParameters.Add(new ListCommandData(Params, SpawnSceneAssistant.GetParams(), (i, p) => i.ListField(p)));
		}
	}

	public interface ISceneAssistantSpawn : ISceneAssistantObject
	{
		bool IsTransformable { get; }
		string SpawnId { get; }
	}

	public interface ISceneAssistantObject
	{
		string CommandId { get; }
		bool IsSpawnEffect { get; }
		List<ICommandParameterData> GetParams();
	}
}