namespace Little_Hafiz
{
    internal class Ranks
    {
        public static readonly string[] RanksText = { "لا يوجد", "الأول", "الثاني", "الثالث", "الرابع", "الخامس", "السادس", "السابع", "الثامن", "التاسع", "العاشر" };
        public static readonly string[] CountText = { "صفر", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة", "عشرة" };

        public static string ConvertNumberToRank(int? i)
        {
            if (i is null)
                return "غير معروف";
            else if (i >= 0 && i < RanksText.Length)
                return RanksText[(int)i];
            else
                return i.ToString();
        }

        public static string ConvertNumberToCount(int? i)
        {
            if (i is null)
                return "غير معروف";
            else if (i >= 0 && i < CountText.Length)
                return CountText[(int)i];
            else
                return i.ToString();
        }
    }
}
