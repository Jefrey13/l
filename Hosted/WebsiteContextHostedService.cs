using PuppeteerSharp;
using System.Text.Json;

namespace CustomerService.API.Hosted
{
    public class WebsiteContextHostedService : BackgroundService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<WebsiteContextHostedService> _logger;

        // Las URLs para scrapear
        private static readonly string[] Urls = new[]
        {
        "https://www.pcgroupsa.com",
        "https://www.pcgroupsa.com/inicio",
        "https://www.pcgroupsa.com/servicios",
        "https://www.pcgroupsa.com/nosotros",
        "https://www.pcgroupsa.com/contactanos"
    };

        public WebsiteContextHostedService(
            IWebHostEnvironment env,
            ILogger<WebsiteContextHostedService> logger)
        {
            _env = env;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Bajar el Chromium una sola vez
            await new BrowserFetcher().DownloadAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Hacer el scraping
                    var allText = new List<string>();
                    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                    foreach (var url in Urls)
                    {
                        try
                        {
                            await using var page = await browser.NewPageAsync();
                            await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);
                            var content = await page.EvaluateExpressionAsync<string>("document.body.innerText");
                            //allText.Add(content.Trim());
                            var hrefs = await page.EvaluateFunctionAsync<string[]>(
                                @"() => Array.from(document.querySelectorAll('a'))
                                            .map(a => a.href)
                                            .filter(href => href && href.trim().length > 0)"
                            );

                            var textContent = await page.EvaluateExpressionAsync<string>("document.body.innerText");

                            allText.Add("URLS:\n" + string.Join("\n", hrefs));
                            allText.Add("TEXT:\n" + textContent.Trim());
                        }
                        catch (Exception exUrl)
                        {
                            _logger.LogWarning(exUrl, "Error scrappeando {Url}", url);
                        }
                    }

                    //Serializar y guardar JSON
                    var contextObj = new WebsiteContextDto
                    {
                        Content = string.Join("\n\n", allText),
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    var json = JsonSerializer.Serialize(contextObj, new JsonSerializerOptions { WriteIndented = true });

                    var folder = Path.Combine(_env.ContentRootPath, "WhContext");
                    Directory.CreateDirectory(folder);
                    var fullPath = Path.Combine(folder, "websiteContext.json");
                    await File.WriteAllTextAsync(fullPath, json, stoppingToken);

                    _logger.LogInformation("🌐 websiteContext.json actualizado a {Time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en WebsiteContextHostedService");
                }

                // Esperar 24 h antes de la próxima ejecución
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }

    public class WebsiteContextDto
    {
        public string Content { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}