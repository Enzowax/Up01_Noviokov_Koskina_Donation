using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Up01_Noviokov_Koskina_Donation.Services;


namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class UserEditPage : Page
    {
        private readonly User _item;
        private readonly bool _isNew;
        private string _photoPath;
        public UserEditPage(User item)
        {
            InitializeComponent();
            _isNew = item == null;
            _item = item ?? new User { Role = "User" };
            Title = _isNew ? "Новиый пользователь" : "Редактирование пользователя";
            HeaderText.Text = Title;
            PasswordLabel.Text = _isNew ? "Пароль *" : "Новый пароль (оставьте пустым, чтобы не менять)";
            LoginBox.Text = _item.Login ?? "";
            FioBox.Text = _item.FIO ?? "";
            RoleBox.SelectedIndex = _item.Role == "Admin" ? 1 : 0;
            _photoPath = _item.Photo;
            ShowPhoto();
        }
        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;
            LoginHint.Visibility= LoginBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
            FioHint.Visibility = FioBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        private void ShowPhoto()
        {
            PhotoImage.Source = null;
            if (string.IsNullOrEmpty(_photoPath) || !File.Exists(_photoPath))
                return;
            try
            {
                PhotoImage.Source = new BitmapImage(new Uri(_photoPath));
            }
            catch
            {
                Messages.Warning("Не удалось открыть файл как изображение");
                _photoPath = null;
            }
        }
        private void ChoosePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите фотографию",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp"
            };
            if (dialog.ShowDialog() == true)
            {
                _photoPath= dialog.FileName;
                ShowPhoto();
            }
        }
        private void ClearPhotoButton_Click(Object sender, RoutedEventArgs e)
        {
            _photoPath = null;
            ShowPhoto();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string[] fioParts = FioBox.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string role = ((ComboBoxItem)RoleBox.SelectedItem).Tag.ToString();
            string password = PasswordBox.Password;
            int id = _item.ID;

            var errors = new StringBuilder();
            if (login.Length < 3)
                errors.AppendLine("Логин должен содержать не менее 3 символов");
            else if (Entities.GetContext().User.Any(u => u.Login == login && u.ID != id))
                errors.AppendLine("Пользователь с таким логином уже существует");
            if (fioParts.Length < 2)
                errors.AppendLine("Введите ФИО полностью (как минимум фамилию и имя)");
            if(_isNew || password != "")
            {
                string passwordError = Security.CheckPassword(password);
                if (passwordError != "") 
                    errors.AppendLine(passwordError);
            }
            if (!_isNew && _item.ID == Session.CurrentUser.ID && role !="Admin")
                errors.AppendLine("Нальзя снять роль администратора с себя");
            if (errors.Length > 0)
            {
                Messages.Warning(errors.ToString());
                return;
            }
            _item.Login = login;
            _item.FIO = string.Join(" ", fioParts);
            _item.Role = role;
            _item.Photo = string.IsNullOrEmpty(_photoPath) ? null : _photoPath;
            if (password != "")
                _item.Password = Security.GetHash(password);
            if (DataHelper.Save(_item, _isNew))
                NavigationService.GoBack();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
