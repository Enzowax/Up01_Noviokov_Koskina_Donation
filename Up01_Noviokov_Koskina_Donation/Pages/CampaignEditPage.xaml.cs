using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class CampaignEditPage : Page
    {
        private Campaign _item;
        private bool _isNew;

        public CampaignEditPage(Campaign item)
        {
            InitializeComponent();

            var context = Entities.GetContext();
            OrganizationBox.ItemsSource = context.Organization.OrderBy(o => o.Name).ToList();
            CategoryBox.ItemsSource = context.Category.OrderBy(c => c.Name).ToList();

            _isNew = item == null;
            _item = item ?? new Campaign();

            if (_isNew)
            {
                HeaderText.Text = "Новый сбор";
                Title = "Новый сбор";
                StartDatePicker.SelectedDate = DateTime.Today;
                StatusBox.SelectedIndex = 0;
            }
            else
            {
                HeaderText.Text = "Редактирование сбора";
                Title = "Редактирование сбора";
                NameBox.Text = _item.Name;
                DescriptionBox.Text = _item.Description;
                TargetBox.Text = _item.TargetAmount.ToString("0.##");
                OrganizationBox.SelectedValue = _item.OrganizationID;
                CategoryBox.SelectedValue = _item.CategoryID;
                StartDatePicker.SelectedDate = _item.StartDate;
                EndDatePicker.SelectedDate = _item.EndDate;
                StatusBox.SelectedValue = _item.Status;
            }
            UpdateHints();
        }

        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            UpdateHints();
        }

        private void UpdateHints()
        {
            if (!IsInitialized)
                return;
            NameHint.Visibility = NameBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            DescriptionHint.Visibility = DescriptionBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            TargetHint.Visibility = TargetBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            string name = NameBox.Text.Trim();
            string description = DescriptionBox.Text.Trim();

            if (OrganizationBox.SelectedValue == null)
                errors.AppendLine("Выберите организацию.");
            if (CategoryBox.SelectedValue == null)
                errors.AppendLine("Выберите категорию.");

            if (name == "")
                errors.AppendLine("Введите название сбора.");
            else if (name.Length > 150)
                errors.AppendLine("Название не должно быть длиннее 150 символов.");

            if (description == "")
                errors.AppendLine("Введите описание сбора.");
            else if (description.Length > 2000)
                errors.AppendLine("Описание не должно быть длиннее 2000 символов.");

            decimal target;
            if (!decimal.TryParse(TargetBox.Text.Trim(), out target))
                errors.AppendLine("Целевая сумма должна быть числом, например 50000 или 99,50.");
            else if (target < 0.01m || target > 1000000000m)
                errors.AppendLine("Целевая сумма должна быть от 0,01 до 1 000 000 000 руб.");

            DateTime? start = StartDatePicker.SelectedDate;
            DateTime? end = EndDatePicker.SelectedDate;
            if (start == null)
                errors.AppendLine("Укажите дату начала.");
            else if (end != null && end.Value < start.Value)
                errors.AppendLine("Дата окончания не может быть раньше даты начала.");

            if (StatusBox.SelectedValue == null)
                errors.AppendLine("Выберите статус.");

            if (errors.Length > 0)
            {
                Messages.Warning(errors.ToString());
                return;
            }

            _item.OrganizationID = (int)OrganizationBox.SelectedValue;
            _item.CategoryID = (int)CategoryBox.SelectedValue;
            _item.Name = name;
            _item.Description = description;
            _item.TargetAmount = target;
            _item.StartDate = start.Value;
            _item.EndDate = end;
            _item.Status = StatusBox.SelectedValue.ToString();

            if (DataHelper.Save(_item, _isNew))
                NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}