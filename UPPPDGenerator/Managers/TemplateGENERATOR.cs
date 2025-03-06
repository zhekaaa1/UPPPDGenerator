using System;
using x2 = DocumentFormat.OpenXml.Packaging;
using System.Text.Json;
using System.IO;
using UPPPDGenerator.DocumentSettings;
using System.Windows;

namespace UPPPDGenerator.Managers
{
    public class TemplateGENERATOR
    {
        public static async void BeginWithoutTitle(string titleListPath, DocumentSettings.Settings documentSettings)
        {
            string docxPath = titleListPath;
            string jsonPath = $"D:\\Templates\\{PreparingTemplate.Name}.json";

            // Извлекаем OpenXML
            string openXml = GetOpenXmlFromDocx(docxPath);
            if (PreparingTemplate.PasswordHash != "ns")
            {
                Console.WriteLine("пришло НЕ NS");
                PasswordManager passwordManager = new PasswordManager();
                PreparingTemplate.PasswordHash = passwordManager.ComputeSHA256Hash(PreparingTemplate.PasswordHash);
            }
            // Создаем JSON-структуру
            var template = new
            {
                TemplateName = PreparingTemplate.Name,
                TemplatePasswordHash = PreparingTemplate.PasswordHash,
                TemplateDescription = PreparingTemplate.Description,
                CreatedAt = PreparingTemplate.Createdat,
                CreatedBy = PreparingTemplate.CreatedByAuthorId,
                UseTitlePage = true,
                TitlePageXml = openXml,
                DocumentSettings = PreparingTemplate.Settings,
            };

            TemplateManager templateManager = new TemplateManager();
            bool success = await templateManager.CreateTemplate(template.TemplateName, template.TemplateDescription, template.CreatedBy, template.TemplatePasswordHash);
            if (success)
            {
                File.WriteAllText(jsonPath, JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true }));
                MessageBox.Show("Готово! Данные сохранены в " + jsonPath);
            }
            // Сохраняем в JSON

        }
        public static string GetOpenXmlFromDocx(string docxPath)
        {
            using (x2.WordprocessingDocument doc = x2.WordprocessingDocument.Open(docxPath, false))
            {
                return doc.MainDocumentPart.Document.OuterXml; // Сырые данные OpenXML
            }
        }
    }
}
