using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using UPPPDGenerator.DocumentSettings;
using System.Collections.ObjectModel;

namespace UPPPDGenerator.Managers
{
    public class TemplateViewModel
    {
        public string FilePath { get; set; } // для открытия по клику
        public string TemplateName { get; set; }
        public string AuthorName { get; set; }
        public string CreatedAtFormatted { get; set; }
        public TemplateJsonStructure TemplateDecoded { get; set; }
    }
    public class TemplateLoader
    {
        public ObservableCollection<TemplateViewModel> Templates { get; set; } = new ObservableCollection<TemplateViewModel>();
        public IEnumerable<TemplateViewModel> LoadTemplates()
        {
            string templatesDirectory = $@"{Properties.Settings.Default.TemplatesDirectory}";
            if (!Directory.Exists(templatesDirectory))
                Directory.CreateDirectory(templatesDirectory);

            foreach (var filePath in Directory.GetFiles(templatesDirectory, "*.ugt"))
            {
                try
                {
                    var decrypted = new TemplateManager().DecryptData(filePath);
                    if (decrypted == null) continue;
                    string authorName = decrypted.CreatedBy.Id == Properties.Settings.Default.LogonUserId ? "Вы" : decrypted.CreatedBy.FullName;
                    string createdDate = decrypted.CreatedAt.ToString("dd.MM.yyyy");

                    Templates.Add(new TemplateViewModel
                    {
                        FilePath = filePath,
                        TemplateName = decrypted.TemplateName,
                        AuthorName = authorName,
                        CreatedAtFormatted = createdDate,
                        TemplateDecoded = decrypted,
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке шаблона {filePath}: {ex.Message}");
                    continue;
                }
            }
            return Templates;
        }
    }
}
