using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrganizationEditPage.xaml
    /// </summary>
    public partial class OrganizationEditPage : Page
    {
        private readonly Organization _item;
        private readonly bool _isNew;
        public OrganizationEditPage(Organization item)
        {
            InitializeComponent();
            _isNew = item == null;
            _item = item ?? new Organization();
            Title = _isNew ? "Новая организация" : "Редактирование организации";
            HeaderText.Text = Title;
            PhoneBox.PreviewTextInput += InputRules.Phone;
            NameBox.Text = _item.Name ?? "";
            DescriptionBox.Text = _item.Description ?? "";
            EmailBox.Text = _item.ContactEmail ?? "";
            PhoneBox.Text = _item.ContactPhone ?? "";
            AddressBox.Text = _item.Address ?? "";

        }

        private void Field_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;
            NameHint.Visibility = HintVisivility(NameBox.Text);
            DescriptionHint.Visibility = HintVisivility(DescriptionBox.Text);
            EmailHint.Visibility = HintVisivility(EmailBox.Text);
            PhoneHint.Visibility = HintVisivility(PhoneBox.Text);
            AddressHint.Visibility = HintVisivility(AddressBox.Text);
        }

        private static Visibility HintVisivility(string text)
        {

            return text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            string description = DescriptionBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string address = AddressBox.Text.Trim();
            int id = _item.ID;

            var errors = new StringBuilder();
            if (name == "")
                errors.AppendLine("Введите название организации");
            else if (Entities.GetContext().Organization.Any(o => o.Name == name && o.ID != id))
                errors.AppendLine("Организация с тиким названтем уже существует");

            if (description == "")
                errors.AppendLine("Введите описание");
            if (email == "")
                errors.AppendLine("Введите контактный e-mail");
            else if (!Check.IsEmail(email))
                errors.AppendLine("E-mail указан в неверном формате (пример: info@Fond.ru)");
            if (phone != "" && !Check.IsPhone(phone))
                errors.AppendLine("Телефон должег содержать 11 цифр и начинаться с +7 или 8");
            if (errors.Length > 0)
            {
                Messages.Warning(errors.ToString());
                return;
            }
            _item.Name = name;
            _item.Description = description;   
            _item.ContactEmail = email;
            _item.ContactPhone = phone == "" ? null : phone;
            _item.Address = address == "" ? null : address;

            if(DataHelper.Save(_item, _isNew)) 
                NavigationService.GoBack();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

    }
}
