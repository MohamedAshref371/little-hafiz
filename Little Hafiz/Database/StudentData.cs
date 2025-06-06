using System.Linq;

namespace Little_Hafiz
{
    internal class StudentData
    {
        public int OfficeId;
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
        public string StudentTeacher, StudentGroup;

        public string StudentMashaykh, MemorizePlaces;
        public string JoiningDate, FirstConclusionDate;
        public string Certificates, Ijazah, Courses, Skills, Hobbies, StdComps, Notes;

        public string Image;
        public string ImageName => Image.Split('\\').Last();

        public override string ToString()
            => $"{OfficeId}, '{FullName.Trim()}', '{NationalNumber}', '{BirthDate}', '{Job.Trim()}', '{FatherQualification.Trim()}', '{MotherQualification.Trim()}', '{FatherJob.Trim()}', '{MotherJob.Trim()}', '{FatherPhone}', '{MotherPhone}', '{GuardianName.Trim()}', '{GuardianLink.Trim()}', '{GuardianBirth}', '{PhoneNumber}', '{Address.Trim()}', '{Email}', '{Facebook}', '{School.Trim()}', '{Class.Trim()}', {BrothersCount}, {ArrangementBetweenBrothers}, '{MaritalStatus.Trim()}', '{MemorizationAmount.Trim()}', '{JoiningDate}', '{FirstConclusionDate}', '{StudentTeacher.Trim()}', '{StudentGroup.Trim()}', '{StudentMashaykh}', '{MemorizePlaces}', '{Certificates}', '{Ijazah}', '{Courses}', '{Skills}', '{Hobbies}', '{StdComps}', '{Notes}', '{ImageName}'";
    }
}
