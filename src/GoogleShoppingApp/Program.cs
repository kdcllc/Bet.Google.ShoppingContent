using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoogleShoppingApp
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
                        services.AddScoped<MerchantDemo>();
                        services.AddScoped<ProductDemo>();
                    })
                    .UseConsoleLifetime()
                    .Build();

            await host.StartAsync();

            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("[App][Started]");

            // 1. Merchant Account Demos
            var merchantDemo = scope.ServiceProvider.GetRequiredService<MerchantDemo>();

            // 1.1 retrieves and saves merchant account info
            await merchantDemo.GetMerchantAccountInfoAsync(saveToFile: false);

            // 1.2 retrieves and saves merchant shipping info
            await merchantDemo.GetMerchantAccountShippingAsync(saveToFile: false);

            // 2. Product Demos
            var productDemo = scope.ServiceProvider.GetRequiredService<ProductDemo>();

            // 2.1 Get timed list
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(1));
            var products = await productDemo.GetProductsAsync(false, cts.Token);
            logger.LogInformation("Total products count {count}", products.Count);

            // 2.2 Get Product Status
            var productStatus = await productDemo.GetProductStatusesAsync(10, products, false);
            logger.LogInformation("Total products statuses count {count}", productStatus.Count);

            // 2.3 Get Product Statuses
            var produtStatusesList = await productDemo.GetProductsWithErrorsAsync();
            logger.LogInformation("Total products with error statuses count {count}", produtStatusesList.Count);

            // logger.LogError(string.Join(",", item.ItemLevelIssues.Select(x => x.Detail)));
            await Task.Delay(TimeSpan.FromSeconds(5));

            logger.LogInformation("[App][Stopped]");

            await host.StopAsync();
            return 0;
        }
    }
}
