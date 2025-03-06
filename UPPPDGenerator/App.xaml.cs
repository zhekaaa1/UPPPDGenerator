using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using UPPPDGenerator.Managers;
using UPPPDGenerator.Properties;
using UPPPDGenerator.Windows;

namespace UPPPDGenerator
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Console.WriteLine($"ПОЛУЧЕНО ИЗ НАСТРОЕК: {Settings.Default.lang}");
            Console.WriteLine($"ПОЛУЧЕНО ИЗ НАСТРОЕК --- ID ПОЛЬЗОВАТЕЛЯ: {Settings.Default.userid}");
            string lang = Settings.Default.lang;
            if (!string.IsNullOrEmpty(lang))
            {
                LangManager.SetLanguage(lang);
            }
            Window mainWindow;
            if (Settings.Default.userid != 0)
            {
                UserManager userManager = new UserManager();
                await userManager.SetLogonUser(Settings.Default.userid);
                mainWindow = new MainWin();
            }
            else
            {
                mainWindow = new MainWindow();
            }
            mainWindow.Show();
        }
    }
}
