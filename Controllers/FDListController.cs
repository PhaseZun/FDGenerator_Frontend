using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MyApp.Web.Models.FD;
using System.Text.Json;

namespace MyApp.Web.Controllers
{
    public class FDListController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FDListController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Display FD list
         public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync("http://localhost:5269/api/FDList/list");
            if (!response.IsSuccessStatusCode)
            {
                return View(new List<FDModel>()); // empty list if fail
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            var fdList = JsonSerializer.Deserialize<List<FDModel>>(jsonString, new JsonSerializerOptions
            { 
                PropertyNameCaseInsensitive = true
            });

            return View(fdList);
        }

        public async Task<IActionResult> DownloadPdf(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("Invalid PDF file name.");

            // Replace with your API URL
            var apiUrl = $"https://localhost:5269/api/pdf/download/{fileName}";

           // var client = _httpClientFactory.CreateClient();
             var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = 
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

            var client = new HttpClient(handler);
            var response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                return NotFound("PDF not found on server.");

            var pdfStream = await response.Content.ReadAsStreamAsync();
            return File(pdfStream, "application/pdf", fileName);
        }
    }
}
