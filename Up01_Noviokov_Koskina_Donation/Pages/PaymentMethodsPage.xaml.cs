using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class PaymentMethodsPage : Page
    {
        private bool _loaded = false;

        public PaymentMethodsPage()
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
            var query = Entities.GetContext().PaymentMethod.AsQueryable();

            string search = SearchBox.Text.Trim();
            if (search != "")
                query = query.Where(c => c.Name.Contains(search));

            switch (SortBox.SelectedIndex)
            {
                case 1:
                    query = query.OrderByDescending(c => c.Name);
                    break;
                case 2:
                    query = query.OrderByDescending(c => c.Payment.Count);
                    break;
                default:
                    query = query.OrderBy(c => c.Name);
                    break;
            }

            var list = query.ToList();
            PaymentMethodsGrid.ItemsSource = list;
            CountText.Text = $"Записей: {list.Count}";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PaymentMethodEditPage(null));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var paymentMethod = (PaymentMethod)((Button)sender).DataContext;
            NavigationService.Navigate(new PaymentMethodEditPage(paymentMethod));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = PaymentMethodsGrid.SelectedItems.Cast<PaymentMethod>().ToList();

            int payments = selected.Sum(c => c.Payment.Count);
            string warning = payments > 0
                ? $"\nВнимание: вместе с ними будут удалены платежи, сделанные этими способами ({payments} шт.)."
                : "";

            if (DataHelper.Delete(selected, warning))
                UpdateList();
        }
    }
}