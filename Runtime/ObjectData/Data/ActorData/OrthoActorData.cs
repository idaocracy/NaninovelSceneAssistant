using UnityEngine;
using UnityEngine.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using Naninovel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaninovelSceneAssistant
{
	public interface IOrthoActorData 
	{
	#if UNITY_EDITOR
		void AddPose(string poseName);
		void AddSharedPose(string poseName);
		string[] GetPoses(); 
	 	void ApplyPose(string name);
	#endif
	}
	
	public abstract class OrthoActorData<TService, TActor, TMeta, TConfig, TState, TPose> : ActorData<TService, TActor, TMeta, TConfig>, IOrthoActorData 
		where TService : class, IActorManager
		where TActor : IActor
		where TMeta : OrthoActorMetadata
		where TConfig : OrthoActorManagerConfiguration<TMeta>
		where TState : ActorState 
		where TPose : ActorPose<TState>, new()
	{
		public OrthoActorData(string id) : base(id) {}
		
		protected override float? DefaultZOffset => Config.ZOffset;
		
		protected virtual async UniTask<string[]> GetOrthoAppearanceList()
		{
			var resourceProviderManager = Engine.GetService<IResourceProviderManager>();
			var providers = resourceProviderManager.GetProviders(Metadata.Loader.ProviderTypes);
			
			// In case you are extending the data class and want to add a dropdown list for appearances, you'll need to be explicit with typing, 
			// as otherwise the dropdown list won't appear in build. await LocateResourcesAtPathAsync<UnityEngine.Object>() will work for all types in the editor.
			if(Actor is SpriteCharacter || Actor is SpriteBackground)  return await LocateResourcesAtPathAsync<Texture2D>();
			else if(Actor is VideoCharacter || Actor is VideoBackground)  return await LocateResourcesAtPathAsync<VideoClip>();
			else return await LocateResourcesAtPathAsync<UnityEngine.Object>();
			
			async UniTask <string[]> LocateResourcesAtPathAsync<T>() where T: UnityEngine.Object
			{
				var actorPath = Metadata.Loader.PathPrefix + "/" + Id;
				var resourcePaths = await providers.LocateResourcesAsync<T>(actorPath);
				var appearances = new List<string>();
				
				foreach (var path in resourcePaths) 
				{
					var appearance = path.Remove(actorPath + "/");
					appearances.Add(appearance);
				} 
				return appearances.ToArray();
			} 
		}

		protected override void GetAppearanceData()
		{
			var appearances = GetOrthoAppearanceList().Result;
				
			if(appearances.Length > 0) 
			{
				CommandParameters.Add(new CommandParameterData<string>(Appearance, () => Actor.Appearance ?? GetDefaultAppearance(appearances), v => Actor.Appearance = (string)v, (i, p) => i.StringDropdownField(p, appearances), defaultValue: GetDefaultAppearance(appearances)));
			}
			else CommandParameters.Add(new CommandParameterData<string>(Appearance, () => Actor.Appearance, v => Actor.Appearance = (string)v, (i, p) => i.StringField(p)));
			
		}
		
		protected string GetDefaultAppearance(string[] appearances)
		{
			if (appearances != null && appearances.Length > 0)
			{
				if (appearances.Any(t => t.EqualsFast(Id))) return appearances.First(t => t.EqualsFast(Id));
				if (appearances.Any(t => t.EqualsFast("Default"))) return appearances.First(t => t.EqualsFast("Default"));
			}
			return appearances.FirstOrDefault();
		}

#if UNITY_EDITOR

		protected TConfig EditorConfig => ProjectConfigurationProvider.LoadOrDefault<TConfig>();
		protected List<SerializedObject> SerializedConfigs => new List<SerializedObject> { new SerializedObject(EditorConfig), new SerializedObject(Config)};
		protected virtual List<List<TPose>> Poses { get; }
		protected virtual List<List<TPose>> SharedPoses { get; }
						
		public void AddPose(string poseName)
		{
			AddSharedPose(poseName);
			TransferPose();
		}
		
		public void AddSharedPose(string poseName)
		{
			AddPoseToList(SharedPoses);
			SetPoseOverrides();
			SetPoseValues(poseName);
		}
		
		public string[] GetPoses() 
		{
			var nullList = new string[] {"None"};
 			var namedPoseList = Poses[1].Where(p => !String.IsNullOrEmpty(p.Name)).Select(p =>  "Pose:" + p.Name).ToArray();
			var namedSharedPoseList = SharedPoses[1].Where(p => !String.IsNullOrEmpty(p.Name)).Select(p => "Shared Pose:" + p.Name).ToArray();
			return nullList.Concat(namedPoseList).Concat(namedSharedPoseList).ToArray();
		}
		
		private void AddPoseToList(List<List<TPose>> poses)
		{
			foreach(var poseslist in poses) poseslist.Add(new TPose());
			foreach(var config in SerializedConfigs) config.ApplyModifiedProperties();
		}
		
		private void SetPoseOverrides()
		{
			foreach(var config in SerializedConfigs)
			{
				config.Update();
				
				var lastPoseIndex = SharedPoses[1].Count-1;
				var element = config.FindProperty("SharedPoses").GetArrayElementAtIndex(lastPoseIndex);
				
				var overrides = CommandParameters.Where(p => p.Selected).Select(p => p.Name.ToLower()).ToList();
				var overriddenProperties = element.FindPropertyRelative("overriddenProperties");
						
				overrides.Add("visible");
				if(overrides.Contains("look")) overrides[overrides.IndexOf("look")] = "lookDirection";
				if(overrides.Contains("tint")) overrides[overrides.IndexOf("tint")] = "tintColor";
				if(overrides.Contains("pos")) overrides[overrides.IndexOf("pos")] = "position";

				for(int i = 0; i < overrides.Count; i++)
				{
					overriddenProperties.InsertArrayElementAtIndex(i);
					overriddenProperties.GetArrayElementAtIndex(i).stringValue = overrides[i];
				}
				
				config.ApplyModifiedProperties();
			}
		}
		
		private void SetPoseValues(string poseName)
		{
			foreach(var config in SerializedConfigs)
			{
				var lastPoseIndex = SharedPoses[1].Count-1;
				var element = config.FindProperty("SharedPoses").GetArrayElementAtIndex(lastPoseIndex);
				
				while (element.NextVisible(true) && !SerializedProperty.EqualContents(element, element.GetEndProperty()))
				{				
					if(element.propertyPath.Contains(".visible")) element.boolValue = true;
					else if(element.propertyPath.Contains(".name")) element.stringValue = poseName;
					
					foreach(var data in CommandParameters) data.SetPoseValue(element);
				}
				
				config.ApplyModifiedProperties();
				AssetDatabase.SaveAssets();
			}
		}
		
		private void TransferPose()
		{
			AddPoseToList(Poses);
			
			foreach (var posesList in Poses) posesList[posesList.Count-1] = SharedPoses[1].Last();
			foreach(var posesList in SharedPoses) posesList.RemoveLastItem();
			
			foreach(var config in SerializedConfigs) AssetDatabase.SaveAssets();
		}
		
		public abstract void ApplyPose(string poseName);
		
#endif
	}
}