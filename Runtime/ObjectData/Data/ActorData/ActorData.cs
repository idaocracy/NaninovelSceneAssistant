using Naninovel;
using UnityEngine;
using System.Linq;

namespace NaninovelSceneAssistant
{
	public abstract class ActorData<TService, TActor, TMeta, TConfig> : NaninovelObjectData<TService, TConfig>, INaninovelObjectData
		where TService : class, IActorManager
		where TActor : IActor
		where TMeta : ActorMetadata
		where TConfig : ActorManagerConfiguration<TMeta>
	{
		public ActorData(string id) : base()
		{
			this.id = id;
			Initialize();
		}

		public override string Id => id;
		public override GameObject GameObject => GetGameObject();
		protected TActor Actor => (TActor)Service.GetActor(Id);
		protected TMeta Metadata => Config.GetMetadataOrDefault(Id);

		protected virtual float? DefaultZOffset { get; }
		private string id;

		protected GameObject GetGameObject()
		{
			var monoActor = Actor as MonoBehaviourActor<TMeta>;
			return monoActor.GameObject;
		}
		
		protected abstract void GetAppearanceData();

		protected virtual void AddBaseParameters(bool includeAppearance = true, bool includeTint = true, bool includeTransform = true, bool includeZPos = true)  
		{
			if (includeAppearance) GetAppearanceData();
			if (includeTransform)
			{
				ICommandParameterData pos = null;
				ICommandParameterData position = null;

				CommandParameters.Add(position = new CommandParameterData<Vector3>("Position", () => Actor.Position, v => Actor.Position = v, (i, p) => i.Vector3Field(p, toggleGroup:pos), defaultValue: new Vector3(0,-5.4f, DefaultZOffset ?? 0)));
				CommandParameters.Add(pos = new CommandParameterData<Vector3>("Pos", () => Actor.Position, v => Actor.Position = v, (i, p) => i.PosField(p, CameraConfiguration, toggleGroup:position), defaultValue: new Vector3(0, -5.4f, DefaultZOffset ?? 0)));
				CommandParameters.Add(new CommandParameterData<Vector3>("Rotation", () => Actor.Rotation.eulerAngles, v => Actor.Rotation = Quaternion.Euler(v), (i, p) => i.Vector3Field(p)));
				CommandParameters.Add(new CommandParameterData<Vector3>("Scale", () => Actor.Scale, v => Actor.Scale = v, (i, p) => i.Vector3Field(p), defaultValue: Vector3.one));
			}

			if (includeTint) CommandParameters.Add(new CommandParameterData<Color>("Tint", () => Actor.TintColor, v => Actor.TintColor = v, (i, p) => i.ColorField(p), defaultValue: Color.white));
		}
	}
}