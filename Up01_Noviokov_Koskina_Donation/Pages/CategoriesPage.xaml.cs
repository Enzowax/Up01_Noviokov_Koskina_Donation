using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class CategoriesPage : Page
    {
        private bool _loaded = false;

        public CategoriesPage()
        {
            InitializeComponent();
            SortBox.SelectedIndex = 0;
            _loaded = true;
            UpdateList();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                UpdateList();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
                return;

            SearchHint.Visibility = SearchBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            UpdateList();
        }

        private void UpdateList()
        {
            var query = Entities.GetContext().Category.AsQueryable();

            string search = SearchBox.Text.Trim();
            if (search != "")
                query = query.Where(c => c.Name.Contains(search));

            switch (SortBox.SelectedIndex)
            {
                case 1:
                    query = query.OrderByDescending(c => c.Name);
                    break;
                case 2:
                    query = query.OrderBy(c => c.ID);
                    break;
                default:
                    query = query.OrderBy(c => c.Name);
                    break;
            }

            var list = query.ToList();
            CategoriesGrid.ItemsSource = list;
            CountText.Text = $"Записей: {list.Count}";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CategoryEditPage(null));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var category = (Category)((Button)sender).DataContext;
            NavigationService.Navigate(new CategoryEditPage(category));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = CategoriesGrid.SelectedItems.Cast<Category>().ToList();

            int campaigns = selected.Sum(c => c.Campaign.Count);
            string warning = campaigns > 0
                ? $"\nВнимание: вместе с категориями будут удалены сборы ({campaigns} шт.), а также их платежи и отчёты."
                : "";

            if (DataHelper.Delete(selected, warning))
                UpdateList();
        }
    }
}