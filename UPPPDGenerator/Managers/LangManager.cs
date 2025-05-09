﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UPPPDGenerator.Managers
{
    public static class LangManager
    {
        /// <summary>
        /// Возвратит 1, если установлен язык Русский, 2 - если английский
        /// </summary>
        /// <returns></returns>
        public static int GetLangNum()
        {
            return Properties.Settings.Default.lang == "ru-RU" ? 2 : 1;
        }
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
