using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Little_Hafiz
{
    internal static class DatabaseHelper
    {
        private static bool success = false;
        private static readonly int classVersion = 8;
        private static readonly string dataFolder = "data", recordsFolder = $"{dataFolder}\\records", imagesFolder = $"{dataFolder}\\images\\", fileFormat = ".reco", recordFile = $"{recordsFolder}\\{DateTime.Now.Ticks}{fileFormat}", databaseFile = $"{dataFolder}\\ProgData.ds";
        private static readonly SQLiteConnection conn = new SQLiteConnection($"Data Source={databaseFile};Version=3;");
        private static readonly SQLiteCommand command = new SQLiteCommand(conn);
        private static SQLiteDataReader reader;
        private static bool copyData;

        #region Metadata
        public static int Version { get; private set; }
        public static DateTime CreateDate { get; private set; }
        public static int CurrentOffice { get; private set; }
        public static string Comment { get; private set; }

        private static string studentsTableColumnsNames;
        #endregion

        static DatabaseHelper() => SafetyExamination();

        private static void SafetyExamination()
        {
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            if (!Directory.Exists(recordsFolder))
                Directory.CreateDirectory(recordsFolder);

            bool created = true;
            if (File.Exists(databaseFile))
                copyData = Properties.Settings.Default.BackupEnabled;
            else
                created = CreateDatabase();

            if (created) ReadMetadata();
        }

        private static bool CreateDatabase()
        {
            try
            {
                conn.Open();
                command.CommandText = "CREATE TABLE offices (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT, notes TEXT);" +
                                      "CREATE TABLE metadata (version INTEGER PRIMARY KEY, create_date INTEGER, office INTEGER REFERENCES offices (id), comment TEXT);" +
                                      "CREATE TABLE students (office INTEGER REFERENCES offices (id), full_name TEXT, national TEXT PRIMARY KEY, birth_date TEXT, job TEXT, father_quali TEXT, mother_quali TEXT, father_job TEXT, mother_job TEXT, father_phone TEXT, mother_phone TEXT, guardian_name TEXT, guardian_link TEXT, guardian_birth TEXT, phone_number TEXT, address TEXT, email TEXT, facebook TEXT, school TEXT, class TEXT, brothers_count INTEGER, arrangement INTEGER, marital_status TEXT, memo_amount TEXT, joining_date TEXT, conclusion_date TEXT, teacher TEXT, std_group TEXT, mashaykh TEXT, memo_places TEXT, certificates TEXT, ijazah TEXT, courses TEXT, skills TEXT, hobbies TEXT, std_comps TEXT, notes TEXT, image TEXT, state INTEGER, state_date INTEGER);" +
                                      "CREATE TABLE grades (national TEXT REFERENCES students (national), std_code INTEGER, prev_level INTEGER, competition_level INTEGER, competition_date TEXT, score NUMERIC, std_rank INTEGER, notes TEXT, PRIMARY KEY (national, competition_date) );" +
                                      $"INSERT INTO offices VALUES (0, '{System.Windows.Forms.Application.ProductName}', '');" +
                                      $"INSERT INTO metadata VALUES ({classVersion}, '{DateTime.Now.Ticks}', 0, 'https://github.com/MohamedAshref371');";
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Program.LogError(ex, true);
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
                command.CommandText = "SELECT version, create_date, office, comment FROM metadata";
                reader = command.ExecuteReader();
                if (!reader.Read()) return;
                Version = reader.GetInt32(0);
                if (Version != classVersion) return;
                CreateDate = new DateTime(reader.GetInt64(1));
                CurrentOffice = reader.GetInt32(2);
                Comment = reader.GetString(3);
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
                Program.LogError(ex, true);
                success = false;
            }
            finally
            {
                reader?.Close();
                conn.Close();
            }
        }

        public static int GetStudentCount(int office)
        {
            if (!success) return -1;
            try
            {
                conn.Open();
                command.CommandText = $"SELECT COUNT(*) FROM students {(office == 0 ? "" : $"WHERE office = {office}")}";
                reader = command.ExecuteReader();
                if (!reader.Read()) return -1;
                return reader.GetInt32(0);
            }
            catch (Exception ex)
            {
                Program.LogError(ex, true);
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
                Program.LogError(ex, true);
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
        public static StudentSearchRowData[] SelectStudents(string undoubtedName = null, string nationalNumber = null, string birthDate = null, StudentState? state = null, string phoneNumber = null, string email = null, int? office = null)
        {
            sb.Clear(); conds.Clear();
            sb.Append("SELECT students.national, full_name, birth_date TEXT, competition_level, MAX(competition_date) competition_date, std_rank, image FROM students LEFT OUTER JOIN grades ON students.national = grades.national");

            if (nationalNumber == null || nationalNumber.Length != 14)
            {
                if (undoubtedName != null)
                    conds.Add($"full_name LIKE '%{undoubtedName}%'");

                if (nationalNumber != null)
                    conds.Add($"students.national LIKE '%{nationalNumber}%'");

                if (birthDate != null)
                {
                    string[] arr = birthDate.Split('|');
                    if (arr.Length == 2)
                    {
                        if (arr[0] != "")
                            conds.Add($"birth_date >= '{arr[0]}'");
                        if (arr[1] != "")
                            conds.Add($"birth_date <= '{arr[1]}'");
                    }
                    else
                        conds.Add($"birth_date = '{birthDate}'");
                }

                if (state != null)
                    conds.Add($"state = {(int)state}");

                if (phoneNumber != null)
                    conds.Add($"phone_number LIKE '%{phoneNumber}%'");

                if (email != null)
                    conds.Add($"email LIKE '%{email}%'");

                if (office != null)
                    conds.Add($"office = {office}");
            }
            else if (office != null)
                conds.Add($"students.national = '{nationalNumber}' AND office = {office}");
            else
                conds.Add($"students.national = '{nationalNumber}'");

            if (conds.Count > 0)
            {
                sb.Append(" WHERE ").Append(conds[0]);
                for (int i = 1; i < conds.Count; i++)
                    sb.Append(" AND ").Append(conds[i]);
            }

            sb.Append(" GROUP BY students.national ORDER BY full_name");

            return SelectMultiRows(sb.ToString(), GetStudentSearchRowData);
        }

        public static StudentSearchRowData[] SelectStudents(TargetField target, string text, int office, bool isPartial = false)
        {
            string column = GetColumnTitle(target);
            if (column is null) return null;

            if (isPartial) text = $"LIKE '{text}%'";
            else text = $"= '{text}'";
            if (office != 0) text += $" AND office = {office}";

            string sql = $"SELECT students.national, full_name, birth_date TEXT, competition_level, MAX(competition_date) competition_date, std_rank, image FROM students LEFT OUTER JOIN grades ON students.national = grades.national WHERE {column} {text} GROUP BY students.national ORDER BY full_name";

            return SelectMultiRows(sql, GetStudentSearchRowData);
        }

        public static CompetitionGradeData[] SelectStudentGrades(string nationalNumber)
            => SelectMultiRows($"SELECT * FROM grades WHERE national = '{nationalNumber}'", GetStudentGrade);

        public static CompetitionRankData[] SelectCompetitionRanks(int level, string dateFrom, string dateTo, int office)
            => SelectMultiRows($"SELECT competition_level, students.national, competition_date, std_code, full_name, score, std_rank FROM students JOIN grades ON students.national = grades.national WHERE {(level == 0 ? "" : $"competition_level = {level} AND")} competition_date >= '{dateFrom}' AND competition_date <= '{dateTo}' {(office == 0 ? "" : $"AND office = {office}")} ORDER BY score DESC", GetCompetitionRanks);
        
        public static string[] GetOffices()
            => SelectMultiRows("SELECT name FROM offices", () => reader.GetString(0));

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
                OfficeId = reader.GetInt32(0),
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
                BrothersCount = reader.GetInt32(20),
                ArrangementBetweenBrothers = reader.GetInt32(21),
                MaritalStatus = (string)reader["marital_status"],
                MemorizationAmount = (string)reader["memo_amount"],
                JoiningDate = (string)reader["joining_date"],
                FirstConclusionDate = (string)reader["conclusion_date"],
                StudentTeacher = (string)reader["teacher"],
                StudentGroup = (string)reader["std_group"],
                StudentMashaykh = (string)reader["mashaykh"],
                MemorizePlaces = (string)reader["memo_places"],
                Certificates = (string)reader["certificates"],
                Ijazah = (string)reader["ijazah"],
                Courses = (string)reader["courses"],
                Skills = (string)reader["skills"],
                Hobbies = (string)reader["hobbies"],
                StdComps = (string)reader["std_comps"],
                Notes = (string)reader["notes"],
                Image = img
            };
        }

        private static CompetitionRankData GetCompetitionRanks()
        {
            return new CompetitionRankData
            {
                Level = reader.GetInt32(0),
                NationalNumber = (string)reader["national"],
                CompetitionDate = (string)reader["competition_date"],
                StudentCode = reader.GetInt32(3),
                StudentName = (string)reader["full_name"],
                Score = reader.GetFloat(5),
                Rank = reader.GetInt32(6),
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
                Notes = (string)reader["notes"],
            };
        }

        public static ExcelRowData[] SelectExcelRowData(int year = 0, int month = 0, int office = 0)
        {
            string dateFilter = year == 0 ? "1=1" : month == 0 ? $"competition_date LIKE '{year}/%'" : $"competition_date = '{year}/{month:D2}'";
            string officeFilter = office == 0 ? "" : $"WHERE s.office = {office}";

            string sql = $@"SELECT s.full_name, s.national, s.birth_date, s.phone_number, s.address, s.job, s.father_job, s.school, s.class, s.memo_amount,  s.office,  COALESCE(g.std_code, 0) AS std_code, COALESCE(g.prev_level, 0) AS prev_level, COALESCE(g.competition_level, 0) AS competition_level, COALESCE(g.competition_date, '') AS competition_date, COALESCE(g.std_rank, 0) AS std_rank  FROM students s LEFT JOIN ( SELECT national, MAX(competition_date) AS max_date FROM grades WHERE {dateFilter} GROUP BY national ) latest ON s.national = latest.national LEFT JOIN grades g ON s.national = g.national AND g.competition_date = latest.max_date {officeFilter}";

            return SelectMultiRows(sql, GetExcelRowData);
        }

        private static ExcelRowData GetExcelRowData()
        {
            return new ExcelRowData
            {
                FullName = (string)reader["full_name"],
                NationalNumber = (string)reader["national"],
                BirthDate = (string)reader["birth_date"],
                PhoneNumber = (string)reader["phone_number"],
                Address = (string)reader["address"],

                Job = (string)reader["job"],
                FatherJob = (string)reader["father_job"],
                School = (string)reader["school"],
                Class = (string)reader["class"],
                MemoAmount = (string)reader["memo_amount"],

                Office = reader.GetInt32(10),

                StudentCode = reader.GetInt32(11),
                PreviousLevel = reader.GetInt32(12),
                CompetitionLevel = reader.GetInt32(13),
                CompetitionDate = (string)reader["competition_date"],
                Rank = reader.GetInt32(15),
            };
        }

        public static FieldData[] FieldSearch(TargetField target)
        {
            string column = GetColumnTitle(target);
            if (column is null) return null;

            string sql = $"SELECT {column} AS text, COUNT({column}) AS count FROM students GROUP BY {column} ORDER BY text";

            return SelectMultiRows(sql, GetFieldData);
        }

        public static FieldData[] DateFieldSearch(TargetField target, bool perYear)
        {
            string column = GetColumnTitle(target);
            if (column is null) return null;

            string sql = $"SELECT strftime('{(perYear ? "%Y" : "%Y-%m")}', {column}) AS text, COUNT({column}) AS count FROM students GROUP BY text ORDER BY text";

            return SelectMultiRows(sql, GetFieldData);
        }

        public static FieldData[] FieldSearch(TargetField target, string text)
        {
            string column = GetColumnTitle(target);
            if (column is null) return null;

            string sql = $"SELECT full_name FROM students WHERE {column} = '{text}' ORDER BY full_name";

            return SelectMultiRows(sql, GetStudentNameFromFieldData);
        }

        private static FieldData GetFieldData()
        {
            return new FieldData
            {
                Text = (string)reader["text"],
                Count = reader.GetInt32(1),
            };
        }

        private static FieldData GetStudentNameFromFieldData()
        {
            return new FieldData
            {
                Text = (string)reader["full_name"],
                Count = 1,
            };
        }

        private static string GetColumnTitle(TargetField target)
        {
            switch (target)
            {
                case TargetField.StudentName: return "full_name";
                case TargetField.StudentBirthDate: return "birth_date";
                case TargetField.StudentJob: return "job";
                case TargetField.FatherQualification: return "father_quali";
                case TargetField.MotherQualification: return "mother_quali";
                case TargetField.FatherJob: return "father_job";
                case TargetField.MotherJob: return "mother_job";
                case TargetField.GuardianName: return "guardian_name";
                case TargetField.GuardianLink: return "guardian_link";
                case TargetField.GuardianBirthDate: return "guardian_birth";
                case TargetField.Address: return "address";
                case TargetField.School: return "school";
                case TargetField.Class: return "class";
                case TargetField.MaritalStatus: return "marital_status";
                case TargetField.MemoAmount: return "memo_amount";
                case TargetField.JoiningDate: return "joining_date";
                case TargetField.FirstConclusionDate: return "conclusion_date";
                case TargetField.StudentTeacher: return "teacher";
                case TargetField.StudentGroup: return "std_group";
                default: return null;
            }
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
                Program.LogError(ex, true);
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
                Program.LogError(ex, true);
            }

            data.Image = "";
        }

        public static int AddStudent(StudentData data)
        {
            if (data.Image != "" && !IsInsideImagesFolder(data))
                CopyImageToImagesFolder(data);

            return ExecuteNonQuery($"INSERT INTO students VALUES ({data}, 0, {DateTime.Now.Ticks})", Program.Record);
        }

        public static int AddGrade(CompetitionGradeData data)
            => ExecuteNonQuery($"INSERT INTO grades VALUES ({data})", Program.Record);

        public static int AddOffice(string name, string notes)
            => ExecuteNonQuery($"INSERT INTO offices (name, notes) VALUES ('{name}', '{notes}')");

        public static int UpdateStudent(StudentData data)
        {
            if (data.Image != "" && !IsInsideImagesFolder(data))
                CopyImageToImagesFolder(data);

            return ExecuteNonQuery($"UPDATE students SET ({studentsTableColumnsNames}) = ({data}, 0, {DateTime.Now.Ticks}) WHERE national = '{data.NationalNumber}'", Program.Record);
        }

        public static int UpdateStudentGrade(CompetitionGradeData data)
            => ExecuteNonQuery($"UPDATE grades SET score = {data.Score}, std_rank = {data.Rank} WHERE national = '{data.NationalNumber}' AND competition_date = '{data.CompetitionDate}'", Program.Record);

        public static int UpdateStudentRank(CompetitionRankData data)
            => ExecuteNonQuery($"UPDATE grades SET std_rank = {data.Rank} WHERE national = '{data.NationalNumber}' AND competition_date = '{data.CompetitionDate}'");

        public static int UpdateMetadataOffice(int office)
        {
            if (office < 0) return -1;
            CurrentOffice = office;
            return ExecuteNonQuery($"UPDATE metadata SET office = {office}");
        }

        public static int DeleteStudentGrade(CompetitionGradeData data)
            => ExecuteNonQuery($"DELETE FROM grades WHERE national = '{data.NationalNumber}' AND competition_date = '{data.CompetitionDate}'", Program.Record);

        public static int DeleteStudentPermanently(string nationalNumber)
            => ExecuteNonQuery($"DELETE FROM students WHERE national = '{nationalNumber}'", Program.Record);

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
            if (copyData)
            {
                copyData = false;
                DatabaseBackup();
            }

            try
            {
                conn.Open();
                command.CommandText = sql;
                int rtrn = command.ExecuteNonQuery();
                if (recording)
                {
                    if (!File.Exists(recordFile))
                        File.AppendAllText(recordFile, Base64Converter.StringToBase64(CreateDate.Ticks.ToString()));
                    File.AppendAllText(recordFile, ";" + Base64Converter.StringToBase64(sql));
                }
                return rtrn;
            }
            catch (Exception ex)
            {
                Program.LogError(ex, true);
                return -1;
            }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }
        #endregion

        #region Database Sweetening
        public static bool RemoveOldImages()
        {
            if (!success) return false;
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
            return true;
        }

        public static void RemoveAllRecords()
        {
            string[] recs = Directory.GetFiles(recordsFolder, $"*{fileFormat}", SearchOption.TopDirectoryOnly);

            foreach (string file in recs)
                File.Delete(file);
        }

        public static string[] ReadRecords(string folder)
        {
            if (!success) return null;
            string[] dataFiles = Directory.GetFiles(folder, $"*{fileFormat}", SearchOption.TopDirectoryOnly).OrderBy(f => f).ToArray();

            List<string> err = new List<string>();
            string[] sqls; bool isTrue;
            for (int i = 0; i < dataFiles.Length; i++)
            {
                sqls = File.ReadAllText(dataFiles[i]).Split(';').Select(sql => Base64Converter.Base64ToString(sql)).Where(sql => sql != string.Empty).ToArray();
                isTrue = long.TryParse(sqls[0], out long num);
                if (!isTrue || num != CreateDate.Ticks)
                {
                    err.Add(Path.GetFileName(dataFiles[i]));
                    continue; 
                }
                if (!ExecuteNonQuery(sqls))
                    err.Add(Path.GetFileName(dataFiles[i]));
            }

            RemoveOldImages();
            RemoveAllRecords();
            return err.ToArray();
        }

        private static bool ExecuteNonQuery(string[] sqls)
        {
            bool noErrors = true;
            for (int i = 1; i < sqls.Length; i++)
                if (ExecuteNonQuery(sqls[i]) == -1)
                    noErrors = false;
            return noErrors;
        }

        private static void DatabaseBackup()
        {
            if (!success) return;

            if (!Directory.Exists($"{dataFolder}\\backup"))
                Directory.CreateDirectory($"{dataFolder}\\backup");

            if (File.Exists(databaseFile))
                File.Copy(databaseFile, $"{dataFolder}\\backup\\{DateTime.Now.Ticks}.ds");
        }
        #endregion
    }
}
