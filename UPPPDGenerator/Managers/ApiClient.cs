using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UPPPDGenerator.Managers
{
    public class ApiManager
    {
        private readonly HttpClient _httpClient;

        public ApiManager()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5121/api/") // Твой адрес API
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("users/register", content).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка регистрации: {ex.Message}");
                return false;
            }
        }

        // Пример GET запроса
        public async Task<IEnumerable<User>> GetUsersAsync()
        {
           var response = await _httpClient.GetAsync("users/all");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var users = JsonSerializer.Deserialize<List<User>>(result, options);
                return users ?? new List<User>();
            }
            else
            {
                // Обработка ошибки
                return new List<User>();
            }
        }
    }
    public class RegisterRequest
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
    }

    public class VerificationCodeResponse
    {
        public string VerificationCode { get; set; }
    }
}
