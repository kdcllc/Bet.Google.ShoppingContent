using System;
using System.IO;

using Bet.Google.ShoppingContent.Options;
using Bet.Google.ShoppingContent.Services;

using Google.Apis.Services;
using Google.Apis.ShoppingContent.v2_1;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ShoppingServiceCollectionExtensions
    {
        /// <summary>
        /// Add Google Shopping Content Management API.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="sectionName">The section name in the configuration provider.</param>
        /// <param name="configureOptions">The configuration options.</param>
        /// <returns></returns>
        public static IServiceCollection AddGoogleShoppingContent(
            this IServiceCollection services,
            string sectionName = nameof(GoogleShoppingOptions),
            Action<GoogleShoppingOptions>? configureOptions = null)
        {
            // configure options
            services.AddOptions<GoogleShoppingOptions>()
                    .Configure<IConfiguration>((o, c) =>
                    {
                        c.Bind(sectionName, o);

                        if (!string.IsNullOrEmpty(o.GoogleServiceAccountFile)
                            && File.Exists(o.GoogleServiceAccountFile))
                        {
                            o.GoogleServiceAccount = File.ReadAllBytes(o.GoogleServiceAccountFile);
                        }

                        // override the options values thru configurations
                        configureOptions?.Invoke(o);
                    });

            services.AddLogging();
            services.AddScoped<IGoogleAuthenticationService, GoogleAuthenticationService>();
            services.AddScoped<IGoogleProductService, GoogleProductService>();
            services.AddScoped<IGoogleMerchantConfigService, GoogleMerchantConfigService>();

            services.AddScoped(sp =>
            {
                var authetnicator = sp.GetRequiredService<IGoogleAuthenticationService>();
                var init = new BaseClientService.Initializer()
                {
                    HttpClientInitializer = authetnicator.AuthenticateAsync(ShoppingContentService.Scope.Content).GetAwaiter().GetResult(),
                    ApplicationName = nameof(Bet.Google.ShoppingContent),
                };

                return new ShoppingContentService(init);
            });

            return services;
        }
    }
}
