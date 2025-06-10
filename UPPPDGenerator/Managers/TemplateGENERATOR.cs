using System.Windows;
using UPPPDGenerator.DocumentSettings;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
namespace UPPPDGenerator.Managers
{
    public class TemplateGENERATOR
    {
        public static async Task GenerateTemplate 
            (TemplateJsonStructure template,
            TemplateAccessMode templateAccessMode)
        {

            //в Properties пока что значение "C:\\Templates\\"
            string safeTemplateName = string.Concat(template.TemplateName.Split(Path.GetInvalidFileNameChars()));
            string fileTargetPath = Path.Combine(Properties.Settings.Default.TemplatesDirectory, safeTemplateName + ".ugt");

            string safeTemplateName1 = string.Concat(template.TemplateName.Split(Path.GetInvalidFileNameChars()));
            string fileTargetPath1 = Path.Combine(Properties.Settings.Default.TemplatesDirectory, safeTemplateName1 + ".json");

            switch (templateAccessMode)
            {
                case TemplateAccessMode.Private:
                    template.TemplateIdFromDatabase = -1;
                    break;
                case TemplateAccessMode.Public:
                    template.TemplateIdFromDatabase = 0;
                    break;
                default:
                    template.TemplateIdFromDatabase = await GetTemplateIdFromCreated(template);
                    break;
            }
            new TemplateManager().EncryptData(template, fileTargetPath);
            if (Properties.Settings.Default.SaveJson)
            {
                string jsonContent = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(fileTargetPath1, jsonContent);
            }
            MessageBox.Show("Готово! Данные сохранены в " + fileTargetPath);
        }
        public static async Task<int> GetTemplateIdFromCreated(TemplateJsonStructure templateJson)
        {
            TemplateManager templateManager = new TemplateManager();
            Template created = await templateManager.AddTemplate(templateJson.TemplateName, templateJson.TemplateDescription, Properties.Settings.Default.LogonUserId);
            return created.Id;
        }
    }
    public enum TemplateAccessMode
    {
        Private = -1,       
        Public = 0,         
        Restricted = 1      
    }
}