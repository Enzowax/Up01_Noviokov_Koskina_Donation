using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class PaymentsPage : Page
    {
        public PaymentsPage()
        {
            InitializeComponent();

            var campaigns = Entities.GetContext().Campaign.OrderBy(c => c.Name).ToList();
            campaigns.Insert(0, new Campaign { ID = 0, Name = "Все сборы" });
            CampaignFilterBox.ItemsSource = campaigns;
            CampaignFilterBox.SelectedIndex = 0;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                UpdateList();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            UpdateList();
        }

        private void UpdateList()
        {
            if (!IsInitialized)
                return;

            SearchHint.Visibility = SearchBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

            var items = Entities.GetContext().Payment.ToList();

            string search = SearchBox.Text.Trim().ToLower();
            if (search != "")
                items = items.Where(p => p.DonorName.ToLower().Contains(search)
                                      || p.Name.ToLower().Contains(search)
                                      || p.Campaign.Name.ToLower().Contains(search)).ToList();

            string status = ((ComboBoxItem)StatusFilterBox.SelectedItem).Tag.ToString();
            if (status != "")
                items = items.Where(p => p.Status == status).ToList();

            var campaign = CampaignFilterBox.SelectedItem as Campaign;
            if (campaign != null && campaign.ID != 0)
                items = items.Where(p => p.CampaignID == campaign.ID).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 0: items = items.OrderByDescending(p => p.Date).ToList(); break;
                case 1: items = items.OrderBy(p => p.Date).ToList(); break;
                case 2: items = items.OrderByDescending(p => p.Total).ToList(); break;
            }

            ItemsGrid.ItemsSource = items;

            decimal completed = items.Where(p => p.Status == "Completed").Sum(p => p.Total);
            CountText.Text = $"Найдено: {items.Count}, зачислено {completed:N2} ₽";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PaymentEditPage(null));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as Payment;
            NavigationService.Navigate(new PaymentEditPage(item));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ItemsGrid.SelectedItems.Cast<Payment>().ToList();
            if (DataHelper.Delete(selected, ""))
                UpdateList();
        }
    }
}