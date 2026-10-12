using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;


namespace Up01_Noviokov_Koskina_Donation.Services
{
    public static class InputRules
    {
        public static void Digits(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
        public static void Money (object sender, TextCompositionEventArgs e)
        {
            var box = (TextBox)sender;
            bool isFirstComma = e.Text == "," && !box.Text.Contains(",");
            e.Handled = !(e.Text.All(char.IsDigit) || isFirstComma);
        }
        public static void Letters (object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(c => char.IsLetter(c) || c == ' ' || c == '-');
        }
        public static void Login(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All (c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || char.IsDigit(c) || c == '_'); 
        }
        public static void Phone(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(c => char.IsDigit(c) || "+()-".IndexOf(c) >= 0);
        }
    }
}
