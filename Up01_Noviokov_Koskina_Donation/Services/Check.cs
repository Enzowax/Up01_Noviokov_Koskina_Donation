using System.Linq;
using System.Text.RegularExpressions;

namespace Up01_Noviokov_Koskina_Donation.Services
{
    public static class Check
    {
        public static bool IsEnail(string text)
        {
            return Regex.IsMatch(text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
        public static bool IsPhone(string text)
        {
            string digits = new string(text.Where(char.IsDigit).ToArray());
            return digits.Length == 11 && ( digits[0] == '7' || digits[0] == '8');
        }
    }
}
