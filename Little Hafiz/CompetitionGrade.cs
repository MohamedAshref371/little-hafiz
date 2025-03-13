using System;
using System.Linq;
using System.Text;

namespace Little_Hafiz
{
    internal class CompetitionGrade
    {
        public string NationalNumber;
        public int CompetitionLevel;
        public string CompetitionDate;
        public float CompetitionDegree;

        public override string ToString()
            => $"{NationalNumber}, {CompetitionLevel}, {CompetitionDate}, {CompetitionDegree}";
    }
}
