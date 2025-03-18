﻿using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Little_Hafiz
{
    internal static class DatabaseHelper
    {
        private static bool success = false;
        private static readonly int classVersion = 1;
        private static readonly string dataFolder = "data", imagesFolder = $"{dataFolder}\\images\\", databaseFile = $"{dataFolder}\\Students.db";
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

            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            if (File.Exists(databaseFile) || CreateDatabase())
                ReadMetadata();
        }

        private static bool CreateDatabase()
        {
            try
            {
                conn.Open();
                command.CommandText = "CREATE TABLE metadata (version INTEGER, create_date TEXT, comment TEXT);" +
                                      "CREATE TABLE students (full_name TEXT, national TEXT PRIMARY KEY, birth_date TEXT, job TEXT, father_quali TEXT, mother_quali TEXT, father_job TEXT, mother_job TEXT, father_phone TEXT, mother_phone TEXT, guardian_name TEXT, guardian_link TEXT, guardian_birth TEXT, phone_number TEXT, address TEXT, email TEXT, facebook TEXT, school TEXT, class TEXT, brothers_count INTEGER, arrangement INTEGER, student_level INTEGER, memo_amount TEXT, mashaykh TEXT, mashaykh_places TEXT, joining_date TEXT, conclusion_date TEXT, certificates TEXT, ijazah TEXT, courses TEXT, skills TEXT, hobbies TEXT, image TEXT, state INTEGER, state_date INTEGER);" +
                                      "CREATE TABLE grades (national TEXT REFERENCES students (national), std_code INTEGER, prev_level INTEGER, competition_level INTEGER, competition_date TEXT, degree NUMERIC, std_place INTEGER, PRIMARY KEY(national, competition_level) );" +
                                      $"INSERT INTO metadata VALUES ({classVersion}, '{DateTime.Now:yyyy/MM/dd}', 'مكتبة الحافظ الصغير بمسطرد');";
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

        private static StudentData SelectUniqueStudent(string sql)
        {
            if (!success) return null;
            try
            {
                conn.Open();
                command.CommandText = sql;
                reader = command.ExecuteReader();
                if (!reader.Read()) return null;
                return GetStudentData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }


        private static readonly StringBuilder sb = new StringBuilder();
        private static readonly List<string> conds = new List<string>();
        public static StudentSearchRowData[] SelectStudents(string undoubtedName = null, string nationalNumber = null, StudentState? state = null, string phoneNumber = null, string email = null, int? level = null)
        {
            sb.Clear(); conds.Clear();
            sb.Append("SELECT students.national, full_name, competition_level, MAX(competition_date) competition_date, std_place FROM students LEFT OUTER JOIN grades ON students.national = grades.national");

            if (nationalNumber == null || nationalNumber.Length != 14)
            {
                if (undoubtedName != null)
                    conds.Add($"full_name LIKE '%{undoubtedName}%'");

                if (nationalNumber != null)
                    conds.Add($"students.national LIKE '%{nationalNumber}%'");

                if (state != null)
                    conds.Add($"state = {(int)state}");

                if (phoneNumber != null)
                    conds.Add($"phone_number LIKE '%{phoneNumber}%'");

                if (email != null)
                    conds.Add($"email LIKE '%{email}%'");

                if (level != null)
                    conds.Add($"student_level = {level}");
            }
            else { conds.Add($"students.national = '{nationalNumber}'"); }
            
            if (conds.Count > 0)
            {
                sb.Append(" WHERE ").Append(conds[0]);
                for (int i = 1; i < conds.Count; i++)
                    sb.Append(" AND ").Append(conds[i]);
            }

            sb.Append(" GROUP BY students.national");

            return SelectMultiRows(sb.ToString(), GetStudentSearchRowData);
        }

        public static CompetitionGradeDate[] SelectStudentGrades(string nationalNumber)
            => SelectMultiRows($"SELECT * FROM grades WHERE national = '{nationalNumber}'", GetStudentGrade);

        private static T[] SelectMultiRows<T>(string sql, Func<T> method)
        {
            if (!success) return null;
            try
            {
                conn.Open();
                command.CommandText = sql;
                reader = command.ExecuteReader();
                List<T> list = new List<T>();
                while (reader.Read())
                    list.Add(method());
                return list.ToArray();
            }
            catch { return null; }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }

        private static StudentSearchRowData GetStudentSearchRowData()
        {
            int? compLevel = null, stdRank = null;
            if (!reader.IsDBNull(2))
                compLevel = reader.GetInt32(2);
            if (!reader.IsDBNull(4))
                stdRank = reader.GetInt32(4);

            return new StudentSearchRowData
            {
                NationalNumber = reader.GetString(0),
                FullName = reader.GetString(1),
                CompetitionLevel = compLevel,
                CompetitionDate = reader.IsDBNull(3) ? null : reader.GetString(3),
                Rank = stdRank
            };
        }
        
        private static StudentData GetStudentData()
        {
            string img = (string)reader["image"];
            if (img != "")
            {
                img = imagesFolder + img;
                if (!File.Exists(img)) img = "";
            }

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
                BrothersCount = reader.GetInt32(19),
                ArrangementBetweenBrothers = reader.GetInt32(20),
                Level = reader.GetInt32(21),
                MemorizationAmount = (string)reader["memo_amount"],
                StudentMashaykh = (string)reader["mashaykh"],
                MashaykhPlaces = (string)reader["mashaykh_places"],
                JoiningDate = (string)reader["joining_date"],
                FirstConclusionDate = (string)reader["conclusion_date"],
                Certificates = (string)reader["certificates"],
                Ijazah = (string)reader["ijazah"],
                Courses = (string)reader["courses"],
                Skills = (string)reader["skills"],
                Hobbies = (string)reader["hobbies"],
                Image = img
            };
        }

        private static CompetitionGradeDate GetStudentGrade()
        {
            return new CompetitionGradeDate
            {
                NationalNumber = (string)reader["national"],
                StudentCode = reader.GetInt32(1),
                PreviousLevel = reader.GetInt32(2),
                CompetitionLevel = reader.GetInt32(3),
                CompetitionDate = (string)reader["competition_date"],
                CompetitionDegree = reader.GetFloat(5),
                Rank = reader.GetInt32(6),
            };
        }
        #endregion

        #region Insert & Update & Delete
        public static bool IsNotInsideImagesFolder(StudentData data)
            => Path.GetFullPath(data.Image) != Path.GetFullPath(imagesFolder + data.ImageName);

        public static void CopyImageToImagesFolder(StudentData data)
        {
            string imagePath = imagesFolder + data.FullName + " - img" + DateTime.Now.Ticks.ToString() + "." + data.ImageName.Split('.').Last();
            File.Copy(data.Image, imagePath);
            data.Image = imagePath;
        }

        public static int AddStudent(StudentData data)
        {
            if (File.Exists(data.Image))
            {
                if (IsNotInsideImagesFolder(data))
                    CopyImageToImagesFolder(data);
            }
            else
                data.Image = "";

            return ExecuteNonQuery($"INSERT INTO students VALUES ({data}, 0, {DateTime.Now.Ticks})");
        }

        public static int UpdateStudent(StudentData data)
        {
            if (!IsNotInsideImagesFolder(data))
                CopyImageToImagesFolder(data);

            return ExecuteNonQuery($"UPDATE students SET ({tableColumnsNames}) = ({data}, 0, {DateTime.Now.Ticks}) WHERE national = '{data.NationalNumber}'");
        }

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
