using System;
using System.Collections.Generic;
using Naninovel;
using Naninovel.Commands;

namespace NaninovelSceneAssistant
{
	public class CharacterData : OrthoActorData<CharacterManager, ICharacterActor, CharacterMetadata, CharactersConfiguration, CharacterState, CharacterMetadata.Pose>
	{
		public CharacterData(string id) : base(id) { }
		
		protected const string CharacterName = "Character", CharacterCommandName = "char";
		public static string TypeId => CharacterName;
		protected override string CommandNameAndId => $"{CharacterCommandName} {Id}";
		
		#if UNITY_EDITOR
		protected override List<CharacterMetadata.Pose>[] Poses => new List<CharacterMetadata.Pose>[] { Metadata.Poses, EditorConfig.GetMetadataOrDefault(Id).Poses };
		protected override List<CharacterMetadata.Pose>[] SharedPoses => new List<CharacterMetadata.Pose>[] { Config.SharedPoses, EditorConfig.SharedPoses };
		public override void ApplyPose(string poseName) => new ModifyCharacter{ Id = Id, Pose = poseName, Duration = 0f }.Execute().Forget();
		#endif
		
		protected override void AddCommandParameters()
		{			
			AddBaseParameters();
			CommandParameters.Add(new CommandParameterData<Enum>(Look, () => Actor.LookDirection, v => Actor.LookDirection = (CharacterLookDirection)v, (i,p) => i.EnumDropdownField(p), defaultValue: Metadata.BakedLookDirection));
		}
	}
}