namespace Little_Hafiz
{
    internal class CompetitionGrade
    {
        public string NationalNumber;
        public int StudentCode;
        public int PreviousLevel;
        public int CompetitionLevel;
        public string CompetitionDate;
        public float CompetitionDegree;
        public int Rank;

        public override string ToString()
            => $"{NationalNumber}, {CompetitionLevel}, {CompetitionDate}, {CompetitionDegree}";
    }
}
