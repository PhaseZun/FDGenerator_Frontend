using Microsoft.AspNetCore.Mvc;
using MyApp.Web.Models.FD;
using System.Text.Json;

namespace MyApp.Web.Controllers
{
    public class FDListController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FDListController> _logger;
        private const string ApiBaseUrl = "http://localhost:5269/api/FDList";


        public FDListController(IHttpClientFactory httpClientFactory, ILogger<FDListController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string token)
        {
            try
            {
                _logger.LogInformation("➡️ [Frontend] Calling API to fetch FD list with token: {Token}", token);

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{ApiBaseUrl}/list/{token}");

                _logger.LogInformation("⬅️ [Backend] Received response: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("⚠️ Unable to fetch FD list from API. Status: {StatusCode}", response.StatusCode);
                    ViewBag.ErrorMessage = "Unable to fetch FD list from API.";
                    return View(new List<FDModel>());
                }

                var jsonString = await response.Content.ReadAsStringAsync();
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
        
        public async Task<IActionResult> DownloadPdf(int id)
        {
            _logger.LogInformation("➡️ [Frontend] Request to download PDF for FD ID: {Id}", id);


            if (id <= 0)
            {
                _logger.LogWarning("⚠️ Invalid FD ID: {Id}", id);
                return BadRequest("Invalid FD ID.");
            }

            var apiUrl = $"{ApiBaseUrl}/downloadpdf/{id}";
            _logger.LogInformation("🔗 Calling backend API: {ApiUrl}", apiUrl);
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            

            using var client = new HttpClient(handler);

            var response = await client.GetAsync(apiUrl);
            _logger.LogInformation("⬅️ [Backend] Received response for PDF download: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("⚠️ PDF not found for FD ID: {Id}. Status: {StatusCode}", id, response.StatusCode);
                return NotFound($"PDF not found for FD ID: {id}");
            }

            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"FD_{id}.pdf";

            _logger.LogInformation("✅ Successfully downloaded PDF for FD ID: {Id}", id);
            return File(pdfBytes, "application/pdf", fileName);
        }

    }
}
