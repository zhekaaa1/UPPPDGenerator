using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UPPPDGenerator.Managers;
using UPPPDGenerator.Properties;

namespace UPPPDGenerator
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Console.WriteLine($"ПОЛУЧЕНО ИЗ НАСТРОЕК: {Settings.Default.lang}");
            string lang = Settings.Default.lang;
            if (!string.IsNullOrEmpty(lang))
            {
                LangManager.SetLanguage(lang);
            }
        }
    }
}
