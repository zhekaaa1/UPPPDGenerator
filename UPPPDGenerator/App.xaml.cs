using System;
using System.IO;
using System.Net.Http;
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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string lang = Settings.Default.lang;
            if (!string.IsNullOrEmpty(lang))
            {
                LangManager.SetLanguage(lang);
            }
            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                if (File.Exists(filePath) && filePath.EndsWith(".ugt", StringComparison.OrdinalIgnoreCase))
                {
                    var template = new TemplateManager().DecryptData(filePath);
                    if (template != null)
                    {
                        new FillOutTheDocument(template).Show();
                        return;
                    }
                    else
                    {
                        MainWin win = new MainWin();
                        win.Show();
                        win.ErrorContainer.Show($"Произошла ошибка при открытии шаблона: Файл шаблона повреждён", UPPPDGenerator.Windows.Elements.ErrorType.Critical, 5000);
                        return;
                    }
                }
            }
            new MainWin().Show();
        }
        public static async Task<bool> IsServerAvailable()
        {
            try
            {
                var pingResult = await new HttpClient().GetAsync("http://localhost:5121/api/templates");
                return pingResult.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
