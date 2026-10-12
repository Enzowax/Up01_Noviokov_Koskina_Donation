using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Up01_Noviokov_Koskina_Donation.Services
{
    public static class Messages
    {
        public static void Info( string text)
        {
            MessageBox.Show(text, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static void Warning(string text)
        {
            MessageBox.Show(text, "Проверьте данные", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static void Error(string text)
        {
            MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static bool Ask(string text)
        {
            return MessageBox.Show(text, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
        public static void ShowException(Exception ex)
        {
            if (ex is DbEntityValidationException validation)
            {
                var lines = validation.EntityValidationErrors.SelectMany(v => v.ValidationErrors).Select(v => v.ErrorMessage);
                Error("Данные не прошли проверку:\n" + string.Join("\n", lines));
                return;
            }
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }   
            Error("Не удалось выполнить операцию:\n" +ex.Message);
            return;
        }

    }
}
