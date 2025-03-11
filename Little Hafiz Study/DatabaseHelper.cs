using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace Little_Hafiz
{
    internal static class DatabaseHelper
    {
        private static bool success = false;
        private static readonly int classVersion = 1;
        private static string dataFolder = "Data", imagesFolder = $"{dataFolder}\\images", databaseFile = $"{dataFolder}\\Students.db";

        private static readonly SQLiteConnection conn = new SQLiteConnection();
        private static readonly SQLiteCommand command = new SQLiteCommand(conn);
        private static SQLiteDataReader reader;

        #region Metadata
        public static int Version { get; private set; }
        public static int StudentCount { get; private set; }
        public static DateTime CreateDate { get; private set; }
        public static string Comment { get; private set; }

        private static string tableColumnsNames;
        #endregion

        static DatabaseHelper() => SafetyExamination();
        
        private static void SafetyExamination()
        {
            conn.ConnectionString = $"Data Source={databaseFile};Version=3;";

            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            if (File.Exists(databaseFile) || CreateDatabase())
                ReadMetadata();
        }

        private static bool CreateDatabase()
        {
            try
            {
                conn.Open();
                command.CommandText = "CREATE TABLE metadata (version INTEGER, create_date TEXT, comment TEXT);" +
                                      "CRETE TABLE students (full_name TEXT, national TEXT PRIMARY KEY, birth_date TEXT, job TEXT, father_quali TEXT, mother_quali TEXT, father_job TEXT, mother_job TEXT, father_phone TEXT, mother_phone TEXT, guardian_name TEXT, guardian_link TEXT, guardian_birth TEXT, phone_number TEXT, address TEXT, email TEXT, facebook TEXT, school TEXT, class TEXT, brothers_count INTEGER, arrangement INTEGER, level INTEGER, memo_amount TEXT, mashaykh TEXT, mashaykh_places TEXT, joining_date TEXT, conclusion_date TEXT, certificates TEXT, vacations TEXT, courses TEXT, skills TEXT, hobbies TEXT, image TEXT, state INTEGER, state_date INTEGER);" +
                                      $"INSERT INTO metadata ({classVersion}, {DateTime.Now:yyyy/MM/dd}, 'مكتبة الحافظ الصغير بمسطرد');";
                command.ExecuteNonQuery();
            }
            catch { return false; }
            finally { conn.Close(); }
            return true;
        }

        #region Select
        private static void ReadMetadata()
        {
            try
            {
                conn.Open();
                command.CommandText = "SELECT version,create_date,comment FROM metadata";
                reader = command.ExecuteReader();
                if (!reader.Read()) return;
                Version = reader.GetInt32(0);
                if (Version != classVersion) return;
                CreateDate = DateTime.ParseExact(reader.GetString(1), "yyyy/MM/dd", DateTimeFormatInfo.CurrentInfo);
                Comment = reader.GetString(2);
                reader.Close();

                command.CommandText = "SELECT GROUP_CONCAT(name) FROM PRAGMA_table_info('students')";
                reader = command.ExecuteReader();
                if (reader.Read())
                    tableColumnsNames = reader.GetString(0);
                else
                    return;
                reader.Close();

                command.CommandText = "SELECT COUNT(*) FROM students";
                reader = command.ExecuteReader();
                if (reader.Read())
                    StudentCount = reader.GetInt32(0);

                success = true;
            }
            catch { success = false; }
            finally
            {
                reader?.Close();
                conn.Close();
            }
        }


        public static StudentData SelectStudent(string nationalNumber)
            => SelectUniqueStudent($"SELECT * FROM students WHERE national = '{nationalNumber}'");

        public static StudentData SelectStudent(string nationalNumber, StudentState state) // depracated
            => SelectUniqueStudent($"SELECT * FROM students WHERE national = '{nationalNumber}' AND state = {(int)state}");

        public static StudentData SelectStudentWithPhoneNumber(string phoneNumber)
            => SelectUniqueStudent($"SELECT * FROM students WHERE phone_number = '{phoneNumber}'");

        public static StudentData SelectStudentWithEmail(string email)
            => SelectUniqueStudent($"SELECT * FROM students WHERE email = '{email}'");

        private static StudentData SelectUniqueStudent(string sql)
        {
            if (!success) return null;
            try
            {
                conn.Open();
                command.CommandText = sql;
                reader = command.ExecuteReader();
                if (!reader.Read()) return null;
                return GetDataFromReader();
            }
            catch { return null; }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }


        public static StudentData[] SelectAllStudents(StudentState state)
            => SelectMultiStudents($"SELECT * FROM students WHERE state = {(int)state}");

        public static StudentData[] SelectAllStudents(int level)
            => SelectMultiStudents($"SELECT * FROM students WHERE level = {level}");

        public static StudentData[] SelectAllStudents(string undoubtedName)
            => SelectMultiStudents($"SELECT * FROM students WHERE full_name = '%{undoubtedName}%'");

        public static StudentData[] SelectMultiStudents(string sql)
        {
            if (!success) return null;
            try
            {
                conn.Open();
                command.CommandText = sql;
                reader = command.ExecuteReader();
                List<StudentData> list = new List<StudentData>();
                while (reader.Read())
                    list.Add(GetDataFromReader());
                return list.ToArray();
            }
            catch { return null; }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }


        private static StudentData GetDataFromReader()
        {
            return new StudentData
            {
                FullName = (string)reader["full_name"],
                NationalNumber = (string)reader["national"],
                BirthDate = (string)reader["birth_date"],
                Job = (string)reader["job"],
                FatherQualification = (string)reader["father_quali"],
                MotherQualification = (string)reader["mother_quali"],
                FatherJob = (string)reader["father_job"],
                MotherJob = (string)reader["mother_job"],
                FatherPhone = (string)reader["father_phone"],
                MotherPhone = (string)reader["mother_phone"],
                GuardianName = (string)reader["guardian_name"],
                GuardianLink = (string)reader["guardian_link"],
                GuardianBirth = (string)reader["guardian_birth"],
                PhoneNumber = (string)reader["phone_number"],
                Address = (string)reader["address"],
                Email = (string)reader["email"],
                Facebook = (string)reader["facebook"],
                School = (string)reader["school"],
                Class = (string)reader["class"],
                BrothersCount = (int)reader["brothers_count"],
                ArrangementBetweenBrothers = (int)reader["arrangement"],
                Level = (int)reader["level"],
                MemorizationAmount = (string)reader["memo_amount"],
                StudentMashaykh= (string)reader["mashaykh"],
                MashaykhPlaces= (string)reader["mashaykh_places"],
                JoiningDate= (string)reader["joining_date"],
                FirstConclusionDate= (string)reader["conclusion_date"],
                Certificates= (string)reader["certificates"],
                Vacations= (string)reader["vacations"],
                Courses= (string)reader["courses"],
                Skills= (string)reader["skills"],
                Hobbies= (string)reader["hobbies"],
                Image= (string)reader["image"]
            };
        }
        #endregion

        #region Insert & Update & Delete
        public static int AddStudent(StudentData data)
            => ExecuteNonQuery($"INSERT INTO students ({data}, 0, {DateTime.Now.Ticks})");

        public static int UpdateStudent(StudentData data)
            => ExecuteNonQuery($"UPDATE students SET ({tableColumnsNames}) = ({data}, 0, {DateTime.Now.Ticks}) WHERE national = '{data.NationalNumber}'");

        public static int DeleteStudentPermanently(string nationalNumber)
            => ExecuteNonQuery($"DELETE FROM students WHERE national = '{nationalNumber}'");

        public static int RemoveDeletedStudent30Days()
            => ExecuteNonQuery($"DELETE FROM students WHERE state = 2 AND state_date >= {DateTime.Now.AddDays(-30).Ticks}");

        public static int EmptyRecycleBin()
            => ExecuteNonQuery($"DELETE FROM students WHERE state = 2");

        #region Update Student State
        public static int RestoreStudent(string nationalNumber)
            => UpdateStudentState(nationalNumber, StudentState.Normal);

        public static int ArchiveStudent(string nationalNumber)
            => UpdateStudentState(nationalNumber, StudentState.Archived);

        public static int MoveToRecycleBin(string nationalNumber)
            => UpdateStudentState(nationalNumber, StudentState.Deleted);

        private static int UpdateStudentState(string nationalNumber, StudentState state)
            => ExecuteNonQuery($"UPDATE students state = {(int)state}, state_date = {DateTime.Now.Ticks} WHERE national = '{nationalNumber}'");
        #endregion

        private static int ExecuteNonQuery(string sql)
        {
            if (!success) return -1;
            try
            {
                conn.Open();
                command.CommandText = sql;
                return command.ExecuteNonQuery();
            }
            catch { return -1; }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }
        #endregion

    }
}
