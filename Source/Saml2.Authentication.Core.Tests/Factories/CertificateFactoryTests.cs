using Microsoft.Extensions.DependencyInjection;
using Saml2.Authentication.Core.Configuration;
using Saml2.Authentication.Core.Factories;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using Xunit;

namespace Saml2.Authentication.Core.Tests.Factories
{
    [ExcludeFromCodeCoverage]
    public class CertificateFactoryTests
    {
        private readonly ServiceProvider serviceProvider;

        public CertificateFactoryTests()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddLogging();

            serviceProvider = services.BuildServiceProvider();
        }

        [Theory]
        [InlineData("stubidp.sustainsys.com.cer",
                    "MIICFTCCAYKgAwIBAgIQzfcJCkM1YahDtRGYsLphrDAJBgUrDgMCHQUAMCExHzAdBgNVBAMTFnN0dWJpZHAuc3VzdGFpbnN5cy5jb20wHhcNMTcxMjE0MTE1NDUwWhcNMzkxMjMxMjM1OTU5WjAhMR8wHQYDVQQDExZzdHViaWRwLnN1c3RhaW5zeXMuY29tMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDSSq8EX46J1yprfaBdh4pWII+/E7ypHM1NjG7mCwFwbkjq2tpSBuoASrQftbjIKqjVzxtxETw802VJu5CJR4d3Zdy5jD8NRTesfaQDazX7iiqisfnxmIdDhtJS0lXeBlj4MipoUW6l8Qsjx7ltZSwdfFLyh+bMqIrwOhMWGs82vQIDAQABo1YwVDBSBgNVHQEESzBJgBCBBNba7KNF5wnXqmYcejn6oSMwITEfMB0GA1UEAxMWc3R1YmlkcC5zdXN0YWluc3lzLmNvbYIQzfcJCkM1YahDtRGYsLphrDAJBgUrDgMCHQUAA4GBAHonBGahlldp7kcN5HGGnvogT8a0nNpM7GMdKhtzpLO3Uk3HyT3AAIKWiSoEv2n1BTalJ/CY/+te/JZPVGhWVzoi5JYytpj5gM0O7RH0a3/yUE8S8YLV2h0a2gsdoMvTRTnTm9CnXezCKqhjYjwsmOZtiCIYuFqX71d/pg5uoJfs")]
        public void GetCertificate_X509KeyInfo(string filename, string expectedX509String)
        {
            CertificateFactory certificateFactory = new CertificateFactory();

            KeyInfo keyInfo = GetX509KeyInfo(filename);

            Certificate certificate = certificateFactory.GetCertificate(keyInfo);

            Assert.NotNull(certificate);
            Assert.NotNull(certificate.X509String);
            Assert.NotEmpty(certificate.X509String);
            Assert.Equal(expectedX509String, certificate.X509String);
        }

        private KeyInfo GetX509KeyInfo(string certFileName)
        {
            KeyInfo keyInfo = new KeyInfo();

            string filename = $"TestsResources/Certificates/{certFileName}";
            var fullFileName = !Path.IsPathRooted(filename)
                ? Path.Combine(Directory.GetCurrentDirectory(), filename)
                : filename;

            using (X509Certificate2 x509Certificate2 = new X509Certificate2(fullFileName))
            {
                keyInfo.AddClause(new KeyInfoX509Data(x509Certificate2));
            }

            return keyInfo;
        }
    }
}
