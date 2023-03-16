using Naninovel;
using NaninovelSceneAssistant;
using UnityEngine;


namespace NaninovelSceneAssistant
{
    public static class CommandDataExtensions
    {
        public static string FormatName(this ICommandData parameter) => char.ToLower(parameter.Name[0]) + parameter.Name.Substring(1);

        public static string FormatValue<T>(this ICommandData<T> parameter)
        {
            if (parameter.Name == "Pos" && parameter.Value is Vector3 vector) return GetPosValue(vector);
            if (parameter.Value is Vector2 || parameter.Value is Vector3 || parameter.Value is Vector4) return parameter.Value.ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
            else if (parameter.Value is Quaternion quaternion) return quaternion.eulerAngles.ToString("0.##").Replace(" ", "").Replace("(", "").Replace(")", "");
            else if (parameter.Value is bool) return parameter.Value.ToString().ToLower();
            else if (parameter.Value is Color color) return "#" + ColorUtility.ToHtmlStringRGBA(color);
            //else if (ValueIsDictionary(Value, out var namedList)) return namedList;
            else if (parameter.Value is Texture texture && texture is null) return string.Empty;
            else return parameter.Value?.ToString();
        }

        private static string GetPosValue(Vector3 vector)
        {
            var config = Engine.GetConfiguration<CameraConfiguration>();
            return new Vector3(config.WorldToSceneSpace(vector).x * 100, config.WorldToSceneSpace(vector).y * 100, vector.z).ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
        }

        public static string GetCommandValue<T>(this ICommandData<T> parameter, bool paramOnly = false)
        {
            if (parameter.Selected && (parameter.HasCondition == null || parameter.HasCondition())) return (paramOnly ? null : parameter.FormatName() + ":") + parameter.FormatValue();
            else return null;
        }

        public static string GetNamedValue<T>(this INamedCommandData<T> parameter, bool paramOnly = false)
        {
            if (parameter.Selected && (parameter.HasCondition == null || parameter.HasCondition())) return (paramOnly ? null : parameter.FormatName()) + parameter.NamedKey + "." + parameter.FormatValue();
            else return null;
        }
    }
}
