using System;
using System.Globalization;

namespace Little_Hafiz
{
    public static class DateTimeExtensions
    {
        public static string ToCompleteStandardString(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
        }

        public static string ToStandardString(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static string ToStandardStringWithoutDay(this DateTime date)
        {
            return date.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        public static DateTime ToStandardDateTime(this string dateStr)
        {
            if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;
            
            return DateTime.MinValue;
        }
    }
}
