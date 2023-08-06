using Naninovel;
using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaninovelSceneAssistant
{
	public static class CommandParameterDataExtensions
	{
		private static string GetFormattedName(this ICommandParameterData data) 
		{
			return char.ToLower(data.Name[0]) + data.Name.Substring(1);
		} 

		public static string GetFormattedValue<T>(this ICommandParameterData<T> data)
		{
			if (data.Name == "Pos" && data.Value is Vector3 vector) return GetPosValue(vector);
			
			var value = data.Value;
			if (value is Vector2 || value is Vector3 || value is Vector4) return value.ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
			else if (value is Quaternion q) return q.eulerAngles.ToString("0.##").Replace(" ", "").Replace("(", "").Replace(")", "");
			else if (value is bool) return value.ToString().ToLower();
			else if (value is float f) return f.ToString("0.###").ToLower();
			else if (value is Color c) return "#" + ColorUtility.ToHtmlStringRGBA(c);
			else if (value is Texture t) return t is null ? string.Empty : t.name;
			else return value?.ToString();
		}

		private static string GetPosValue(Vector3 vector)
		{
			var config = Engine.GetConfiguration<CameraConfiguration>();
			return new Vector3(config.WorldToSceneSpace(vector).x * 100, config.WorldToSceneSpace(vector).y * 100, vector.z).ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
		}

		public static string GetCommandValue<T>(this ICommandParameterData<T> data, bool paramOnly = false)
		{
			if (data.IsValid()) return (paramOnly ? null : data.GetFormattedName() + ":") + data.GetFormattedValue();
			else return null;
		}

		public static string GetNamedValue<T>(this INamedCommandParameterData<T> data, bool paramOnly = false)
		{
			if (data.IsValid()) return (paramOnly ? null : data.GetFormattedName()) + data.NamedKey + "." + data.GetFormattedValue();
			else return null;
		}

		public static string GetListValue(this IListCommandParameterData data, bool paramOnly = false)
		{
			if (!data.Selected || !data.FulfillsConditions() || data.Values.All(c => c.GetCommandValue() == null)) return null;
			else return (paramOnly ? string.Empty : data.GetFormattedName() + ":") + string.Join(",", data.Values.Select(c => c.GetCommandValue(paramOnly: true) ?? string.Empty));
		}

		// Is the object selected, not eligible for exclusion and fulfills potential conditions.
		private static bool IsValid<T>(this ICommandParameterData<T> data) => data.Selected && !ExcludeState(data) && !ExcludeDefault(data) && data.FulfillsConditions();

		// Will exclude the object if Exclude State Values is enabled and the current value is equal to the initial state value.  
		private static bool ExcludeState<T>(this ICommandParameterData<T> data) 
		{
			if(data is IListCommandParameterData) return false;
			
			if(data.Default == null && data.Value == null) return true;
			else if(CommandParameterData.ExcludeState) return data.Value.Equals(data.State) ? true : false; 
			else return false;
		} 
		
		private static bool ExcludeDefault<T>(this ICommandParameterData<T> data) 
		{
			if(data is IListCommandParameterData) return false;
			
			if(data.Default == null && data.Value == null) return true;
			else if(CommandParameterData.ExcludeDefault) return data.Value.Equals(data.Default) ? true : false; 
			else return false;
		} 
		
		public static bool FulfillsConditions(this ICommandParameterData data) 
		{
			if (data.Conditions.Length == 0) return true; 
			else if (data.Conditions.All(c => c())) return true;
			else return false;
		} 
		

		#if UNITY_EDITOR
		public static void SetPoseValue(this ICommandParameterData data, SerializedProperty poseProperty)
		{
			if(!data.Selected) return;
			
			var path = poseProperty.propertyPath;
			
			if(path.Contains("appearance") && data.Name == "Appearance") 
			{
				var stringData = data as ICommandParameterData<string>;
				poseProperty.stringValue = stringData.Value;
			}
			
			else if(path.EndsWith("position") && (data.Name == "Position" || data.Name == "Pos"))
			{
				var positionData = data as ICommandParameterData<Vector3>;		
				poseProperty.vector3Value = positionData.Value;		
			} 
			
			else if(path.EndsWith("rotation") && data.Name == "Rotation")
			{
				var rotationData = data as ICommandParameterData<Vector3>;
				poseProperty.quaternionValue = Quaternion.Euler(rotationData.Value);
			} 
			
			else if(path.EndsWith("scale") && data.Name == "Scale")
			{
				var scaleData = data as ICommandParameterData<Vector3>;		
				poseProperty.vector3Value = scaleData.Value;		
			} 
			
			else if(path.Contains("tintColor") && data.Name == "Tint")
			{
				var tintData = data as ICommandParameterData<Color>;		
				poseProperty.colorValue = tintData.Value;
			}
			
			else if(path.Contains("lookDirection") && data.Name == "Look")
			{
				var lookData = data as ICommandParameterData<Enum>;	
				poseProperty.enumValueIndex = (int)(CharacterLookDirection)lookData.Value;
			}
			
		}
		#endif
	}
}