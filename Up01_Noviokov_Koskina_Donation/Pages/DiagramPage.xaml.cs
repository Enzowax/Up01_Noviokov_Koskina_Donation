using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using Up01_Noviokov_Koskina_Donation.Services;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class DiagramPage : Page
    {
        private readonly string _folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public DiagramPage()
        {
            InitializeComponent();

            DonationsChart.ChartAreas.Add(new ChartArea("Main"));
            DonationsChart.Series.Add(new Series("Собрано, ₽")
            {
                IsValueShownAsLabel = true,
                LabelFormat = "N2"
            });

            var organizations = Entities.GetContext().Organization.OrderBy(o => o.Name).ToList();
            organizations.Insert(0, new Organization { ID = 0, Name = "Все организации" });
            OrganizationBox.ItemsSource = organizations;
            OrganizationBox.SelectedIndex = 0;
        }

        private void Chart_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateChart();
        }

        private void UpdateChart()
        {
            if (!IsInitialized || OrganizationBox.SelectedItem == null || ChartTypeBox.SelectedItem == null)
                return;

            Series series = DonationsChart.Series[0];

            string typeName = ((ComboBoxItem)ChartTypeBox.SelectedItem).Tag.ToString();
            series.ChartType = (SeriesChartType)Enum.Parse(typeof(SeriesChartType), typeName);
            series.Points.Clear();

            var organization = (Organization)OrganizationBox.SelectedItem;
            foreach (var category in Entities.GetContext().Category.OrderBy(c => c.Name).ToList())
            {
                decimal sum = category.Campaign
                    .Where(c => organization.ID == 0 || c.OrganizationID == organization.ID)
                    .Sum(c => c.CollectedAmount);
                series.Points.AddXY(category.Name, (double)sum);
            }
        }

        private static void AddParagraph(Word.Document document, string text, Word.WdBuiltinStyle style)
        {
            Word.Paragraph paragraph = document.Paragraphs.Add();
            paragraph.Range.Text = text;
            paragraph.set_Style(style);
            paragraph.Range.InsertParagraphAfter();
        }

        private void WordButton_Click(object sender, RoutedEventArgs e)
        {
            var campaigns = Entities.GetContext().Campaign.OrderBy(c => c.Name).ToList();
            string docxPath = Path.Combine(_folder, "Отчёт по сборам.docx");
            string pdfPath = Path.Combine(_folder, "Отчёт по сборам.pdf");
            var normal = Word.WdBuiltinStyle.wdStyleNormal;

            var application = new Word.Application();
            try
            {
                Word.Document document = application.Documents.Add();

                for (int i = 0; i < campaigns.Count; i++)
                {
                    var campaign = campaigns[i];

                    AddParagraph(document, campaign.Name, Word.WdBuiltinStyle.wdStyleHeading1);
                    AddParagraph(document, $"Организация: {campaign.Organization.Name}", normal);
                    AddParagraph(document, $"Категория: {campaign.Category.Name}", normal);
                    AddParagraph(document, $"Статус: {campaign.StatusText}, сроки: {campaign.PeriodText}", normal);
                    AddParagraph(document, $"Цель: {campaign.TargetAmount:N2} ₽. Собрано: {campaign.CollectedAmount:N2} ₽ ({campaign.Progress:N0}%). " +
                                           $"Потрачено: {campaign.SpentAmount:N2} ₽. Остаток: {campaign.Balance:N2} ₽.", normal);

                    var payments = campaign.Payment.Where(p => p.Status == "Completed").OrderBy(p => p.Date).ToList();
                    if (payments.Count == 0)
                    {
                        AddParagraph(document, "Зачисленных пожертвований пока нет.", normal);
                    }
                    else
                    {
                        Word.Paragraph tableParagraph = document.Paragraphs.Add();
                        Word.Table table = document.Tables.Add(tableParagraph.Range, payments.Count + 1, 4);
                        table.Borders.Enable = 1;

                        table.Cell(1, 1).Range.Text = "Дата";
                        table.Cell(1, 2).Range.Text = "Донор";
                        table.Cell(1, 3).Range.Text = "Назначение";
                        table.Cell(1, 4).Range.Text = "Сумма, ₽";
                        table.Rows[1].Range.Bold = 1;

                        for (int row = 0; row < payments.Count; row++)
                        {
                            var payment = payments[row];
                            table.Cell(row + 2, 1).Range.Text = payment.Date.ToString("dd.MM.yyyy");
                            table.Cell(row + 2, 2).Range.Text = payment.DonorName;
                            table.Cell(row + 2, 3).Range.Text = payment.Name;
                            table.Cell(row + 2, 4).Range.Text = payment.Total.ToString("N2");
                        }
                        document.Paragraphs.Add();

                        var max = payments.OrderByDescending(p => p.Total).First();
                        AddParagraph(document, $"Крупнейшее пожертвование: {max.Total:N2} ₽ — {max.DonorName}, {max.Date:dd.MM.yyyy}",
                                     Word.WdBuiltinStyle.wdStyleSubtitle);
                    }

                    if (i < campaigns.Count - 1)
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }

                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Text = $"Сервис сбора пожертвований. Отчёт от {DateTime.Now:dd.MM.yyyy}";
                    headerRange.Font.Size = 10;
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                application.Visible = true;
                document.SaveAs2(docxPath);
                document.ExportAsFixedFormat(pdfPath, Word.WdExportFormat.wdExportFormatPDF);
                Messages.Info($"Отчёт сохранён на Рабочий стол:\n{docxPath}\n{pdfPath}");
            }
            catch (Exception ex)
            {
                ((Word._Application)application).Quit(false);
                Messages.ShowException(ex);
            }
        }

        private void ExcelButton_Click(object sender, RoutedEventArgs e)
        {
            var campaigns = Entities.GetContext().Campaign.OrderBy(c => c.Name).ToList();
            string path = Path.Combine(_folder, "Отчёт по сборам.xlsx");

            var application = new Excel.Application();
            try
            {
                application.DisplayAlerts = false;
                Excel.Workbook workbook = application.Workbooks.Add();
                Excel.Worksheet sheet = (Excel.Worksheet)workbook.Worksheets[1];
                sheet.Name = "Сборы";

                string[] headers = { "Сбор", "Организация", "Категория", "Статус",
                                     "Цель, ₽", "Собрано, ₽", "Потрачено, ₽", "Остаток, ₽", "Выполнено, %" };
                for (int col = 0; col < headers.Length; col++)
                    sheet.Cells[1, col + 1] = headers[col];

                int row = 2;
                foreach (var campaign in campaigns)
                {
                    sheet.Cells[row, 1] = campaign.Name;
                    sheet.Cells[row, 2] = campaign.Organization.Name;
                    sheet.Cells[row, 3] = campaign.Category.Name;
                    sheet.Cells[row, 4] = campaign.StatusText;
                    sheet.Cells[row, 5] = (double)campaign.TargetAmount;
                    sheet.Cells[row, 6] = (double)campaign.CollectedAmount;
                    sheet.Cells[row, 7] = (double)campaign.SpentAmount;
                    sheet.Cells[row, 8] = (double)campaign.Balance;
                    sheet.Cells[row, 9] = Math.Round(campaign.Progress, 1);
                    row++;
                }

                sheet.Cells[row, 1] = "ИТОГО";
                sheet.Range[$"E{row}"].Formula = $"=SUM(E2:E{row - 1})";
                sheet.Range[$"F{row}"].Formula = $"=SUM(F2:F{row - 1})";
                sheet.Range[$"G{row}"].Formula = $"=SUM(G2:G{row - 1})";
                sheet.Range[$"H{row}"].Formula = $"=SUM(H2:H{row - 1})";
                sheet.Range[$"A{row}", $"I{row}"].Font.Bold = true;

                Excel.Range header = sheet.Range["A1", "I1"];
                header.Font.Bold = true;
                header.Interior.Color = 11507713;
                header.Font.Color = 16777215;
                sheet.Range["E2", $"H{row}"].NumberFormat = "#,##0.00";
                sheet.Range["A1", $"I{row}"].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                sheet.Columns.AutoFit();

                application.Visible = true;
                workbook.SaveAs(path);
                Messages.Info($"Отчёт сохранён на Рабочий стол:\n{path}");
            }
            catch (Exception ex)
            {
                application.Quit();
                Messages.ShowException(ex);
            }
        }
    }
}