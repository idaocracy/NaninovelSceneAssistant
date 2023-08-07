using System.Collections.Generic;
using Naninovel;
using Naninovel.Commands;

namespace NaninovelSceneAssistant
{
	public class BackgroundData : OrthoActorData<BackgroundManager, IBackgroundActor, BackgroundMetadata, BackgroundsConfiguration, BackgroundState, BackgroundMetadata.Pose>
	{
		public BackgroundData(string id) : base(id) { }
		public static string TypeId => "Background";
		protected override string CommandNameAndId => $"back id:{Id}";
		
		#if UNITY_EDITOR
		
		protected override List<List<BackgroundMetadata.Pose>> Poses => new List<List<BackgroundMetadata.Pose>> { Metadata.Poses, EditorConfig.GetMetadataOrDefault(Id).Poses };
		protected override List<List<BackgroundMetadata.Pose>> SharedPoses => new List<List<BackgroundMetadata.Pose>> { Config.SharedPoses, EditorConfig.SharedPoses };
		public override void ApplyPose(string poseName) => new ModifyBackground{ Id = Id, Pose = poseName, Duration = 0f }.ExecuteAsync().Forget();
		#endif
		
		protected override void AddCommandParameters()
		{
			AddBaseParameters();
		}
	}
}