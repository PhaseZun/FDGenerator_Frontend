using Microsoft.AspNetCore.Mvc;
using MyApp.Web.Models.FD;
using System.Net;
using System.Text.Json;

namespace MyApp.Web.Controllers
{
    public class FDListController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FDListController> _logger;
        private readonly IConfiguration _configuration;
        


        public FDListController(IHttpClientFactory httpClientFactory, ILogger<FDListController> logger,IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }
        
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("Token");
            var userId = HttpContext.Session.GetString("UserId");
            try
            {
                _logger.LogInformation("➡️ [Frontend] Calling API to fetch FD list with token: {Token}", token);

                var client = _httpClientFactory.CreateClient();
                var URL=_configuration["ExternalApis:FDListApiBaseUrl"]+$"list/{userId}/{token}";
                var response = await client.GetAsync(URL);

                _logger.LogInformation("⬅️ [Backend] Received response: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("⚠️ Unable to fetch FD list from API. Status: {StatusCode}", response.StatusCode);
                    ViewBag.ErrorMessage = "Unable to fetch FD list from API.";
                    return View(new List<FDModel>());
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                        {
                            // 🔴 Token expired
                            HttpContext.Session.Clear();
                            return RedirectToAction("Login", "Account");
                        }
                }
                var fdList = JsonSerializer.Deserialize<List<FDModel>>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                _logger.LogInformation("✅ Successfully fetched {Count} FDs from API.", fdList?.Count ?? 0);
                return View(fdList ?? new List<FDModel>());
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Network error while calling FD list API.");
                ViewBag.ErrorMessage = $"Network error: {ex.Message}";
                return View(new List<FDModel>());
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(string userId,int fdId,string token)
        {
            _logger.LogInformation("➡️ [Frontend] Request to download PDF for FD ID: {fdId}", fdId);


            if (fdId <= 0)
            {
                _logger.LogWarning("⚠️ Invalid FD ID: {fdId}", fdId);
                return BadRequest("Invalid FD ID.");
            }
            var apiUrl=_configuration["ExternalApis:FDListApiBaseUrl"]+$"downloadpdf/{userId}/{fdId}/{token}";
            //var apiUrl = $"{ApiBaseUrl}/downloadpdf/{userId}/{fdId}/{token}";
            _logger.LogInformation("🔗 Calling backend API: {ApiUrl}", apiUrl);
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            

            using var client = new HttpClient(handler);

            var response = await client.GetAsync(apiUrl);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                        {
                            // 🔴 Token expired
                            HttpContext.Session.Clear();
                            return RedirectToAction("Login", "Account");
                        }
                }
            _logger.LogInformation("⬅️ [Backend] Received response for PDF download: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("⚠️ PDF not found for FD ID: {Id}. Status: {StatusCode}", fdId, response.StatusCode);
                return NotFound($"PDF not found for FD ID: {fdId}");
            }

            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"FD_{fdId}.pdf";

            _logger.LogInformation("✅ Successfully downloaded PDF for FD ID: {Id}", fdId);
            return File(pdfBytes, "application/pdf", fileName);
        }

    }
}
