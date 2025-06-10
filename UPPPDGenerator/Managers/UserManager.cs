using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            LoginRequest request = new LoginRequest()
            {
                Email = email,
                PasswordHash = password
            };
            string json = JsonSerializer.Serialize(request);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("login/", content);
                httpStatusCode = response.StatusCode;
                response.EnsureSuccessStatusCode();
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                var user = JsonSerializer.Deserialize<User>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                SetLogonUser(user);
                return user;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка запроса: {ex.Message}");
                return CheckErrors(httpStatusCode, 1);
            }
        }
        public async Task<User> Register(string fullname, string email, string password)
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            RegisterRequest registerRequest = new RegisterRequest()
            {
                FullName = fullname,
                Email = email,
                PasswordHash = password
            };
            string json = JsonSerializer.Serialize(registerRequest);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("register/", content);
                httpStatusCode = response.StatusCode;
                response.EnsureSuccessStatusCode(); // выбросит исключение при других ошибках
                string responseString = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return user;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка запроса: {ex.Message}");
                return CheckErrors(httpStatusCode, 2);  
            }
        }
        /// <summary>
        /// Принимает параметры статуса ошибки выполнения запроса <paramref name="httpStatusCode"/> и стадии авторизации <paramref name="authState"/>
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="authState"></param>
        /// <returns>
        /// <seealso cref="User"/> с полями:
        /// <list type="string">
        /// <item>
        /// <c>Id = 0</c>, если произошла критическая ошибка
        /// </item>
        /// <item>
        /// <c>Id = -1</c>, если произошла ошибка на стороне сервера, или интернета нет.
        /// </item>
        /// <item>
        /// <c>FullName</c> будет являться сообщением ошибки.
        /// </item>
        /// </list>
        /// </returns>
        public User CheckErrors(HttpStatusCode httpStatusCode, int authState)
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) 
                return new User 
                { 
                    Id = -1, 
                    FullName = "Проверьте подключение к сети интернет или войдите войти как гость." 
                };
            switch (httpStatusCode)
            {
                case HttpStatusCode.InternalServerError:
                    return new User()
                    {
                        Id = -1,
                        FullName = "Возникли ошибки на сервере. Попробуйте позже или войдите как гость."
                    };
                case HttpStatusCode.Conflict:
                    switch (authState)
                    {
                        case 1:
                            return new User()
                            {
                                Id = 0,
                                FullName = "Неверная почта или пароль."
                            };
                        case 2:
                            return new User()
                            {
                                Id = 0,
                                FullName = "Пользователь с такой почтой уже существует."
                            };
                        case 3:
                            return new User()
                            {
                                Id = 0,
                                FullName = "Код просрочен или недействителен."
                            };
                        case 4:
                            return new User()
                            {
                                Id = 0,
                                FullName = "Пользователь не найден."
                            };
                        default:
                            return new User()
                            {
                                Id = 0,
                                FullName = "Неизвестная стадия авторизации. Повторите попытку позже."
                            };
                    }
                default:
                    return new User()
                    {
                        Id = -1,
                        FullName = "Сервер недоступен. Попробуйте позже или войдите как гость."
                    };
            }
        }
        /// <summary>
        /// Проверяет код <paramref name="code"/> пользователя с идентификатором <paramref name="userId"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns><seealso cref="VerifyResult"/> с <c>Success = true</c>, если валидация пройдена, <c>false</c>, если иное.</returns>
        public async Task<VerifyResult> VerifyUser(User user, string code)
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            VerificationRequest verificationRequest = new VerificationRequest()
            {
                UserId = user.Id,
                Code = code
            };
            string json = JsonSerializer.Serialize(verificationRequest);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("verify", content);
                httpStatusCode = response.StatusCode;
                response.EnsureSuccessStatusCode(); // выбросит исключение при других ошибках
                SetLogonUser(user);
                VerifyResult verifyResult = new VerifyResult()
                {
                    Success = true
                };
                return verifyResult;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка запроса: {ex.Message}");
                return new VerifyResult()
                {
                    Success = false,
                    User = CheckErrors(httpStatusCode, 2)
                };
            }
        }
        public async Task<User> GetInfo(int id)
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            GetInfoRequest getInfoRequest = new GetInfoRequest()
            {
                UserId = id
            };
            string json = JsonSerializer.Serialize(getInfoRequest);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("getinfo", content);
                httpStatusCode = response.StatusCode;
                response.EnsureSuccessStatusCode(); // выбросит исключение при других ошибках
                string responseString = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(responseString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return user;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Произошла ошибка: " + ex.Message);
                return CheckErrors(httpStatusCode, 4);
            }
        }
        /// <summary>
        /// Удаляет пользователя из локальных настроек.
        /// </summary>
        public void Logout()
        {
            Properties.Settings.Default.LogonUserId = 0;
            Properties.Settings.Default.LogonUserFullName = "";
            Properties.Settings.Default.LogonUserCreatedAt = DateTime.Now;
            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// Добавляет пользователя в локальные настройки.
        /// </summary>
        /// <param name="user"></param>
        public void SetLogonUser(User user)
        {
            Properties.Settings.Default.LogonUserId = user.Id;
            Properties.Settings.Default.LogonUserFullName = user.FullName;
            Properties.Settings.Default.LogonUserCreatedAt = user.CreatedAt;
            Properties.Settings.Default.Save();
        }
    }
    public class GetInfoRequest
    {
        public int UserId { get; set; }
    }
    public class VerifyResult
    {
        public bool Success { get; set; }
        public User User { get; set; }
        public string Message => User?.FullName;
    }
    public class LoginRequest
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
    public class VerificationRequest
    {
        public int UserId { get; set; }
        public string Code { get; set; }
    }
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
