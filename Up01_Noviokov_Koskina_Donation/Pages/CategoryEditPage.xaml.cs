using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Up01_Noviokov_Koskina_Donation.Services;

namespace Up01_Noviokov_Koskina_Donation.Pages
{
    public partial class CategoryEditPage : Page
    {
        private readonly Category _item;

        public CategoryEditPage(Category item)
        {
            InitializeComponent();
            _item = item;

            if (_item == null)
            {
                Title = "Новая категория";
            }
            else
            {
                Title = "Редактирование категории";
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
                Messages.Warning("Введите название категории");
                return;
            }

            bool exists = Entities.GetContext().Category.Any(c => c.Name == name && c.ID != id);
            if (exists)
            {
                Messages.Warning("Категория с таким названием уже существует");
                return;
            }

            bool isNew = _item == null;
            Category category = isNew ? new Category() : _item;
            category.Name = name;

            if (DataHelper.Save(category, isNew))
                NavigationService.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}