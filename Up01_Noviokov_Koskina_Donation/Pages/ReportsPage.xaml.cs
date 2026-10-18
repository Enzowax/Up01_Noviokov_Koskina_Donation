using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
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

            var items = Entities.GetContext().Report.ToList();

            string search = SearchBox.Text.Trim().ToLower();
            if (search != "")
                items = items.Where(r => r.Name.ToLower().Contains(search)
                                      || r.Description.ToLower().Contains(search)).ToList();

            var campaign = CampaignFilterBox.SelectedItem as Campaign;
            if (campaign != null && campaign.ID != 0)
                items = items.Where(r => r.CampaignID == campaign.ID).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 0: items = items.OrderByDescending(r => r.Date).ToList(); break;
                case 1: items = items.OrderBy(r => r.Date).ToList(); break;
                case 2: items = items.OrderByDescending(r => r.SpentAmount).ToList(); break;
            }

            ItemsGrid.ItemsSource = items;
            CountText.Text = $"Найдено: {items.Count}, потрачено {items.Sum(r => r.SpentAmount):N2} ₽";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ReportEditPage(null));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as Report;
            NavigationService.Navigate(new ReportEditPage(item));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ItemsGrid.SelectedItems.Cast<Report>().ToList();
            if (DataHelper.Delete(selected, ""))
                UpdateList();
        }
    }
}