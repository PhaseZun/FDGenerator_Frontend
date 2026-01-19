  using MyApp.Web.Models.Auth;
  using System.Text.Json; 

 

namespace MyApp.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(HttpClient httpClient,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
             string  apiUrl = _configuration["ExternalApis:AuthApiBaseUrl"]+"Login";
            Console.WriteLine("request arpi"+apiUrl);
            var response = await _httpClient.PostAsJsonAsync(apiUrl, request);
            string jsonRequest = JsonSerializer.Serialize(request);
            Console.WriteLine("Request being sent to API:");
            Console.WriteLine(jsonRequest);
            if (response.IsSuccessStatusCode)
            {
                //var result = JsonSerializer.Deserialize<LoginResponse>(content);
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result!;
            }
            
            return new LoginResponse {UserId = 0,
             Username = null,
             Token = null};
        }
    }
}
