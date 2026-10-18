using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class ReportEditPage : Page
    {
        private readonly Report _item;
        private readonly bool _isNew;

        public ReportEditPage(Report item)
        {
            InitializeComponent();
            _isNew = item == null;
            _item = item ?? new Report { Date = DateTime.Today };

            Title = _isNew ? "Новый отчёт о расходе" : "Редактирование отчёта";
            HeaderText.Text = Title;

            SpentBox.PreviewTextInput += InputRules.Money;
            ReportDatePicker.DisplayDateEnd = DateTime.Today;

            CampaignBox.ItemsSource = Entities.GetContext().Campaign.OrderBy(c => c.Name).ToList();

            CampaignBox.SelectedValue = _item.CampaignID;
            ReportDatePicker.SelectedDate = _item.Date;
            NameBox.Text = _item.Name ?? "";
            DescriptionBox.Text = _item.Description ?? "";
            SpentBox.Text = _isNew ? "" : _item.SpentAmount.ToString("0.##");
            FilePathBox.Text = _item.FilePath ?? "";
        }

        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;
            NameHint.Visibility = HintVisibility(NameBox.Text);
            DescriptionHint.Visibility = HintVisibility(DescriptionBox.Text);
            SpentHint.Visibility = HintVisibility(SpentBox.Text);
        }

        private static Visibility HintVisibility(string text)
        {
            return text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл отчёта",
                Filter = "Документы и сканы|*.pdf;*.docx;*.xlsx;*.jpg;*.png|Все файлы|*.*"
            };
            if (dialog.ShowDialog() == true)
                FilePathBox.Text = dialog.FileName;
        }

        private void ClearFileButton_Click(object sender, RoutedEventArgs e)
        {
            FilePathBox.Text = "";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            string description = DescriptionBox.Text.Trim();
            string filePath = FilePathBox.Text;
            DateTime? date = ReportDatePicker.SelectedDate;
            var campaign = CampaignBox.SelectedItem as Campaign;

            var errors = new StringBuilder();
            if (campaign == null)
                errors.AppendLine("Выберите сбор");
            if (date == null)
                errors.AppendLine("Укажите дату");
            else if (date > DateTime.Today)
                errors.AppendLine("Дата расхода не может быть в будущем");
            if (name == "")
                errors.AppendLine("Введите название");
            if (description == "")
                errors.AppendLine("Введите описание");

            if (!decimal.TryParse(SpentBox.Text.Trim(), out decimal spent))
                errors.AppendLine("Введите потраченную сумму числом");
            else if (spent < 0.01m || spent > 1000000000m)
                errors.AppendLine("Сумма должна быть от 0,01 до 1 000 000 000 ₽");
            else if (Math.Round(spent, 2) != spent)
                errors.AppendLine("У суммы может быть не больше 2 знаков после запятой");

            if (filePath.Length > 500)
                errors.AppendLine("Путь к файлу слишком длинный (больше 500 символов)");

            if (errors.Length > 0)
            {
                Messages.Warning(errors.ToString());
                return;
            }

            decimal otherReports = campaign.Report.Where(r => r != _item).Sum(r => r.SpentAmount);
            if (otherReports + spent > campaign.CollectedAmount)
            {
                string question = $"Расходы по сбору составят {otherReports + spent:N2} ₽, а собрано только {campaign.CollectedAmount:N2} ₽.\nВсё равно сохранить?";
                if (!Messages.Ask(question))
                    return;
            }

            _item.Campaign = campaign;
            _item.Date = date.Value;
            _item.Name = name;
            _item.Description = description;
            _item.SpentAmount = spent;
            _item.FilePath = filePath == "" ? null : filePath;

            if (DataHelper.Save(_item, _isNew))
                NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}