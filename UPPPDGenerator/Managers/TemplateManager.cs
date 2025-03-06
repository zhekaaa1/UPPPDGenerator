using DocumentFormat.OpenXml.Drawing.Diagrams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UPPPDGenerator.Managers
{
    public class TemplateManager
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5121/api/templates/";

        public TemplateManager()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        // Метод для создания шаблона
        public async Task<bool> CreateTemplate(string name, string description, int createdBy, string passwordHash)
        {
            var requestBody = new
            {
                Name = name,
                Description = description,
                CreatedByUserId = createdBy,
                PasswordHash = string.IsNullOrEmpty(passwordHash) ? "ns" : passwordHash
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("createTemplate", content);
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Content);
            string responseString = await response.Content.ReadAsStringAsync();

            var Otvet = JsonSerializer.Deserialize<object>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            string json = JsonSerializer.Serialize(Otvet);
            Console.WriteLine(json);

            return response.IsSuccessStatusCode;
        }

        // Метод для получения шаблонов по автору
        public async Task<string> GetTemplatesByAuthor(int authorId)
        {
            var requestBody = new { AuthorId = authorId };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/watchbyauthor", content);

            return await response.Content.ReadAsStringAsync();
        }

        // Метод для получения шаблонов, к которым у пользователя есть доступ
        public async Task<string> GetTemplatesByAccess(int userId)
        {
            var requestBody = new { UserId = userId };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/watchByAccesses", content);

            return await response.Content.ReadAsStringAsync();
        }
        public class PostTemplateRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int CreatedByUserId { get; set; }
            public string PasswordHash {  get; set; }
        }
    }
}
