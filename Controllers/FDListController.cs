// using Microsoft.AspNetCore.Mvc;
// using System.Collections.Generic;
// using System.Net.Http;
// using System.Threading.Tasks;
// using MyApp.Web.Models.FD;
// using System.Text.Json;

// namespace MyApp.Web.Controllers
// {
//     public class FDListController : Controller
//     {
//         private readonly IHttpClientFactory _httpClientFactory;

//         public FDListController(IHttpClientFactory httpClientFactory)
//         {
//             _httpClientFactory = httpClientFactory;
//         }

//         // Display FD list
//          public async Task<IActionResult> Index()
//         {
//             var client = _httpClientFactory.CreateClient();

//             var response = await client.GetAsync("http://localhost:5269/api/FDList/list");
//             if (!response.IsSuccessStatusCode)
//             {
//                 return View(new List<FDModel>()); // empty list if fail
//             }

//             var jsonString = await response.Content.ReadAsStringAsync();

//             var fdList = JsonSerializer.Deserialize<List<FDModel>>(jsonString, new JsonSerializerOptions
//             { 
//                 PropertyNameCaseInsensitive = true
//             });

//             return View(fdList);
//         }

//         public async Task<IActionResult> DownloadPdf(string fileName)
//         {
//             if (string.IsNullOrEmpty(fileName))
//                 return BadRequest("Invalid PDF file name.");

//             // Replace with your API URL
//             var apiUrl = $"https://localhost:5269/api/pdf/download/{fileName}";

//            // var client = _httpClientFactory.CreateClient();
//              var handler = new HttpClientHandler
//                     {
//                         ServerCertificateCustomValidationCallback = 
//                             HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//                     };

//             var client = new HttpClient(handler);
//             var response = await client.GetAsync(apiUrl);

//             if (!response.IsSuccessStatusCode)
//                 return NotFound("PDF not found on server.");

//             var pdfStream = await response.Content.ReadAsStreamAsync();
//             return File(pdfStream, "application/pdf", fileName);
//         }
//     }
// }
using Microsoft.AspNetCore.Mvc;
using MyApp.Web.Models.FD;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


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

        /// <summary>
        /// Fetch and display the list of Fixed Deposits.
        /// </summary>
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
                // Log ex (for real-world cases)
                _logger.LogError(ex, "❌ Network error while calling FD list API.");
                ViewBag.ErrorMessage = $"Network error: {ex.Message}";
                return View(new List<FDModel>());
            }
        }

        /// <summary>
        /// Download a PDF for a specific FD by ID.
        /// </summary>
        /// <param name="id">Fixed Deposit ID</param>
        // [HttpGet]
        // public async Task<IActionResult> DownloadPdf(int id)
        // {
        //     if (id <= 0)
        //         return BadRequest("Invalid FD ID.");

        //     var apiUrl = $"{ApiBaseUrl}/downloadpdf/{id}";

        //     // Allow self-signed certs in dev environments
        //     var handler = new HttpClientHandler
        //     {
        //         ServerCertificateCustomValidationCallback = 
        //             HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        //     };

        //     using var client = new HttpClient(handler);
        //     var response = await client.GetAsync(apiUrl);

        //     if (!response.IsSuccessStatusCode)
        //         return NotFound($"PDF not found for FD ID: {id}");

        //     var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        //     var fileName = $"FD_{id}.pdf";

        //     return File(pdfBytes, "application/pdf", fileName);
        // }
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
