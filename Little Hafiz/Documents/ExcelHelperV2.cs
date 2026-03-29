using ClosedXML.Excel;

namespace Little_Hafiz
{
    internal class ExcelHelperV2
    {
        public static void ExtractExcel(ExcelRowDataV2[] rows, string path)
        {
            using (var workbook = new XLWorkbook())
            {
                workbook.RightToLeft = true;
                IXLWorksheet[] sheets = new IXLWorksheet[11]
                {
                    workbook.Worksheets.Add("جميع المستويات"),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[1]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[2]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[3]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[4]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[5]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[6]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[7]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[8]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[9]),
                    workbook.Worksheets.Add("المستوى " + Ranks.RanksText[10]),
                };

                for (int i = 0; i < sheets.Length; i++)
                    SetTitlesOnExcelFile(sheets[i]);

                int[] sheetsRowIndexes = new int[11] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
                for (int i = 0; i < rows.Length; i++)
                {
                    int sheetIndex = rows[i].CompetitionLevel;
                    SetDataOnExcelFile(sheets[sheetIndex], ref sheetsRowIndexes[sheetIndex], rows[i]);
                    if (sheetIndex != 0)
                        SetDataOnExcelFile(sheets[0], ref sheetsRowIndexes[0], rows[i]);
                }

                workbook.SaveAs(path);
            }
        }

        private static void SetTitlesOnExcelFile(IXLWorksheet sheet)
        {
            sheet.Range("A1:AO1").Merge().Value = "المسابقة القرآنية الرمضانية";
            sheet.Row(1).Height = 30;
            sheet.Cell("A1").Style.Font.Bold = true;
            sheet.Cell("A1").Style.Font.FontSize = 16;
            sheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            int col = 1;

            sheet.Cell(2, col++).Value = "م";
            sheet.Cell(2, col++).Value = "الاسم";
            sheet.Cell(2, col++).Value = "الرقم القومي";
            sheet.Cell(2, col++).Value = "تاريخ الميلاد";
            sheet.Cell(2, col++).Value = "تليفون الطالب";
            sheet.Cell(2, col++).Value = "العنوان";
            sheet.Cell(2, col++).Value = "البريد الإلكتروني";
            sheet.Cell(2, col++).Value = "فيسبوك";

            sheet.Cell(2, col++).Value = "الوظيفة";
            sheet.Cell(2, col++).Value = "المدرسة";
            sheet.Cell(2, col++).Value = "الصف";

            sheet.Cell(2, col++).Value = "مؤهل الأب";
            sheet.Cell(2, col++).Value = "مؤهل الأم";
            sheet.Cell(2, col++).Value = "وظيفة الأب";
            sheet.Cell(2, col++).Value = "وظيفة الأم";
            sheet.Cell(2, col++).Value = "هاتف الأب";
            sheet.Cell(2, col++).Value = "هاتف الأم";

            sheet.Cell(2, col++).Value = "ولي الأمر";
            sheet.Cell(2, col++).Value = "صلة القرابة";
            sheet.Cell(2, col++).Value = "تاريخ ميلاد ولي الأمر";

            sheet.Cell(2, col++).Value = "عدد الإخوة";
            sheet.Cell(2, col++).Value = "الترتيب بين الإخوة";
            sheet.Cell(2, col++).Value = "الحالة الاجتماعية";

            sheet.Cell(2, col++).Value = "مقدار الحفظ";
            sheet.Cell(2, col++).Value = "تاريخ الالتحاق بالمكتب";
            sheet.Cell(2, col++).Value = "تاريخ إتمام الختمة الأولى";

            sheet.Cell(2, col++).Value = "مدرس الطالب";
            sheet.Cell(2, col++).Value = "مجموعة الطالب";

            sheet.Cell(2, col++).Value = "المشايخ الذين حفظوه";
            sheet.Cell(2, col++).Value = "الأماكن التي حفظ فيها";
            sheet.Cell(2, col++).Value = "شهادات التقدير وأماكنها";
            sheet.Cell(2, col++).Value = "الإجازات";
            sheet.Cell(2, col++).Value = "الدورات";
            sheet.Cell(2, col++).Value = "المهارات";
            sheet.Cell(2, col++).Value = "الهوايات";
            sheet.Cell(2, col++).Value = "مسابقات";
            sheet.Cell(2, col++).Value = "الملاحظات";

            sheet.Cell(2, col++).Value = "المكتب";

            sheet.Cell(2, col++).Value = "الكود";
            sheet.Cell(2, col++).Value = "المستوى السابق";
            sheet.Cell(2, col++).Value = "المستوى الحالي";
            sheet.Cell(2, col++).Value = "تاريخ المسابقة";
            sheet.Cell(2, col++).Value = "المركز";
            sheet.Cell(2, col++).Value = "الدرجة";
        }

        private static void SetDataOnExcelFile(IXLWorksheet sheet, ref int row, ExcelRowDataV2 d)
        {
            int col = 1;

            sheet.Cell(row, col++).Value = (row - 2);

            sheet.Cell(row, col++).Value = d.FullName;
            sheet.Cell(row, col++).Value = d.NationalNumber;
            sheet.Cell(row, col++).Value = d.BirthDate;
            sheet.Cell(row, col++).Value = d.PhoneNumber;
            sheet.Cell(row, col++).Value = d.Address;
            sheet.Cell(row, col++).Value = d.Email;
            sheet.Cell(row, col++).Value = d.Facebook;

            sheet.Cell(row, col++).Value = d.Job;
            sheet.Cell(row, col++).Value = d.School;
            sheet.Cell(row, col++).Value = d.Class;

            sheet.Cell(row, col++).Value = d.FatherQuali;
            sheet.Cell(row, col++).Value = d.MotherQuali;
            sheet.Cell(row, col++).Value = d.FatherJob;
            sheet.Cell(row, col++).Value = d.MotherJob;
            sheet.Cell(row, col++).Value = d.FatherPhone;
            sheet.Cell(row, col++).Value = d.MotherPhone;

            sheet.Cell(row, col++).Value = d.GuardianName;
            sheet.Cell(row, col++).Value = d.GuardianLink;
            sheet.Cell(row, col++).Value = d.GuardianBirth;

            sheet.Cell(row, col++).Value = d.BrothersCount;
            sheet.Cell(row, col++).Value = d.Arrangement;
            sheet.Cell(row, col++).Value = d.MaritalStatus;

            sheet.Cell(row, col++).Value = d.MemoAmount;
            sheet.Cell(row, col++).Value = d.JoiningDate;
            sheet.Cell(row, col++).Value = d.ConclusionDate;

            sheet.Cell(row, col++).Value = d.Teacher;
            sheet.Cell(row, col++).Value = d.StdGroup;

            sheet.Cell(row, col++).Value = d.Mashaykh;
            sheet.Cell(row, col++).Value = d.MemoPlaces;
            sheet.Cell(row, col++).Value = d.Certificates;
            sheet.Cell(row, col++).Value = d.Ijazah;
            sheet.Cell(row, col++).Value = d.Courses;
            sheet.Cell(row, col++).Value = d.Skills;
            sheet.Cell(row, col++).Value = d.Hobbies;
            sheet.Cell(row, col++).Value = d.StdComps;
            sheet.Cell(row, col++).Value = d.Notes;

            sheet.Cell(row, col++).Value = d.Office == 0 ? "غير معروف" : Program.Form.Offices[d.Office];

            sheet.Cell(row, col++).Value = d.StudentCode;
            sheet.Cell(row, col++).Value = Ranks.ConvertNumberToRank(d.PreviousLevel);
            sheet.Cell(row, col++).Value = Ranks.ConvertNumberToRank(d.CompetitionLevel);
            sheet.Cell(row, col++).Value = d.CompetitionDate;
            sheet.Cell(row, col++).Value = d.Rank;
            sheet.Cell(row, col++).Value = d.Score;

            row++;
        }
    }
}
