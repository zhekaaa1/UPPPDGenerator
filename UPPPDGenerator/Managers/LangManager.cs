using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UPPPDGenerator.Managers
{
    public static class LangManager
    {
        public static void SetLanguage(string lang)
        {
            Properties.Settings.Default.lang = lang;
            Properties.Settings.Default.Save();
            Console.WriteLine($"НАЗНАЧЕН lang: {lang}, в Properties - {Properties.Settings.Default.lang}");

            var dictionary = new ResourceDictionary();
            dictionary.Source = new Uri($"Strings.{lang}.xaml", UriKind.Relative);

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }
    }
}
