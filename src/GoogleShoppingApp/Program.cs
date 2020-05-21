using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Bet.Google.ShoppingContent.Options;
using Bet.Google.ShoppingContent.Services;

using Google.Apis.Services;
using Google.Apis.ShoppingContent.v2_1;

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
                        // configure options
                        services.AddOptions<AppsOptions>().Configure<IConfiguration>((o, c) =>
                        {
                            c.Bind("AppsOptions", o);

                            if (!string.IsNullOrEmpty(o.GoogleServiceAccountFile)
                                && File.Exists(o.GoogleServiceAccountFile))
                            {
                                o.GoogleServiceAccount = File.ReadAllBytes(o.GoogleServiceAccountFile);
                            }
                        });

                        services.AddScoped<AuthenticationService>();

                        services.AddScoped<ShoppingContentService>(sp =>
                        {
                            var authetnicator = sp.GetRequiredService<AuthenticationService>();
                            var init = new BaseClientService.Initializer()
                            {
                                HttpClientInitializer = authetnicator.AuthenticateAsync(ShoppingContentService.Scope.Content).GetAwaiter().GetResult(),
                                ApplicationName = nameof(ShoppingContent),
                            };

                            return new ShoppingContentService(init);
                        });

                        services.AddScoped<ProductService>();
                        services.AddScoped<MerchantConfigService>();
                    })
                    .UseConsoleLifetime()
                    .Build();

            await host.StartAsync();

            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("[App][Started]");
            var appLifeTime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();

            var merchantAccount = scope.ServiceProvider.GetRequiredService<MerchantConfigService>();

            var shipping = await merchantAccount.GetShippingSettingsAsync(appLifeTime.ApplicationStopping);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "shipping.json"), JsonSerializer.Serialize(shipping));

            var configMerch = await merchantAccount.GetAsync(appLifeTime.ApplicationStopping);

            var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

            var products = await productService.GetProducts(appLifeTime.ApplicationStopping);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "products.json"), JsonSerializer.Serialize(products));

            var groupedBy = products.GroupBy(g => new { g.ShippingLabel }).Select(g => g.First()).ToList();
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "groupedBy.json"), JsonSerializer.Serialize(groupedBy));

            logger.LogInformation("Total products count {count}", products.Count);

            await Task.Delay(TimeSpan.FromSeconds(10));

            logger.LogInformation("[App][Stopped]");

            await host.StopAsync();
            return 0;
        }
    }
}
