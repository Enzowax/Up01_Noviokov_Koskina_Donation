using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private int _failedAttempts = 0;
        private string _captchaText = "";
        private readonly Random _random = new Random();
        public AuthPage()
        {
            InitializeComponent();
            LoginBox.PreviewTextInput += InputRules.Login;
        }

        private void LoginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoginHint.Visibility = LoginBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordHint.Visibility = PasswordBox.Password.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        public void CaptchaBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CaptchaHint.Visibility = CaptchaBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaPanel.Visibility == Visibility.Visible)
                return;
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            if (login == "" || password == "")
            {
                Messages.Warning("Введите логин и пароль");
                return;
            }
            User user;
            try
            {
                user = AuthService.Authenticate(login, password);
            }
            catch (Exception ex)
            {
                Messages.ShowException(ex);
                return;
            }
            if (user == null)
            {
                _failedAttempts++;
                if (_failedAttempts >= 3)
                {
                    Messages.Error("Неверный логин или пароль. Попытка 3 из 3.\nПодтвердите, что вы не робот.");
                    ShowCaptcha(true);
                }
                else
                {
                    Messages.Error($"Неверный логин или пароль. Попытка {_failedAttempts} из 3"); 
                }
                return;
            }
            _failedAttempts = 0;
            Session.CurrentUser = user;
            if (user.Role == "Admin")
                NavigationService.Navigate(new AdminPage());
            else
                NavigationService.Navigate(new CatalogPage());

        }

        private void RegButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegPage());
        }

        private void ChangePsswordButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ChangePasswordPage());
        }
        private void ShowCaptcha(bool show)
        {
            LoginPanel.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            CaptchaPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            if (show)
                DrawCaptcha();
        }
        private void DrawCaptcha()
        {
            _captchaText = Security.GenerateCapcha(5);
            CaptchCanvas.Children.Clear();
            CaptchaBox.Text = "";

            Brush[] colors = { Brushes.DarkBlue, Brushes.DarkRed, Brushes.DarkGreen, Brushes.Black };
            for (int i = 0; i < _captchaText.Length; i++)
            {
                var letter = new TextBlock
                {
                    Text = _captchaText[i].ToString(),
                    FontSize = _random.Next(24, 32),
                    FontWeight = FontWeights.Bold,
                    Foreground = colors[_random.Next(colors.Length)],
                    RenderTransform = new RotateTransform(_random.Next(-25, 26))
                };
                Canvas.SetLeft(letter, 12 + i * 36);
                Canvas.SetTop(letter, _random.Next(2, 19));
                CaptchCanvas.Children.Add(letter);
            }
            for (int i = 0; i < 6; i++)
            {
                var line = new Line
                {
                    X1 = _random.Next(0, 200),
                    Y1 = _random.Next(0, 60),
                    X2 = _random.Next(0, 200),
                    Y2 = _random.Next(0, 60),
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1.5
                };
                CaptchCanvas.Children.Add(line);
            }
        }
        private void CaptchaCheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaBox.Text == _captchaText)
            {
                Messages.Info("Проверка пройдена. Повторите вход.");
                _failedAttempts = 0;
                ShowCaptcha(false);
            }
            else
            {
                Messages.Error("Символы введены неверно. Попробуйте еще раз.");
                DrawCaptcha();
            }
        }
        private void CaptchaRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            DrawCaptcha();
        }
    }
}
