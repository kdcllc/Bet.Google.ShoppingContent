using System;
using System.IO;
using System.Threading.Tasks;

using Bet.Google.ShoppingContent.Options;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Google.ShoppingContent.Services
{
    public class AuthenticationService
    {
        private readonly AppsOptions _options;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IOptions<AppsOptions> options,
            ILogger<AuthenticationService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IConfigurableHttpClientInitializer> AuthenticateAsync(string scope = "")
        {
            _logger.LogInformation("Loading service account credentials from {path} ", _options.GoogleServiceAccountFile);

            if (_options.GoogleServiceAccount == null)
            {
                throw new ArgumentNullException(nameof(_options.GoogleServiceAccount));
            }

            var scopes = new[] { scope };

            using var stream = new MemoryStream();
            await stream.WriteAsync(_options.GoogleServiceAccount, 0, _options.GoogleServiceAccount.Length);
            stream.Position = 0;
            var credential = GoogleCredential.FromStream(stream);

            return credential.CreateScoped(scopes);
        }
    }
}
