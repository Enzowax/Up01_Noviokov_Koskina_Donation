using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class PaymentEditPage : Page
    {
        private readonly Payment _item;
        private readonly bool _isNew;

        public PaymentEditPage(Payment item)
        {
            InitializeComponent();
            _isNew = item == null;
            _item = item ?? new Payment { Date = DateTime.Today, Num = 1, Status = "Completed" };

            Title = _isNew ? "Новое пожертвование" : "Редактирование пожертвования";
            HeaderText.Text = Title;

            NumBox.PreviewTextInput += InputRules.Digits;
            PriceBox.PreviewTextInput += InputRules.Money;
            PaymentDatePicker.DisplayDateEnd = DateTime.Today;

            var context = Entities.GetContext();
            UserBox.ItemsSource = context.User.OrderBy(u => u.FIO).ToList();
            CampaignBox.ItemsSource = context.Campaign.OrderBy(c => c.Name).ToList();
            MethodBox.ItemsSource = context.PaymentMethod.OrderBy(m => m.Name).ToList();

            UserBox.SelectedValue = _item.UserID;
            CampaignBox.SelectedValue = _item.CampaignID;
            MethodBox.SelectedValue = _item.PaymentMethodID;
            PaymentDatePicker.SelectedDate = _item.Date;
            NameBox.Text = _item.Name ?? "";
            NumBox.Text = _item.Num.ToString();
            PriceBox.Text = _isNew ? "" : _item.Price.ToString("0.##");
            StatusBox.SelectedValue = _item.Status;
            AnonymousBox.IsChecked = _item.IsAnonymous;
        }

        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;
            NameHint.Visibility = NameBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (int.TryParse(NumBox.Text, out int num) && decimal.TryParse(PriceBox.Text, out decimal price))
                TotalText.Text = $"Итого: {num * price:N2} ₽";
            else
                TotalText.Text = "Итого: —";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            DateTime? date = PaymentDatePicker.SelectedDate;

            var errors = new StringBuilder();
            if (UserBox.SelectedItem == null)
                errors.AppendLine("Выберите донора");
            if (CampaignBox.SelectedItem == null)
                errors.AppendLine("Выберите сбор");
            if (MethodBox.SelectedItem == null)
                errors.AppendLine("Выберите способ оплаты");

            if (date == null)
                errors.AppendLine("Укажите дату");
            else if (date > DateTime.Today)
                errors.AppendLine("Дата пожертвования не может быть в будущем");

            if (name == "")
                errors.AppendLine("Введите назначение платежа");

            if (!int.TryParse(NumBox.Text, out int num) || num < 1 || num > 1000)
                errors.AppendLine("Количество — целое число от 1 до 1000");

            if (!decimal.TryParse(PriceBox.Text.Trim(), out decimal price))
                errors.AppendLine("Введите сумму числом, например 500 или 150,50");
            else if (price < 0.01m || price > 1000000000m)
                errors.AppendLine("Сумма должна быть от 0,01 до 1 000 000 000 ₽");
            else if (Math.Round(price, 2) != price)
                errors.AppendLine("У суммы может быть не больше 2 знаков после запятой");

            if (StatusBox.SelectedItem == null)
                errors.AppendLine("Выберите статус");

            if (errors.Length > 0)
            {
                Messages.Warning(errors.ToString());
                return;
            }

            _item.User = (User)UserBox.SelectedItem;
            _item.Campaign = (Campaign)CampaignBox.SelectedItem;
            _item.PaymentMethod = (PaymentMethod)MethodBox.SelectedItem;
            _item.Date = date.Value;
            _item.Name = name;
            _item.Num = num;
            _item.Price = price;
            _item.Status = StatusBox.SelectedValue.ToString();
            _item.IsAnonymous = AnonymousBox.IsChecked == true;

            if (DataHelper.Save(_item, _isNew))
                NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}