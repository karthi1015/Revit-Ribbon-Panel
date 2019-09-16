namespace Gladkoe.Utilities
{
    using Autodesk.Revit.DB;

    public static class ParameterHelper
    {
        private const double MetersInFeet = 0.3048;

        public static double FeetAsMeters(this double param)
        {
            double imperialValue = param;

            return imperialValue * MetersInFeet;
        }

        public static double FeetAsMillimeters(this double param)
        {
            double imperialValue = param;

            return imperialValue * MetersInFeet * 1000;
        }

        public static double FeetAsCentimeters(this double param)
        {
            double imperialValue = param;

            return imperialValue * MetersInFeet * 100;
        }

        public static string GetStringParameterValue(this Parameter param)
        {
            string s;
            switch (param.StorageType)
            {
                case StorageType.Double:
                    s = param.HasValue ? param.AsValueString() : string.Empty;
                    break;

                case StorageType.Integer:
                    s = param.HasValue ? param.AsInteger().ToString() : string.Empty;
                    break;

                case StorageType.String:
                    s = param.HasValue ? param.AsString() : string.Empty;
                    break;

                case StorageType.ElementId:
                    s = param.HasValue ? param.AsElementId().IntegerValue.ToString() : string.Empty;
                    break;

                case StorageType.None:
                    s = "?NONE?";
                    break;

                default:
                    s = "?ELSE?";
                    break;
            }

            return s;
        }
    }
}