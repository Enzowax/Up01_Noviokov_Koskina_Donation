using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    /// <summary>
    /// Логика взаимодействия для CampaignsPage.xaml
    /// </summary>
    public partial class CampaignsPage : Page
    {
        public CampaignsPage()
        {
            InitializeComponent();
            var categories = Entities.GetContext().Category.OrderBy(c => c.Name).ToList();
            categories.Insert(0, new Category { ID = 0, Name = "Все категории" });
            CategoryFilterBox.ItemsSource = categories;
            CategoryFilterBox.SelectedIndex = 0;
        }
        private void Page_IsVisibleChanged(object sender, RoutedEventArgs e)
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
            var items = Entities.GetContext().Campaign.ToList();
            string search = SearchBox.Text.Trim().ToLower();
            if (search != "")
                items = items.Where(c => c.Name.ToLower().Contains(search)).ToList();
            string status = ((ComboBoxItem)StatusFilterBox.SelectedItem).Tag.ToString();
            if (status != "")
                items = items.Where(c => c.Status == status).ToList();
            var category = CategoryFilterBox.SelectedItem as Category;
            if (category != null && category.ID != 0) 
                items = items.Where(c => c.CategoryID == category.ID).ToList();

            switch (SortBox.SelectedIndex)
            {
                case 0: items = items.OrderBy(c => c.Name).ToList(); break;
                case 1: items = items.OrderByDescending(c => c.StartDate).ToList(); break;
                case 2: items = items.OrderByDescending(c => c.TargetAmount).ToList(); break;
                case 3: items = items.OrderByDescending(c => c.Progress).ToList(); break;
            }
            
            ItemsGrid.ItemsSource = items;
            CountText.Text = $"Найдено: {items.Count}";
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CampaignEditPage());
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as Campaign;
            NavigationService.Navigate(new CampaignEditPage(item));
        }
        private void DeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            var selected = ItemsGrid.SelectedItems.Cast<Campaign>().ToList();
            int payments = selected.Sum(c => c.Payment.Count);
            int reports = selected.Sum (c => c.Report.Count);
            string warning = payments + reports > 0 ? $"\nВместе со сборами будут удалены пожертвования ({payments}) и отчёты ({reports})." : "";
            if (DataHelper.Delete(selected, warning))
                UpdateList();
        }

    }
}
