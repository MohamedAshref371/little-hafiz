namespace Little_Hafiz
{
    public class CompetitionGradeData
    {
        public int RowId;
        public string NationalNumber;
        public int StudentCode;
        public int PreviousLevel;
        public int CompetitionLevel;
        public string CompetitionDate;
        public float Score;
        public int Rank;

        public override string ToString()
            => $"'{NationalNumber}', {StudentCode}, {PreviousLevel}, {CompetitionLevel}, '{CompetitionDate}', {Score}, {Rank}";
    }
}
