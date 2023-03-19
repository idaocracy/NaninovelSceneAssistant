using Naninovel;
using System.Linq;
using UnityEngine;

namespace NaninovelSceneAssistant
{
    public static class CommandParameterDataExtensions
    {
        private static string FormatName(this string name) => char.ToLower(name[0]) + name.Substring(1);

        public static string FormatValue<T>(this ICommandParameterData<T> parameter)
        {
            if (parameter.Name == "Pos" && parameter.Value is Vector3 vector) return GetPosValue(vector);
            else return FormatValue(parameter.Value);
        }

        public static string FormatValue<T>(this T value)
        {
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

        public static string GetCommandValue<T>(this ICommandParameterData<T> parameter, bool paramOnly = false)
        {
            if (parameter.IsValid()) return (paramOnly ? null : parameter.Name.FormatName() + ":") + parameter.Value.FormatValue();
            else return null;
        }

        public static string GetNamedValue<T>(this INamedCommandParameterData<T> parameter, bool paramOnly = false)
        {
            if (parameter.IsValid()) return (paramOnly ? null : parameter.Name.FormatName()) + parameter.NamedKey + "." + parameter.Value.FormatValue();
            else return null;
        }

        public static string GetListValue(this IListCommandParameterData parameter, bool paramOnly = false)
        {
            if (parameter.Values.All(c => c.GetCommandValue() == null)) return null;
            else return (paramOnly ? string.Empty : parameter.Name.FormatName() + ":") + string.Join(",", parameter.Values.Select(c => c.GetCommandValue(paramOnly: true) ?? string.Empty));
        }

        private static bool IsValid<T>(this ICommandParameterData<T> parameter) => parameter.Selected && !ExcludeState(parameter) && (parameter.HasCondition == null || parameter.HasCondition());

        private static bool ExcludeState<T>(this ICommandParameterData<T> parameter) => CommandParameterData.ExcludeState && parameter.Value.Equals(parameter.State);
    }
}