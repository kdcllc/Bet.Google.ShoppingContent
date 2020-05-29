using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Bet.Google.ShoppingContent.Services;

using Google.Apis.ShoppingContent.v2_1.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bet.Google.ShoppingContent
{
    internal sealed class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var host = Host
                    .CreateDefaultBuilder(args)
                    .UseEnvironment("Development")
                    .ConfigureLogging((hostingContext, loggerBuilder) =>
                    {
                        loggerBuilder.AddConfiguration(hostingContext.Configuration);
                        loggerBuilder.AddConsole();
                        loggerBuilder.AddDebug();
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddGoogleShoppingContent();
                    })
                    .UseConsoleLifetime()
                    .Build();

            await host.StartAsync();

            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("[App][Started]");

            var appLifeTime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();

            var merchantAccount = scope.ServiceProvider.GetRequiredService<IMerchantConfigService>();
            var shipping = await merchantAccount.GetShippingSettingsAsync(appLifeTime.ApplicationStopping);

            // File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "shipping.json"), JsonSerializer.Serialize(shipping));
            var configMerch = await merchantAccount.GetAsync(appLifeTime.ApplicationStopping);

            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            var products = await productService.GetAllAsync(appLifeTime.ApplicationStopping);

            // File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "products.json"), JsonSerializer.Serialize(products));
            logger.LogInformation("Total products count {count}", products.Count);

            var prodStatus = await productService.GetAllStatusAsync(appLifeTime.ApplicationStopping);
            var invalid = prodStatus.Where(x => x.ItemLevelIssues != null);

            var i = 0;
            var dic = products.Take(10).ToDictionary(_ => (long)++i, p => p.Id);

            var test = await productService.GetStatusAsync(dic, appLifeTime.ApplicationStopping);

            foreach (var st in invalid)
            {
                PrintStatus(st, logger);
            }

            await Task.Delay(TimeSpan.FromSeconds(10));

            logger.LogInformation("[App][Stopped]");

            await host.StopAsync();
            return 0;
        }

        private static void PrintStatus(ProductStatus status, ILogger<Program> logger)
        {
            logger.LogInformation("Information for product {0}:", status.ProductId);
            logger.LogInformation("- Title: {0}", status.Title);

            logger.LogInformation("- Destination statuses:");
            foreach (var stat in status.DestinationStatuses)
            {
                logger.LogInformation("  - {0}: {1}", stat.Destination, stat.Status);
            }

            if (status.ItemLevelIssues == null)
            {
                logger.LogInformation("- No issues.");
            }
            else
            {
                var issues = status.ItemLevelIssues;
                logger.LogInformation("- There are {0} issues:", issues.Count);
                foreach (var issue in issues)
                {
                    logger.LogInformation("  - Code: {0}", issue.Code);
                    logger.LogInformation("    Description: {0}", issue.Description);
                    logger.LogInformation("    Detailed description: {0}", issue.Detail);
                    logger.LogInformation("    Resolution: {0}", issue.Resolution);
                    logger.LogInformation("    Servability: {0}", issue.Servability);
                }
            }
        }
    }
}
