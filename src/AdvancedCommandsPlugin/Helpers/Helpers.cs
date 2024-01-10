namespace Loupedeck.AdvancedCommandsPlugin.Helpers
{
    using System;

    public static class Helpers
    {
        public static String GetId(ActionEditorActionParameters actionParameters)
        {
            var id = String.Empty;

            foreach (var param in actionParameters.Parameters)
            {
                id += param.Value;
            }

            return id;
        }

        public static Boolean GetBooleanParam(ActionEditorActionParameters actionParameters, String paramName, Boolean defaultValue = false)
        {
            actionParameters.Parameters.TryGetValue(paramName, out var paramValueString);
            if (String.IsNullOrEmpty(paramValueString))
            {
                return defaultValue;
            }

            var repeat = Boolean.Parse(paramValueString);
            return repeat;
        }

        public static Int32 GetIntParam(ActionEditorActionParameters actionParameters, String paramName, Int32 defaultValue = 0)
        {
            actionParameters.Parameters.TryGetValue(paramName, out var paramValueString);
            if (String.IsNullOrEmpty(paramValueString))
            {
                return defaultValue;
            }

            var duration = Int32.Parse(paramValueString);
            return duration;
        }
    }
}
