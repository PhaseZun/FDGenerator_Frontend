    using MyApp.Web.Models.Auth;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
using System.Text.Json; 
    using System.Net.Http.Json;
 

namespace MyApp.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            const string apiUrl = "http://localhost:5269/api/auth/login";
            Console.WriteLine("request arpi"+apiUrl);
            var response = await _httpClient.PostAsJsonAsync(apiUrl, request);
            string jsonRequest = JsonSerializer.Serialize(request);
            Console.WriteLine("Request being sent to API:");
            Console.WriteLine(jsonRequest);
            if (response.IsSuccessStatusCode)
            {
                //var result = JsonSerializer.Deserialize<LoginResponse>(content);
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result ?? new LoginResponse { Success = false, Message = "Invalid API response" };
            }
            
            return new LoginResponse { Success = false, Message = "API call failed" };
        }
    }
}
