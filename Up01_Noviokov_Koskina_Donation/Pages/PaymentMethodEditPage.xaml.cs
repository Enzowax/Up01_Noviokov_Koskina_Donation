using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class PaymentMethodEditPage : Page
    {
        private readonly PaymentMethod _item;

        public PaymentMethodEditPage(PaymentMethod item)
        {
            InitializeComponent();
            _item = item;

            if (_item == null)
            {
                Title = "Новый способ оплаты";
            }
            else
            {
                Title = "Редактирование способа оплаты";
                NameBox.Text = _item.Name;
            }
        }

        private void Field_TextChanged(object sender, TextChangedEventArgs e)
        {
            NameHint.Visibility = NameBox.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            int id = _item != null ? _item.ID : 0;

            if (name == "")
            {
                Messages.Warning("Введите название способа оплаты");
                return;
            }

            bool exists = Entities.GetContext().PaymentMethod.Any(c => c.Name == name && c.ID != id);
            if (exists)
            {
                Messages.Warning("Способ оплаты с таким названием уже существует");
                return;
            }

            bool isNew = _item == null;
            PaymentMethod paymentMethod = isNew ? new PaymentMethod() : _item;
            paymentMethod.Name = name;

            if (DataHelper.Save(paymentMethod, isNew))
                NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}