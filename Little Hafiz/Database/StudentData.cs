using System.Linq;

namespace Little_Hafiz
{
    internal class StudentData
    {
        public string FullName;
        public string NationalNumber;
        public string BirthDate;
        public string Job;

        public string FatherQualification, MotherQualification;
        public string FatherJob, MotherJob;
        public string FatherPhone, MotherPhone;
        public string GuardianName, GuardianLink;
        public string GuardianBirth;

        public string PhoneNumber, Address, Email, Facebook;
        public string School, Class;

        public int BrothersCount, ArrangementBetweenBrothers;

        public string MaritalStatus; public string MemorizationAmount;
        public string StudentMashaykh, MemorizePlaces;
        public string JoiningDate, FirstConclusionDate;
        public string Certificates, Ijazah, Courses, Skills, Hobbies;

        public string Image;
        public string ImageName => Image.Split('\\').Last();

        public override string ToString()
            => $"'{FullName}', '{NationalNumber}', '{BirthDate}', '{Job}', '{FatherQualification}', '{MotherQualification}', '{FatherJob}', '{MotherJob}', '{FatherPhone}', '{MotherPhone}', '{GuardianName}', '{GuardianLink}', '{GuardianBirth}', '{PhoneNumber}', '{Address}', '{Email}', '{Facebook}', '{School}', '{Class}', {BrothersCount}, {ArrangementBetweenBrothers}, {MaritalStatus}, '{MemorizationAmount}', '{StudentMashaykh}', '{MemorizePlaces}', '{JoiningDate}', '{FirstConclusionDate}', '{Certificates}', '{Ijazah}', '{Courses}', '{Skills}', '{Hobbies}', '{ImageName}'";
    }
}
