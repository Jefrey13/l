using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace CustomerService.API.Hosted
{
    public class WebsiteContextHostedService : BackgroundService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<WebsiteContextHostedService> _logger;
        //private static readonly string[] Urls = new[]
        //{
        //    "https://en.wikipedia.org/wiki/Classroom_of_the_Elite"
        //};

        private static readonly string[] Urls = new[]
            {
                "https://www.pcgroupsa.com",
                "https://www.pcgroupsa.com/inicio",
                "https://www.pcgroupsa.com/servicios",
                "https://www.pcgroupsa.com/nosotros",
                "https://www.pcgroupsa.com/contactanos"
            };

        public WebsiteContextHostedService(IWebHostEnvironment env, ILogger<WebsiteContextHostedService> logger)
        {
            _env = env;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await new BrowserFetcher().DownloadAsync();
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    try
            //    {
            //        var allText = new List<string>();
            //        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            //        foreach (var url in Urls)
            //        {
            //            try
            //            {
            //                await using var page = await browser.NewPageAsync();
            //                await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);
            //                await page.WaitForSelectorAsync("body", new WaitForSelectorOptions { Timeout = 30000 });
            //                await Task.Delay(6000, stoppingToken);

            //                //var hrefs = await page.EvaluateFunctionAsync<string[]>(
            //                //    "() => Array.from(document.querySelectorAll('a')).map(a => a.href).filter(h => h.trim().length > 0)"
            //                //);

            //                var textContent = await page.EvaluateFunctionAsync<string>(
            //                    "() => document.body.textContent.trim()"
            //                );

            //                //allText.Add("URLS:\n" + string.Join("\n", hrefs));
            //                allText.Add("TEXT:\n" + textContent);
            //            }
            //            catch (Exception exUrl)
            //            {
            //                _logger.LogWarning(exUrl, "Error scraping {Url}", url);
            //            }
            //        }

            //        var contextObj = new WebsiteContextDto
            //        {
            //            Content = string.Join("\n\n", allText),
            //            UpdatedAtUtc = DateTime.UtcNow
            //        };

            //        var json = JsonSerializer.Serialize(contextObj, new JsonSerializerOptions { WriteIndented = true });
            //        var folder = Path.Combine(_env.ContentRootPath, "WhContext");
            //        Directory.CreateDirectory(folder);
            //        var fullPath = Path.Combine(folder, "websiteContext.json");
            //        await File.WriteAllTextAsync(fullPath, json, stoppingToken);

            //        _logger.LogInformation("🌐 websiteContext.json updated at {Time}", DateTime.UtcNow);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error in WebsiteContextHostedService");
            //    }

            //    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            //}
        }
    }

    public class WebsiteContextDto
    {
        public string Content { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}