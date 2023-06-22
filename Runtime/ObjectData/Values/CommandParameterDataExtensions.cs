using Naninovel;
using System.Linq;
using UnityEngine;

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
			else if (value is Quaternion quaternion) return quaternion.eulerAngles.ToString("0.##").Replace(" ", "").Replace("(", "").Replace(")", "");
			else if (value is bool) return value.ToString().ToLower();
			else if (value is Color color) return "#" + ColorUtility.ToHtmlStringRGBA(color);
			else if (value is Texture texture) return texture is null ? string.Empty : texture.name;
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
		private static bool IsValid<T>(this ICommandParameterData<T> data) => data.Selected && !ExcludeState(data) && data.FulfillsConditions();

		// Will exclude the object if Exclude State Values is enabled and the current value is equal to the initial state value.  
		private static bool ExcludeState<T>(this ICommandParameterData<T> data) 
		{
			if(data is IListCommandParameterData) return false;
			
			if (CommandParameterData.ExcludeState)
			{
				if(data.Value == null) return true;
				else if(data.Value.Equals(data.State)) return true;
				else return false;
			} 
			else return false;
		} 
		
		public static bool FulfillsConditions(this ICommandParameterData data) 
		{
			if (data.Conditions.Length == 0) return true; 
			else if (data.Conditions.All(c => c())) return true;
			else return false;
		} 
	}
}