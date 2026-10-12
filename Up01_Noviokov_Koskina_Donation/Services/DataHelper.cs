using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;

namespace Up01_Noviokov_Koskina_Donation.Services
{
    public static class DataHelper
    {
        public static bool Save<T>(T item, bool isNew) where T : class
        {
            var context = Entities.GetContext();
            try
            {
                if (isNew)
                    context.Set<T>().Add(item);
                context.SaveChanges();
                Messages.Info("Данные сохранены");
                return true;
            }
            catch (Exception ex)
            {
                DiscardChanges();
                Messages.ShowException(ex);
                return false;
            }
        }
        public static bool Delete<T>(List<T> items, string warnings) where T : class
        {
            if (items.Count == 0)
            {
                Messages.Warning("Выделите в таблице записи для удаления (несколько — с зажатым Ctrl)");
                return false;
            }
            if (!Messages.Ask($"Вы действительно хотите удалить выбранные записи ({items.Count})?{warnings}"))
                return false;
            var context = Entities.GetContext();
            try
            {
                context.Set<T>().RemoveRange(items);
                context.SaveChanges();
                Messages.Info("Записи удалены");
                return true;
            }
            catch (Exception ex) 
            {
                DiscardChanges();
                Messages.ShowException(ex);
                return false;
            }
        }
        public static void DiscardChanges()
        {
            var context = Entities.GetContext();
            foreach (var entry  in context.ChangeTracker.Entries().ToList())
            {
                if (entry.State == EntityState.Added)
                    entry.State = EntityState.Detached;
                else if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    entry.Reload();
            }
        }
    }
}
