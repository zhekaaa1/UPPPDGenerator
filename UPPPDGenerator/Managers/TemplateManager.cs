using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UPPPDGenerator.DocumentSettings;

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
        public async Task<Template> AddTemplate(string name, string description, int createdBy)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            var requestBody = new PostAddTemplateRequest
            {
                Name = name,
                Description = description,
                CreatedByUserId = createdBy,
            };
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("createTemplate", content);
                statusCode = response.StatusCode;
                response.EnsureSuccessStatusCode();
                string responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<TemplateDTO>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return new Template { Id = responseObject.Id };
            }
            catch
            {
                return CheckErrors(statusCode, 1);
            }
        }
        public Template CheckErrors(HttpStatusCode httpStatusCode, int requestCategory)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return new Template
                {
                    Id = -1,
                    Name = "Проверьте подключение к сети интернет или войдите войти как гость."
                };
            switch (httpStatusCode)
            {
                case HttpStatusCode.InternalServerError:
                    return new Template()
                    {
                        Id = -1,
                        Name = "Возникли ошибки на сервере. Попробуйте позже или войдите как гость."
                    };
                case HttpStatusCode.Conflict:
                    switch (requestCategory)
                    {
                        case 1:
                            return new Template()
                            {
                                Id = 0,
                                Name = "Ошибка добавления: не удалось найти авторизованного пользователя."
                            };
                        case 2:
                            return new Template()
                            {
                                Id = 0,
                                Name = "Ошибка обновления"
                            };
                        case 3:
                            return new Template()
                            {
                                Id = 0,
                                Name = "Код просрочен или недействителен."
                            };
                        case 4:
                            return new Template()
                            {
                                Id = 0,
                                Name = "Пользователь не найден."
                            };
                        default:
                            return new Template()
                            {
                                Id = 0,
                                Name = "Неизвестная стадия авторизации. Повторите попытку позже."
                            };
                    }
                default:
                    return new Template()
                    {
                        Id = -1,
                        Name = "Сервер недоступен. Попробуйте позже или войдите как гость."
                    };
            }
        }

        public async Task<string> GetTemplatesByAuthor(int authorId)
        {
            var requestBody = new { AuthorId = authorId };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/watchbyauthor", content);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetTemplatesByAccess(int userId)
        {
            var requestBody = new { UserId = userId };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/watchByAccesses", content);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<bool> AddAccessToTemplate(int templateId)
        {
            var requestBody = new PostTemplateAccessRequest
            {
                TemplateId = templateId,
                UserId = Properties.Settings.Default.LogonUserId,
                ExpiresAt = DateTime.MaxValue,
                IsAuthor = false,
                IsPermanent = true
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}addAccess", content);
            return response.IsSuccessStatusCode;
        }



        private static readonly byte[] Key = Encoding.UTF8.GetBytes(Properties.Settings.Default.AesKey);
        private static readonly byte[] IV = Encoding.UTF8.GetBytes(Properties.Settings.Default.AesIV);
        public void EncryptData(TemplateJsonStructure template, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
            byte[] plainBytes = Encoding.UTF8.GetBytes(jsonString);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = Key;
                aes.IV = IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    File.WriteAllBytes(filePath, ms.ToArray());
                }
            }
        }

        public TemplateJsonStructure DecryptData(string filePath)
        {
            byte[] encryptedBytes = File.ReadAllBytes(filePath);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using (MemoryStream ms = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs, Encoding.UTF8))
                        {
                            string jsonString = reader.ReadToEnd();
                            return JsonSerializer.Deserialize<TemplateJsonStructure>(jsonString) ?? new TemplateJsonStructure();
                        }
                    }
                }
            }
        }
        public class TemplateDTO
        { 
            public int Id { get; set; }
        }
        public class PostAddTemplateRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int? CreatedByUserId { get; set; }
        }
        public class PostTemplateAccessRequest
        {
            public int TemplateId { get; set; }
            public int UserId { get; set; }
            public DateTime ExpiresAt { get; set; }
            public bool IsPermanent { get; set; }
            public bool IsAuthor { get; set; }
        }
        public class UpdateTemplateRequest
        {
            public int TemplateId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public class DeleteTemplateRequest
        {
            public int TemplateId { get; set; }
        }
    }
}
