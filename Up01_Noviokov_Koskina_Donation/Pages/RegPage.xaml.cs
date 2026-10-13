using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            LoginBox.PreviewTextInput += InputRules.Login;
            FioBox.PreviewTextInput += InputRules.Letters;
        }
        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            LoginHint.Visibility = HintVisibility(LoginBox.Text);
            FioHint.Visibility = HintVisibility(FioBox.Text);
            PasswordHint.Visibility = HintVisibility(PasswordBox.Password);
            ConfirmHint.Visibility = HintVisibility(ConfirmBox.Password);

        }
        private static Visibility HintVisibility(string text)
        {
            return text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string fio = FioBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirm = ConfirmBox.Password;
            string role = ((ComboBoxItem)RoleBox.SelectedItem).Tag.ToString();
            var errors = new StringBuilder();
            if (login.Length < 3)
                errors.AppendLine("Логин должен содержать не менее 3 символов");
            else if (Entities.GetContext().User.Any(u => u.Login == login))
                errors.AppendLine("Пользователь с таким логином уже существует");
            string[] fioParts = fio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (fioParts.Length < 2)
                errors.AppendLine("Введите ФИО полностью (как минимум фамилию и имя");
            string passwordError = Security.CheckPassword(password);
            if (passwordError != "")
                errors.AppendLine(passwordError);
            else if (password != confirm)
                errors.AppendLine("Пароли не совподают");

            if (errors.Length > 0)
            {
                Messages.Warning(errors.ToString());
                return;
            }
            var user = new User
            {
                Login = login,
                FIO = string.Join(" ", fioParts),
                Password = Security.GetHash(password),
                Role = role,
            };
            if (DataHelper.Save(user, true))
                NavigationService.GoBack();


        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
