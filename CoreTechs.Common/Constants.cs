namespace CoreTechs.Common
{
    public static class Constants
    {
        public const double DaysPerYear = 365.242448489698;

        /*private static double CalculateDaysPerYear()
        {
            var max = DateTime.MaxValue;
            const int years = 9998;
            var span = max - max.AddMonths(-12 * years);
            var daysPerYear = span.TotalDays/years;
            return daysPerYear; //365.242448489698;
        }*/
    }
}