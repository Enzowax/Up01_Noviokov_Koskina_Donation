using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();

            var context = Entities.GetContext();

            var categories = context.Category.OrderBy(c => c.Name).ToList();
            categories.Insert(0, new Category { ID = 0, Name = "Все категории" });
            CategoryFilterBox.ItemsSource = categories;
            CategoryFilterBox.SelectedIndex = 0;

            var organizations = context.Organization.OrderBy(o => o.Name).ToList();
            organizations.Insert(0, new Organization { ID = 0, Name = "Все организации" });
            OrganizationFilterBox.ItemsSource = organizations;
            OrganizationFilterBox.SelectedIndex = 0;
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

            var allItems = Entities.GetContext().Campaign.Where(c => c.Status != "Draft").ToList();
            var items = allItems;

            string search = SearchBox.Text.Trim().ToLower();
            if (search != "")
                items = items.Where(c => c.Name.ToLower().Contains(search)
                                      || c.Description.ToLower().Contains(search)).ToList();

            var category = CategoryFilterBox.SelectedItem as Category;
            if (category != null && category.ID != 0)
                items = items.Where(c => c.CategoryID == category.ID).ToList();

            var organization = OrganizationFilterBox.SelectedItem as Organization;
            if (organization != null && organization.ID != 0)
                items = items.Where(c => c.OrganizationID == organization.ID).ToList();

            if (ActiveOnlyBox.IsChecked == true)
                items = items.Where(c => c.Status == "Active").ToList();

            switch (SortBox.SelectedIndex)
            {
                case 0: items = items.OrderBy(c => c.Name).ToList(); break;
                case 1: items = items.OrderByDescending(c => c.Progress).ToList(); break;
                case 2: items = items.OrderBy(c => c.TargetAmount).ToList(); break;
                case 3: items = items.OrderByDescending(c => c.TargetAmount).ToList(); break;
                case 4: items = items.OrderBy(c => c.EndDate ?? DateTime.MaxValue).ToList(); break;
            }

            CampaignsList.ItemsSource = items;
            CountText.Text = $"Показано {items.Count} из {allItems.Count}";
            EmptyText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            CategoryFilterBox.SelectedIndex = 0;
            OrganizationFilterBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
            ActiveOnlyBox.IsChecked = false;
        }

        private void CampaignsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var campaign = CampaignsList.SelectedItem as Campaign;
            if (campaign == null)
                return;
            CampaignsList.SelectedItem = null;
            NavigationService.Navigate(new CampaignDetailsPage(campaign));
        }
    }
}