using System;

namespace Little_Hafiz
{
    public static class AgeCalculator
    {
        public static string GetAgeDescription(DateTime birthDate, DateTime currentDate, out float year)
        {
            year = 0;

            if (currentDate < birthDate)
            {
                year = -1f;
                return "تاريخ الميلاد أكبر من تاريخ اليوم الحالي.";
            }
            
            int years = currentDate.Year - birthDate.Year;
            int months = currentDate.Month - birthDate.Month;
            int days = currentDate.Day - birthDate.Day;

            if (days < 0)
            {
                months--;
                int prevMonth = currentDate.Month == 1 ? 12 : currentDate.Month - 1;
                int prevMonthYear = currentDate.Month == 1 ? currentDate.Year - 1 : currentDate.Year;
                days += DateTime.DaysInMonth(prevMonthYear, prevMonth);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            string result = ""; 
            if (years > 0) result += $"{FormatPart(years, GetYearWord)} ";
            if (result != "" && months > 0) result += "و";
            if (months > 0) result += $"{FormatPart(months, GetMonthWord)} ";
            if (result != "" && days > 0) result += "و";
            if (days > 0) result += $"{FormatPart(days, GetDayWord)}.";

            year = years + (months / 12f) + (days / 365f);
            return result;
        }

        private static string FormatPart(int number, Func<int, string> getWord)
        {
            return (number == 1 || number == 2) ? getWord(number) : number + " " + getWord(number);
        }

        private static string GetYearWord(int number)
        {
            switch (number)
            {
                case 1: return "سنة";
                case 2: return "سنتين";
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10: return "سنوات";
                default: return "سنة";
            }
        }

        private static string GetMonthWord(int number)
        {
            switch (number)
            {
                case 1: return "شهر";
                case 2: return "شهرين";
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10: return "أشهر";
                default: return "شهر";
            }
        }

        private static string GetDayWord(int number)
        {
            switch (number)
            {
                case 1: return "يوم";
                case 2: return "يومين";
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10: return "أيام";
                default: return "يوم";
            }
        }
    }
}
