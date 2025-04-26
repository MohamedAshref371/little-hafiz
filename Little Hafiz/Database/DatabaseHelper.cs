using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Little_Hafiz
{
    internal static class DatabaseHelper
    {
        private static bool success = false;
        private static readonly int classVersion = 2;
        private static readonly string dataFolder = "data", imagesFolder = $"{dataFolder}\\images\\", fileFormat = ".reco", recordFile = $"{dataFolder}\\{DateTime.Now.Ticks}{fileFormat}", databaseFile = $"{dataFolder}\\Students.db";
        private static readonly SQLiteConnection conn = new SQLiteConnection();
        private static readonly SQLiteCommand command = new SQLiteCommand(conn);
        private static SQLiteDataReader reader;

        public static bool Record { private get; set; } = true;

        #region Metadata
        public static int Version { get; private set; }
        public static DateTime CreateDate { get; private set; }
        public static string Comment { get; private set; }

        private static string studentsTableColumnsNames;
        #endregion

        static DatabaseHelper() => SafetyExamination();

        private static void SafetyExamination()
        {
            conn.ConnectionString = $"Data Source={databaseFile};Version=3;";

            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            RemoveEmptyRecords();
            if (!File.Exists(recordFile))
                File.WriteAllText(recordFile, "");

            if (File.Exists(databaseFile) || CreateDatabase())
                ReadMetadata();
        }

        private static bool CreateDatabase()
        {
            try
            {
                conn.Open();
                command.CommandText = "CREATE TABLE metadata (version INTEGER, create_date TEXT, comment TEXT);" +
                                      "CREATE TABLE students (full_name TEXT, national TEXT PRIMARY KEY, birth_date TEXT, job TEXT, father_quali TEXT, mother_quali TEXT, father_job TEXT, mother_job TEXT, father_phone TEXT, mother_phone TEXT, guardian_name TEXT, guardian_link TEXT, guardian_birth TEXT, phone_number TEXT, address TEXT, email TEXT, facebook TEXT, school TEXT, class TEXT, brothers_count INTEGER, arrangement INTEGER, student_level INTEGER, memo_amount TEXT, mashaykh TEXT, memo_places TEXT, joining_date TEXT, conclusion_date TEXT, certificates TEXT, ijazah TEXT, courses TEXT, skills TEXT, hobbies TEXT, image TEXT, state INTEGER, state_date INTEGER);" +
                                      "CREATE TABLE grades (national TEXT REFERENCES students (national), std_code INTEGER, prev_level INTEGER, competition_level INTEGER, competition_date TEXT, score NUMERIC, std_rank INTEGER, PRIMARY KEY (national, competition_date) );" +
                                      $"INSERT INTO metadata VALUES ({classVersion}, '{DateTime.Now:yyyy/MM/dd}', 'مكتبة الحافظ الصغير بمسطرد');";
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Program.LogError(ex.Message, ex.StackTrace, true);
                return false;
            }
            finally { conn.Close(); }
            return true;
        }

        #region Select SQLs
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
                    studentsTableColumnsNames = reader.GetString(0);
                else
                    return;

                success = true;
            }
            catch (Exception ex)
            {
                Program.LogError(ex.Message, ex.StackTrace, true);
                success = false;
            }
            finally
            {
                reader?.Close();
                conn.Close();
            }
        }

        public static int GetStudentCount()
        {
            if (!success) return -1;
            try
            {
                conn.Open();
                command.CommandText = "SELECT COUNT(*) FROM students";
                reader = command.ExecuteReader();
                if (!reader.Read()) return -1;
                return reader.GetInt32(0);
            }
            catch (Exception ex)
            {
                Program.LogError(ex.Message, ex.StackTrace, true);
                return -1;
            }
            finally
            {
                reader.Close();
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
                Program.LogError(ex.Message, ex.StackTrace, true);
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
            sb.Append("SELECT students.national, full_name, birth_date TEXT, competition_level, MAX(competition_date) competition_date, std_rank, image FROM students LEFT OUTER JOIN grades ON students.national = grades.national");

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

        public static CompetitionGradeData[] SelectStudentGrades(string nationalNumber)
            => SelectMultiRows($"SELECT * FROM grades WHERE national = '{nationalNumber}'", GetStudentGrade);

        public static CompetitionRankData[] SelectCompetitionRanks(int level, string dateFrom, string dateTo)
            => SelectMultiRows($"SELECT students.national, competition_date, std_code, full_name, score, std_rank FROM students JOIN grades ON students.national = grades.national WHERE competition_level = {level} AND competition_date >= '{dateFrom}' AND competition_date <= '{dateTo}' ORDER BY score DESC", GetCompetitionRanks);

        private static StudentSearchRowData GetStudentSearchRowData()
        {
            int? compLevel = null, stdRank = null;
            if (!reader.IsDBNull(3))
                compLevel = reader.GetInt32(3);
            if (!reader.IsDBNull(5))
                stdRank = reader.GetInt32(5);

            string img = (string)reader["image"];
            if (img != "")
            {
                img = imagesFolder + img;
                if (!File.Exists(img)) img = "";
            }

            return new StudentSearchRowData
            {
                NationalNumber = reader.GetString(0),
                FullName = reader.GetString(1),
                BirthDate = reader.GetString(2),
                CompetitionLevel = compLevel,
                CompetitionDate = reader.IsDBNull(4) ? null : reader.GetString(4),
                Rank = stdRank,
                Image = img,
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
                MemorizePlaces = (string)reader["memo_places"],
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

        private static CompetitionRankData GetCompetitionRanks()
        {
            return new CompetitionRankData
            {
                NationalNumber = (string)reader["national"],
                CompetitionDate = (string)reader["competition_date"],
                StudentCode = reader.GetInt32(2),
                StudentName = (string)reader["full_name"],
                Score = reader.GetFloat(4),
                Rank = reader.GetInt32(5),
            };
        }

        private static CompetitionGradeData GetStudentGrade()
        {
            return new CompetitionGradeData
            {
                NationalNumber = (string)reader["national"],
                StudentCode = reader.GetInt32(1),
                PreviousLevel = reader.GetInt32(2),
                CompetitionLevel = reader.GetInt32(3),
                CompetitionDate = (string)reader["competition_date"],
                Score = reader.GetFloat(5),
                Rank = reader.GetInt32(6),
            };
        }

        public static ExcelRowData[] SelectExcelRowData(int year = 0, int month = 0)
        {
            string dateFilter = year == 0 ? "1=1" : month == 0 ? $"competition_date LIKE '{year}/%'" : $"competition_date = '{year}/{month:D2}'";

            string sql = $@"SELECT g.std_code, s.full_name, s.birth_date, s.phone_number, g.competition_level, g.prev_level, s.class, s.address, s.memo_places, g.std_rank, g.competition_date FROM students s JOIN ( SELECT national, MAX(competition_date) AS max_date FROM grades WHERE {dateFilter} GROUP BY national ) latest ON s.national = latest.national JOIN grades g ON s.national = g.national AND g.competition_date = latest.max_date";

            return SelectMultiRows(sql, GetExcelRowData);
        }

        private static ExcelRowData GetExcelRowData()
        {
            return new ExcelRowData
            {
                StudentCode = reader.GetInt32(0),
                FullName = (string)reader["full_name"],
                BirthDate = (string)reader["birth_date"],
                PhoneNumber = (string)reader["phone_number"],
                CompetitionLevel = reader.GetInt32(4),
                PreviousLevel = reader.GetInt32(5),
                Class = (string)reader["class"],
                Address = (string)reader["address"],
                MemoPlace = (string)reader["memo_places"],
                Rank = reader.GetInt32(9),
                CompetitionAddedDate = (string)reader["competition_date"]
            };
        }

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
            catch (Exception ex)
            {
                Program.LogError(ex.Message, ex.StackTrace, true);
                return null;
            }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }
        #endregion

        #region Insert & Update & Delete
        public static bool IsInsideImagesFolder(StudentData data)
            => Path.GetFullPath(data.Image) == Path.GetFullPath(imagesFolder + data.ImageName);

        public static void CopyImageToImagesFolder(StudentData data)
        {
            try
            {
                if (File.Exists(data.Image))
                {
                    string imagePath = imagesFolder + data.FullName.Trim() + "_img" + DateTime.Now.Ticks.ToString() + "." + data.ImageName.Split('.').Last();
                    File.Copy(data.Image, imagePath);
                    data.Image = imagePath;
                    return;
                }
            }
            catch (Exception ex)
            {
                Program.LogError(ex.Message, ex.StackTrace, true);
            }

            data.Image = "";
        }

        public static int AddStudent(StudentData data)
        {
            if (data.Image != "" && !IsInsideImagesFolder(data))
                CopyImageToImagesFolder(data);

            return ExecuteNonQuery($"INSERT INTO students VALUES ({data}, 0, {DateTime.Now.Ticks}); ", Record);
        }

        public static int AddGrade(CompetitionGradeData data)
            => ExecuteNonQuery($"INSERT INTO grades (national, std_code, prev_level, competition_level, competition_date, score, std_rank) VALUES ({data}); ", Record);
        
        public static int UpdateStudent(StudentData data)
        {
            if (data.Image != "" && !IsInsideImagesFolder(data))
                CopyImageToImagesFolder(data);

            return ExecuteNonQuery($"UPDATE students SET ({studentsTableColumnsNames}) = ({data}, 0, {DateTime.Now.Ticks}) WHERE national = '{data.NationalNumber}'; ", Record);
        }

        public static int UpdateStudentGrade(CompetitionGradeData data)
            => ExecuteNonQuery($"UPDATE grades SET score = {data.Score}, std_rank = {data.Rank} WHERE national = '{data.NationalNumber}' AND competition_date = '{data.CompetitionDate}'; ", Record);
        
        public static int UpdateStudentRank(CompetitionRankData data)
            => ExecuteNonQuery($"UPDATE grades SET std_rank = {data.Rank} WHERE national = '{data.NationalNumber}' AND competition_date = '{data.CompetitionDate}'");
        
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

        private static int ExecuteNonQuery(string sql, bool recording = false)
        {
            if (!success || sql is null || sql.Trim() == "") return -1;
            try
            {
                conn.Open();
                command.CommandText = sql;
                int rtrn = command.ExecuteNonQuery();
                if (recording) File.AppendAllText(recordFile, sql);
                return rtrn;
            }
            catch (Exception ex)
            {
                Program.LogError(ex.Message, ex.StackTrace, true);
                return -1;
            }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }
        #endregion

        #region Records Helper
        public static void RemoveOldImages()
        {
            if (!success) return;
            string sql = "SELECT image FROM students WHERE image IS NOT NULL AND TRIM(image) <> '' ORDER BY image";

            string[] databaseImages = SelectMultiRows(sql, () => Path.Combine(imagesFolder, reader.GetString(0).Trim()));

            string[] realImages = Directory.GetFiles(imagesFolder).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray();


            int dbIndex = 0, fileIndex = 0; int comparison; string dbImage;

            while (fileIndex < realImages.Length)
            {
                dbImage = dbIndex < databaseImages.Length ? databaseImages[dbIndex] : null;

                comparison = dbImage == null ? -1 : string.Compare(realImages[fileIndex], dbImage, StringComparison.OrdinalIgnoreCase);

                if (comparison == 0)
                {
                    dbIndex++;
                    fileIndex++;
                }
                else if (comparison < 0 || dbImage == null) // الصورة غير موجودة في الداتا بيز
                {
                    try { File.Delete(realImages[fileIndex]); } catch { }
                    fileIndex++;
                }
                else
                    dbIndex++;
            }
        }

        private static void RemoveEmptyRecords()
        {
            string[] recs = Directory.GetFiles(dataFolder, $"*{fileFormat}", SearchOption.TopDirectoryOnly);

            foreach (string file in recs)
                if (File.ReadAllBytes(file).Length == 0)
                    File.Delete(file);
        }

        private static void RemoveAllRecords()
        {
            string[] recs = Directory.GetFiles(dataFolder, $"*{fileFormat}", SearchOption.TopDirectoryOnly);

            foreach (string file in recs)
                File.Delete(file);
        }

        public static bool ReadRecords(string folder)
        {
            if (!success) return false;
            string[] dataFiles = Directory.GetFiles(folder, $"*{fileFormat}", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < dataFiles.Length; i++)
                ExecuteNonQuery(File.ReadAllText(dataFiles[i]));

            RemoveOldImages();
            RemoveAllRecords();
            return true;
        }

        #endregion
    }
}
