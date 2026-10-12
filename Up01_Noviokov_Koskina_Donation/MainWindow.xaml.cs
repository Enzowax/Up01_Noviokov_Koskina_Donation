using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Up01_Noviokov_Koskina_Donation.Pages;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _clockTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new AuthPage());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, args) => ClockText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            _clockTimer.Start();
            ClockText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private void MainFrame_ContentRendered(object sender, EventArgs e)
        {
            Page page = MainFrame.Content as Page;
            if (page == null)
                return;

            PageTitle.Text = page.Title;
            Title = page.Title + " — Сервис сбора пожертвований";

            bool loggedIn = Session.CurrentUser != null;
            bool isAdmin = loggedIn && Session.CurrentUser.Role == "Admin";

            if (page is AuthPage || page is AdminPage || (page is CatalogPage && !isAdmin))
            {
                while (MainFrame.RemoveBackEntry() != null)
                {
                }
            }

            LogoutButton.Visibility = loggedIn ? Visibility.Visible : Visibility.Collapsed;
            BackButton.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
            UserText.Text = loggedIn
                ? $"{Session.CurrentUser.FIO} ({Session.CurrentUser.RoleText})"
                : "Гость. Войдите в систему.";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Toggle();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Messages.Ask("Выйти из учётной записи?"))
                return;

            Session.CurrentUser = null;
            MainFrame.Navigate(new AuthPage());
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !Messages.Ask("Закрыть приложение?");
            if (!e.Cancel)
                _clockTimer.Stop();
        }
    }
}