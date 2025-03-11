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

namespace Little_Hafiz
{
    internal static class DatabaseHelper
    {
        private static bool success = false;
        private static readonly int version = 1;
        private static string dataFolder = "Data", imagesFolder = $"{dataFolder}\\images", databaseFile = $"{dataFolder}\\Students.db";

        private static readonly SQLiteConnection conn = new SQLiteConnection();
        private static readonly SQLiteCommand command = new SQLiteCommand(conn);
        private static SQLiteDataReader reader;

        #region Metadata
        public static int Version { get; private set; }
        public static int StudentCount { get; private set; }
        public static DateTime CreateDate { get; private set; }
        public static string Comment { get; private set; }
        #endregion

        private static string tableColumnsNames;

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
                                      $"INSERT INTO metadata ({version}, {DateTime.Now:yyyy-MM-dd}, 'مكتبة الحافظ الصغير بمسطرد');";
                command.ExecuteNonQuery();
            }
            catch { return false; }
            finally { conn.Close(); }
            return true;
        }

        private static void ReadMetadata()
        {
            try
            {
                conn.Open();
                command.CommandText = "SELECT version,create_date,comment FROM metadata";
                reader = command.ExecuteReader();
                if (!reader.Read()) return;
                Version = reader.GetInt32(0);
                if (Version != version) return;
                CreateDate = DateTime.ParseExact(reader.GetString(1), "yyyy-MM-dd", DateTimeFormatInfo.CurrentInfo);
                Comment = reader.GetString(2);
                reader.Close();
                success = Version == version;

                command.CommandText = "SELECT GROUP_CONCAT(name) FROM PRAGMA_table_info('students')";
                reader = command.ExecuteReader();
                if (reader.Read())
                    tableColumnsNames = reader.GetString(0);
                else
                    success = false;
                reader.Close();

                command.CommandText = "SELECT COUNT(*) FROM students";
                reader = command.ExecuteReader();
                if (reader.Read())
                    StudentCount = reader.GetInt32(0);
            }
            catch { success = false; }
            finally
            {
                reader?.Close();
                conn.Close();
            }
        }

        public static bool AddStudent(StudentData data)
            => ExecuteNonQuery($"INSERT INTO students ({data}, 0, {DateTime.Now.Ticks})");

        public static StudentData SelectStudent(string nationalNumber, StudentState state)
        {
            if (!success) return null;
            try
            {
                conn.Open();
                command.CommandText = $"SELECT * FROM students WHERE national = '{nationalNumber}'";
                if (state != StudentState.All)
                    command.CommandText += $" AND state = {(int)state}";
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

        public static bool UpdateStudent(StudentData data)
            => ExecuteNonQuery($"UPDATE students SET ({tableColumnsNames}) = ({data}, 0, {DateTime.Now.Ticks}) WHERE national = '{data.NationalNumber}'");

        public static bool RestoreStudent(string nationalNumber)
            => UpdateStudentState(nationalNumber, StudentState.Normal);

        public static bool ArchiveStudent(string nationalNumber)
            => UpdateStudentState(nationalNumber, StudentState.Archived);
        
        public static bool MoveToRecycleBin(string nationalNumber)
            => UpdateStudentState(nationalNumber, StudentState.Deleted);
        
        private static bool UpdateStudentState(string nationalNumber, StudentState state)
            => ExecuteNonQuery($"UPDATE students state = {(int)state}, state_date = {DateTime.Now.Ticks} WHERE national = '{nationalNumber}'");

        public static bool DeleteStudent(string nationalNumber)
            => ExecuteNonQuery($"DELETE FROM students WHERE national = '{nationalNumber}'");
        
        private static bool ExecuteNonQuery(string sql)
        {
            if (!success) return false;
            try
            {
                conn.Open();
                command.CommandText = sql;
                command.ExecuteNonQuery();
                return true;
            }
            catch { return false; }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }

        public static int RemoveDeletedStudent30Days()
        {
            if (!success) return -1;
            try
            {
                conn.Open();
                command.CommandText = $"DELETE FROM students WHERE state = 2 AND state_date >= {DateTime.Now.AddDays(-30).Ticks}";
                return command.ExecuteNonQuery();
            }
            catch { return -1; }
            finally
            {
                conn.Close();
            }
        }

    }
}
