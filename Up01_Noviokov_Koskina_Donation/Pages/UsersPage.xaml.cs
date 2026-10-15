using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;


namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class UsersPage : Page
    {
        public UsersPage()
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
            var items = Entities.GetContext().User.ToList();
            string search = SearchBox.Text.Trim().ToLower(); 
            if (search != "")
                items = items.Where(u => u.FIO.ToLower().Contains(search) || u.Login.ToLower().Contains(search)).ToList();
            string role = ((ComboBoxItem)RoleFilterBox.SelectedItem).Tag.ToString();
            if (role != "")
                items = items.Where(u => u.Role == role).ToList();
            switch (SortBox.SelectedIndex)
            {
                case 0: items = items.OrderBy(u => u.FIO).ToList(); break;
                case 1: items = items.OrderByDescending(u => u.FIO).ToList(); break;
                case 2:  items = items.OrderBy(u => u.Login).ToList(); break;
            }
            ItemsGrid.ItemsSource = items;
            CountText.Text = $"Найдено: {items.Count}";
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserEditPage(null));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as User;
            NavigationService.Navigate(new UserEditPage(item));
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ItemsGrid.SelectedItems.Cast<User>().ToList();
            if (selected.Any(u => u.ID == Session.CurrentUser.ID))
            {
                Messages.Warning("Нельзя удалить свою учётную запись. Снимите с неё выделение");
                return;
            }
            int payments = selected.Sum(u => u.Payment.Count);
            string warning = payments > 0 ? $"\nВместе с ними будут удалены их пожертвования ({payments})." : "";
            if (DataHelper.Delete(selected, warning))
                UpdateList();
        }
    }
}
