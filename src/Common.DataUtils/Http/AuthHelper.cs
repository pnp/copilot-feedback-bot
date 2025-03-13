using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;

namespace Common.DataUtils.Http
{

    public class AuthHelper
    {
        public const string CertificateName = "O365AdvancedAnalytics";
        private static X509Certificate2? _cachedCert = null;
        public static async Task<IConfidentialClientApplication> GetNewClientApp(
            string tenantId, string clientId, string clientSecret, string keyVaultUrl, bool useClientCertificate, ILogger debugTracer)
        {
            IConfidentialClientApplication? app = null;
            if (useClientCertificate)
            {
                var appRegistrationCert = await RetrieveKeyVaultCertificate(CertificateName, keyVaultUrl, debugTracer);

                app = ConfidentialClientApplicationBuilder.Create(clientId)
                                                      .WithCertificate(appRegistrationCert)
                                                      .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                                                      .Build();

                return app;
            }
            else
            {
                app = ConfidentialClientApplicationBuilder.Create(clientId)
                                                     .WithClientSecret(clientSecret)
                                                     .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                                                     .Build();
            }
            return app;
        }

        public static async Task<X509Certificate2> RetrieveKeyVaultCertificate(string certName, string keyVaultUrl, ILogger debugTracer)
        {
            if (_cachedCert == null)
            {
                debugTracer.LogInformation($"Retrieving certificate {certName} from KeyVault {keyVaultUrl}");
                var client = new CertificateClient(vaultUri: new Uri(keyVaultUrl), credential: new DefaultAzureCredential());

                // Get private key
                var secret = await client.DownloadCertificateAsync(certName);
                if (secret.Value != null)
                {
                    _cachedCert = new X509Certificate2(secret.Value);
                }
                else
                {
                    debugTracer.LogCritical($"Certificate {certName} not found in KeyVault {keyVaultUrl}");
                    throw new Exception($"Auth error: Certificate {certName} not found in KeyVault {keyVaultUrl}");
                }
            }
            return _cachedCert;

        }
    }
}
