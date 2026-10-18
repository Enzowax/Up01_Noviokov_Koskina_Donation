using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class CampaignDetailsPage : Page
    {
        private readonly Campaign _campaign;

        public CampaignDetailsPage(Campaign campaign)
        {
            InitializeComponent();
            _campaign = campaign;
            Title = campaign.Name;

            AmountBox.PreviewTextInput += InputRules.Money;
            MethodBox.ItemsSource = Entities.GetContext().PaymentMethod.OrderBy(m => m.Name).ToList();
            MethodBox.SelectedIndex = 0;

            bool active = _campaign.Status == "Active";
            DonatePanel.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
            ClosedText.Visibility = active ? Visibility.Collapsed : Visibility.Visible;

            ShowCampaign();
        }

        private void ShowCampaign()
        {
            NameText.Text = _campaign.Name;
            CategoryText.Text = _campaign.Category.Name;
            OrganizationText.Text = "Организатор: " + _campaign.Organization.Name;
            DescriptionText.Text = _campaign.Description;

            CampaignProgress.Value = _campaign.Progress;
            ProgressText.Text = _campaign.ProgressText;
            StatusLine.Text = $"{_campaign.StatusText} · {_campaign.PeriodText}";
            SpentText.Text = $"Потрачено: {_campaign.SpentAmount:N2} ₽. Остаток: {_campaign.Balance:N2} ₽";

            var reports = _campaign.Report.OrderByDescending(r => r.Date).ToList();
            ReportsGrid.ItemsSource = reports;
            NoReportsText.Visibility = reports.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            var payments = _campaign.Payment
                .Where(p => p.Status == "Completed")
                .OrderByDescending(p => p.Date)
                .Take(10)
                .ToList();
            PaymentsGrid.ItemsSource = payments;
            NoPaymentsText.Visibility = payments.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Amount_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;
            AmountHint.Visibility = AmountBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            var method = MethodBox.SelectedItem as PaymentMethod;
            if (method == null)
                errors.AppendLine("Выберите способ оплаты");

            decimal amount;
            if (!decimal.TryParse(AmountBox.Text.Trim(), out amount))
                errors.AppendLine("Введите сумму числом, например 500 или 150,50");
            else if (amount < 0.01m || amount > 1000000000m)
                errors.AppendLine("Сумма должна быть от 0,01 до 1 000 000 000 ₽");
            else if (Math.Round(amount, 2) != amount)
                errors.AppendLine("У суммы может быть не больше 2 знаков после запятой");

            if (errors.Length > 0)
            {
                Messages.Warning(errors.ToString());
                return;
            }

            if (!Messages.Ask($"Пожертвовать {amount:N2} ₽ на сбор «{_campaign.Name}»?"))
                return;

            var payment = new Payment
            {
                User = Session.CurrentUser,
                Campaign = _campaign,
                PaymentMethod = method,
                Date = DateTime.Today,
                Name = "Пожертвование на сбор",
                Num = 1,
                Price = amount,
                Status = "Completed",
                IsAnonymous = AnonymousBox.IsChecked == true
            };

            if (DataHelper.Save(payment, true))
            {
                AmountBox.Text = "";
                AnonymousBox.IsChecked = false;
                ShowCampaign();
                Messages.Info("Спасибо! Пожертвование принято.");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}