using ClosedXML.Excel;

namespace Little_Hafiz
{
    internal static class ExcelHelper
    {
        public static void ExtractExcel(ExcelRowData[] rows, string path)
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
            sheet.Range("A1:M1").Merge().Value = "المسابقة القرآنية الرمضانية";
            sheet.Row(1).Height = 30;
            sheet.Cell("A1").Style.Font.Bold = true;
            sheet.Cell("A1").Style.Font.FontSize = 16;
            sheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            sheet.Column(1).Width = 7;
            sheet.Cell(2, 1).Value = "م";

            sheet.Column(2).Width = 20;
            sheet.Cell(2, 2).Value = "الاسم";

            sheet.Column(3).Width = 15;
            sheet.Cell(2, 3).Value = "الرقم القومي";

            sheet.Column(4).Width = 15;
            sheet.Cell(2, 4).Value = "تاريخ الميلاد";

            sheet.Column(5).Width = 15;
            sheet.Cell(2, 5).Value = "رقم التليفون";

            sheet.Column(6).Width = 15;
            sheet.Cell(2, 6).Value = "العنوان";


            sheet.Column(7).Width = 10;
            sheet.Cell(2, 7).Value = "الوظيفة";

            sheet.Column(8).Width = 10;
            sheet.Cell(2, 8).Value = "وظيفة الأب";

            sheet.Column(9).Width = 10;
            sheet.Cell(2, 9).Value = "المدرسة/الكلية";

            sheet.Column(10).Width = 10;
            sheet.Cell(2, 10).Value = "الصف";

            sheet.Column(11).Width = 15;
            sheet.Cell(2, 11).Value = "مقدار الحفظ";


            sheet.Column(12).Width = 15;
            sheet.Cell(2, 12).Value = "المكتب";


            sheet.Column(13).Width = 7;
            sheet.Cell(2, 13).Value = "الكود";

            sheet.Column(14).Width = 7;
            sheet.Cell(2, 14).Value = "السابق";

            sheet.Column(15).Width = 7;
            sheet.Cell(2, 15).Value = "الحالي";

            sheet.Column(16).Width = 14;
            sheet.Cell(2, 16).Value = "تاريخ المسابقة";

            sheet.Column(17).Width = 7;
            sheet.Cell(2, 17).Value = "المركز";

            sheet.Column(18).Width = 7;
            sheet.Cell(2, 18).Value = "الدرجة";
        }

        private static void SetDataOnExcelFile(IXLWorksheet sheet, ref int row, ExcelRowData data)
        {
            sheet.Cell(row, 1).Value = (row - 2).ToString();

            sheet.Cell(row, 2).Value = data.FullName;
            sheet.Cell(row, 3).Value = data.NationalNumber;
            sheet.Cell(row, 4).Value = data.BirthDate;
            sheet.Cell(row, 5).Value = data.PhoneNumber;
            sheet.Cell(row, 6).Value = data.Address;

            sheet.Cell(row, 7).Value = data.Job;
            sheet.Cell(row, 8).Value = data.FatherJob;
            sheet.Cell(row, 9).Value = data.School;
            sheet.Cell(row, 10).Value = data.Class;
            sheet.Cell(row, 11).Value = data.MemoAmount;

            sheet.Cell(row, 12).Value = data.Office == 0 ? "غير معروف" : Program.Form.Offices[data.Office];

            sheet.Cell(row, 13).Value = data.StudentCode;
            sheet.Cell(row, 14).Value = Ranks.ConvertNumberToRank(data.PreviousLevel);
            sheet.Cell(row, 15).Value = Ranks.ConvertNumberToRank(data.CompetitionLevel);
            sheet.Cell(row, 16).Value = data.CompetitionDate;
            sheet.Cell(row, 17).Value = data.Rank;
            sheet.Cell(row, 18).Value = data.Score;
            row++;
        }
    }
}
