namespace Little_Hafiz
{
    internal class StaticMembers
    {
        public static readonly string[] RanksText = { "لا يوجد", "الأول", "الثاني", "الثالث", "الرابع", "الخامس", "السادس", "السابع", "الثامن", "التاسع", "العاشر" };

        public static string ConvertNumberToRank(int? i)
        {
            if (i is null)
                return "غير معروف";
            else if (i >= 0 && i <= 10)
                return RanksText[(int)i];
            else
                return i.ToString();
        }
    }
}
