using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace UPPPDGenerator.Managers
{
    public class UserManager
    {
        private HttpClient _httpClient;
        private const string MainDomain = "http://localhost:5121/api/users/";
        public UserManager()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(MainDomain);
        }
        public async Task<User> Authorize(string email, string password)
        {
            PasswordManager passwordManager = new PasswordManager();
            LoginRequest request = new LoginRequest()
            {
                Email = email,
                PasswordHash = passwordManager.ComputeSHA256Hash(password)
            };
            string json = JsonSerializer.Serialize(request);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("login/", content);
                response.EnsureSuccessStatusCode(); // Выбросит исключение при ошибке

                string responseString = await response.Content.ReadAsStringAsync();
                
                return JsonSerializer.Deserialize<User>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка запроса: {ex.Message}");
                return null;
            }
        }
        public async Task<User> GetInfo(int Id)
        {
            try
            {
                var response = await _httpClient.GetAsync(Id.ToString());
                response.EnsureSuccessStatusCode();
                string responseString = await response.Content.ReadAsStringAsync();
                User user = JsonSerializer.Deserialize<User>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return user;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка запроса: {ex.Message}");
                return null;
            }
        }
        public void Logout()
        {
            Properties.Settings.Default.userid = 0;
            Properties.Settings.Default.Save();
            Console.WriteLine("!!!!!!!ОПЛЬЗОАВТЕЛЬ ВЫШЕЛ!!!! В ПРОПЕРТЯХ" + Properties.Settings.Default.userid);
            LogonUser.Id = 0;
            LogonUser.Email = "";
            LogonUser.FullName = "";
            LogonUser.CreatedAt = DateTime.Now;
        }
        public class LoginRequest
        {
            public string Email { get; set; }
            public string PasswordHash { get; set; }
        }
        public async Task SetLogonUser(int Id)
        {
            User user = await GetInfo(Id);
            Properties.Settings.Default.userid = user.Id;
            Properties.Settings.Default.Save();
            LogonUser.Id = user.Id;
            LogonUser.Email = user.Email;
            LogonUser.CreatedAt = user.CreatedAt;
            LogonUser.FullName = user.FullName;
        }
    }
}
