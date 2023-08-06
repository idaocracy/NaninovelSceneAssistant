using System;
using System.Collections.Generic;
using Naninovel;
using Naninovel.Commands;

namespace NaninovelSceneAssistant
{
	public class CharacterData : OrthoActorData<CharacterManager, ICharacterActor, CharacterMetadata, CharactersConfiguration, CharacterState, CharacterMetadata.Pose>
	{
		public CharacterData(string id) : base(id) { }
		public static string TypeId => "Character";
		protected override string CommandNameAndId => "char " + Id;
		protected override float? DefaultZOffset => Config.ZOffset;
		
		#if UNITY_EDITOR
		protected override List<List<CharacterMetadata.Pose>> Poses => new List<List<CharacterMetadata.Pose>> { Metadata.Poses, EditorConfig.GetMetadataOrDefault(Id).Poses };
		protected override List<List<CharacterMetadata.Pose>> SharedPoses => new List<List<CharacterMetadata.Pose>> { Config.SharedPoses, EditorConfig.SharedPoses };
		
		public override void ApplyPose(string name) => new ModifyCharacter{ Id = Id, Pose = name, Duration = 0f }.ExecuteAsync().Forget();

		#endif
		
		protected override void AddCommandParameters()
		{
			AddBaseParameters();
			CommandParameters.Add(new CommandParameterData<Enum>("Look", () => Actor.LookDirection, v => Actor.LookDirection = (CharacterLookDirection)v, (i,p) => i.EnumDropdownField(p), defaultValue: Metadata.BakedLookDirection));
		}
	}
	
}