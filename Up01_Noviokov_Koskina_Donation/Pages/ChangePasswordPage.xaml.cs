using System;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordPage.xaml
    /// </summary>
    public partial class ChangePasswordPage : Page
    {
        public ChangePasswordPage()
        {
            InitializeComponent();
            LoginBox.PreviewTextInput += InputRules.Login;
        }
        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            LoginHint.Visibility = HintVisibility(LoginBox.Text);
            OldPasswordHint.Visibility = HintVisibility(OldPasswordBox.Password);
            NewPasswordHint.Visibility = HintVisibility(NewPasswordBox.Password);
            ConfirmHint.Visibility = HintVisibility(ConfirmBox.Password);
        }
        private static Visibility HintVisibility(string text)
        {
            return text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;

            if (login == "" || oldPassword == "")
            {
                Messages.Warning("Введите логин и текущий пароль");
                return;
            }
            User user;
            try
            {
                user = AuthService.Authenticate(login, oldPassword);
            }
            catch (Exception ex)
            {
                Messages.Warning(ex.Message);
                return;
            }
            if (user == null)
            {
                Messages.Error("Неверный логин или текущий пароль");
                return;
            }

            string error = Security.CheckPassword(newPassword);
            if (error == "" && newPassword != ConfirmBox.Password)
                error = "Новый пароль и повтор не совпадают";
            if (error == "" && newPassword == oldPassword)
                error = "Новый пароль должен отличаться от текущего";
            if (error != "")
            {
                Messages.Warning(error);
                return;
            }
            user.Password = Security.GetHash(newPassword);
            if (DataHelper.Save(user, false))
                NavigationService.GoBack();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
