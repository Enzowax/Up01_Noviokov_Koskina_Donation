using Microsoft.Office.Core;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class OrganizationsPage : Page
    {
        public OrganizationsPage()
        {
            InitializeComponent();
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

            var items = Entities.GetContext().Organization.ToList();
            string search = SearchBox.Text.Trim().ToLower();
            if (search != "")
                items = items.Where(o => o.Name.ToLower().Contains(search) || o.ContactEmail.ToLower().Contains(search)).ToList();
            if (ActiveOnlyBox.IsChecked == true)
                items = items.Where(o => o.Campaign.Any(c => c.Status == "Active")).ToList();
            switch (SortBox.SelectedIndex)
            {
                case 0: items = items.OrderBy(o => o.Name).ToList(); break;
                case 1: items = items.OrderByDescending(o => o.Name).ToList(); break;
                case 2: items = items.OrderByDescending(o => o.Campaign.Count).ToList(); break;
            }

            ItemsGrid.ItemsSource = items;
            CountText.Text = $"Найдено: {items.Count}";
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)

        {
            NavigationService.Navigate(new OrganizationEditPage(null));
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)

        {
            var item = (sender as Button).DataContext as Organization;
            NavigationService.Navigate(new OrganizationEditPage(item));

        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)

        {
            var selected = ItemsGrid.SelectedItems.Cast<Organization>().ToList();
            int campaigns = selected.Sum(o => o.Campaign.Count);
            string warning = campaigns > 0
            ? $"\nВместе с ними будут удалены сборы этих организаций ({campaigns}), их пожертвования и отчёты." : "";
            if (DataHelper.Delete(selected, warning))
                UpdateList();
        }
    }
}