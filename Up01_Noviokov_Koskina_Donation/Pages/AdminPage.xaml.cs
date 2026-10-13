using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            HelloText.Text = $"Здравствйте, {Session.CurrentUser.FIO}";
        }

    private void Menu_Click(object sender, RoutedEventArgs e)
        {
            string section = ((Button)sender).Tag.ToString();
            Page page;
            switch (section)
            {

                case "Users": page = new UsersPage(); 
                    break;
                case "Organizations": page = new OrganizationsPage(); 
                    break;
                case "Categories": page = new CategoriesPage(); 
                    break;
                case "Campaigns": page = new CampaignsPage();
                    break;
                case "PaymentMethods": page = new PaymentMethodsPage();
                    break;
                case "Payments": page = new PaymentsPage();
                    break;
                case "Reports": page = new ReportsPage(); 
                    break;
                case "Diagram": page = new DiagramPage(); 
                    break;
                case "Catalog": page = new CatalogPage(); 
                    break;
                default: 
                    return;

            }
            NavigationService.Navigate(page);
        }
    }
}
