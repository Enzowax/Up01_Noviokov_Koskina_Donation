using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Up01_Noviokov_Koskina_Donation.Services
{
    public static class ThemeManager
    {
        public static string CurrentTheme { get; private set; } = "Light";
        public static void Apply(string theme)
        {
            if (theme != "Light" && theme != "Dark")
                theme = "Light";
            var dictionary = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Styles/{theme}Theme.xaml")
            };
            var merged = Application.Current.Resources.MergedDictionaries;
            merged.Clear();
            merged.Add(dictionary);

            CurrentTheme = theme;
            Properties.Settings.Default.Theme = theme;
            Properties.Settings.Default.Save();
        }
        public static void Toggle()
        {
            Apply(CurrentTheme == "Light" ? "Dark" : "Light");
        }
        public static void LoadSaved()
        {
            Apply(Properties.Settings.Default.Theme);
        }
    }
}
