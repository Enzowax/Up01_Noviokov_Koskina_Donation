using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace Up01_Noviokov_Koskina_Donation.Services
{
    public static class Security
    {
        public static string GetHash(string text)
        {
            using (var sha  = SHA1.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                return string.Concat(bytes.Select(b => b.ToString("X2")));
            }
        }
        public static string CheckPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return "Пароль должен быть не короче 6 символов";

            bool hasDigits = false;
            bool hasLetters = false;
            foreach (char c in password) 
            {
                if (c >= '0' && c <= '9')
                    hasDigits = true;
                else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    hasLetters = true;
                else if (c == ' ' || c > '-')
                    return "В пароле можно использовать латинские буквы, цифры и знаки без пробелов";
            }
            if (!hasDigits || !hasLetters)
            {
                return "Пароль должен содержать хотя бы одну латинскую букву и одну цифру";
            }
            return "";
        }
        public static readonly Random _random = new Random();

        public static string GenerateCapcha (int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var result = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[_random.Next(chars.Length)]);
            }
                return result.ToString();
        }
    }
}
