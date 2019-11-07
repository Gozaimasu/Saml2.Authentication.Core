using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Saml2.Authentication.Core.Configuration;
using System.Net;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Saml2.Authentication.Core.Factories;
using System.Threading.Tasks;
using System.Threading;
using Moq;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Saml2.Authentication.Core.Tests.Configuration
{
    [ExcludeFromCodeCoverage]
    public class IdentityProviderConfigurationUpdaterTests
    {
        private readonly ServiceProvider serviceProvider;

        public IdentityProviderConfigurationUpdaterTests()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddLogging();
            services.TryAddTransient<ICertificateFactory, CertificateFactory>();

            serviceProvider = services.BuildServiceProvider();

            WebRequest.RegisterPrefix("mock:", new MockRequestCreator());
            ConfigureMetadata("FederationMetadata.xml");
            ConfigureMetadata("AdfsFederationMetadata.xml");
        }

        [Theory]
        [InlineData("FederationMetadata.xml",
                    "https://stubidp.sustainsys.com/Metadata", 
                    "https://stubidp.sustainsys.com/ArtifactResolve",
                    "https://stubidp.sustainsys.com/",
                    "https://stubidp.sustainsys.com/Logout")]
        [InlineData("AdfsFederationMetadata.xml",
                    "http://testauth.indexdev.france/adfs/services/trust",
                    null,
                    "https://testauth.indexdev.france/adfs/ls/",
                    "https://testauth.indexdev.france/adfs/ls/")]
        public async Task ExecuteAsync_CorrectResults(
            string FederationMetadataFileName,
            string expectedEntityId, 
            string expectedArtifactResolve,
            string expectedSingleSignOn,
            string expectedSingleSignOut)
        {
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            ICertificateFactory certificateFactory = serviceProvider.GetService<ICertificateFactory>();

            Saml2Configuration configuration = GetSaml2Configuration(FederationMetadataFileName);

            IdentityProviderConfigurationUpdater idpConfigUpdater = 
                new IdentityProviderConfigurationUpdater(configuration, certificateFactory, loggerFactory);

            await idpConfigUpdater.StartAsync(CancellationToken.None);
            await Task.Delay(100);//Give some time to invoke the methods under test
            await idpConfigUpdater.StopAsync(CancellationToken.None);

            IdentityProviderConfiguration identityProviderConfiguration = configuration.IdentityProviderConfiguration[0];

            Assert.Equal(expectedEntityId, identityProviderConfiguration.EntityId);
            Assert.Equal(expectedArtifactResolve, identityProviderConfiguration.ArtifactResolveService);
            Assert.NotNull(identityProviderConfiguration.SigningCertificate);
            Assert.NotNull(identityProviderConfiguration.EncryptionCertificate);
            Assert.Equal(expectedSingleSignOn, identityProviderConfiguration.SingleSignOnService);
            Assert.Equal(expectedSingleSignOut, identityProviderConfiguration.SingleSignOutService);
        }

        [Fact]
        public async Task ExecuteAsync_NoFederationMetadata()
        {
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            ICertificateFactory certificateFactory = serviceProvider.GetService<ICertificateFactory>();

            Saml2Configuration configuration = GetSaml2Configuration();

            IdentityProviderConfigurationUpdater idpConfigUpdater =
                new IdentityProviderConfigurationUpdater(configuration, certificateFactory, loggerFactory);

            await idpConfigUpdater.StartAsync(CancellationToken.None);
            await Task.Delay(100);//Give some time to invoke the methods under test
            await idpConfigUpdater.StopAsync(CancellationToken.None);

            IdentityProviderConfiguration identityProviderConfiguration = configuration.IdentityProviderConfiguration[0];
        }

        private Saml2Configuration GetSaml2Configuration(string FederationMetadataFileName)
        {
            Saml2Configuration configuration = new Saml2Configuration
            {
                IdentityProviderConfiguration = new List<IdentityProviderConfiguration>
                {
                    new IdentityProviderConfiguration
                    {
                        Name = "MockIdP",
                        FederationMetadata = $"mock://{FederationMetadataFileName}"
                    }
                }
            };

            return configuration;
        }

        private Saml2Configuration GetSaml2Configuration()
        {
            Saml2Configuration configuration = new Saml2Configuration
            {
                IdentityProviderConfiguration = new List<IdentityProviderConfiguration>
                {
                    new IdentityProviderConfiguration
                    {
                        Name = "MockIdP"
                    }
                }
            };

            return configuration;
        }

        private void ConfigureMetadata(string FederationMetadataFileName)
        {
            Stream responseStream = new MemoryStream();
            using (Stream stream = new FileStream($"TestsResources/{FederationMetadataFileName}", FileMode.Open))
            {
                stream.CopyTo(responseStream);
                responseStream.Position = 0;
            }

            Mock<WebResponse> mockWebResponse = new Mock<WebResponse>();
            mockWebResponse.Setup(wr => wr.GetResponseStream()).Returns(responseStream);
            MockWebRequest.AddData($"mock://{FederationMetadataFileName}", mockWebResponse.Object);
        }
    }
}
