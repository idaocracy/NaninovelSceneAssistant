using System.Collections.Generic;
using Naninovel;
using Naninovel.Commands;

namespace NaninovelSceneAssistant
{
	public class BackgroundData : OrthoActorData<BackgroundManager, IBackgroundActor, BackgroundMetadata, BackgroundsConfiguration, BackgroundState, BackgroundMetadata.Pose>
	{
		public BackgroundData(string id) : base(id) { }
		
		protected const string BackgroundName = "Background", BackgroundCommandName = "back";
		public static string TypeId => BackgroundName;
		protected override string CommandNameAndId => $"{BackgroundCommandName} id:{Id}";
		
		#if UNITY_EDITOR
		protected override List<BackgroundMetadata.Pose>[] Poses => new List<BackgroundMetadata.Pose>[] { Metadata.Poses, EditorConfig.GetMetadataOrDefault(Id).Poses };
		protected override List<BackgroundMetadata.Pose>[] SharedPoses => new List<BackgroundMetadata.Pose>[] { Config.SharedPoses, EditorConfig.SharedPoses };
		public override void ApplyPose(string poseName) => new ModifyBackground{ Id = Id, Pose = poseName, Duration = 0f }.Execute().Forget();
		#endif
		
		protected override void AddCommandParameters()
		{
			AddBaseParameters();
		}
	}
}