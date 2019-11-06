using dk.nita.saml20;
using dk.nita.saml20.Schema.Metadata;
using Microsoft.Extensions.Hosting;
using Saml2.Authentication.Core.Factories;
using Saml2.Authentication.Core.Schema.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Saml2.Authentication.Core.Configuration
{
    internal class IdentityProviderConfigurationUpdater : BackgroundService
    {
        private readonly Saml2Configuration configuration;
        private readonly ICertificateFactory certificateFactory;
        private readonly int delay = 24 * 60 * 60 * 1000; // 24h

        public IdentityProviderConfigurationUpdater(Saml2Configuration configuration, ICertificateFactory certificateFactory)
        {
            this.configuration = configuration;
            this.certificateFactory = certificateFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IList<IdentityProviderConfiguration> configurations = configuration.IdentityProviderConfiguration;
                foreach (IdentityProviderConfiguration configuration in configurations)
                {
                    if (string.IsNullOrEmpty(configuration.FederationMetadata))
                        continue;

                    await UpdateConfigurationAsync(configuration);

                    // Télécharger url
                    // mettre à jour configuration
                }
                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task UpdateConfigurationAsync(IdentityProviderConfiguration configuration)
        {
            WebRequest webRequest = WebRequest.Create(configuration.FederationMetadata);

            try
            {
                using (WebResponse webResponse = await webRequest.GetResponseAsync())
                {
                    using (Stream stream = webResponse.GetResponseStream())
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(EntityDescriptor));
                        EntityDescriptor entityDescriptor = (EntityDescriptor)serializer.Deserialize(stream);

                        configuration.EntityId = entityDescriptor.EntityID;
                        foreach (var roles in entityDescriptor.IDPSSORoles)
                        {
                            IDPSSODescriptor descriptor = roles as IDPSSODescriptor;

                            // On ne gère que le protocole SAML2
                            if (descriptor.ProtocolSupportEnumeration != Saml2Constants.PROTOCOL)
                                continue;

                            foreach (EndpointType singleSignOnService in descriptor.SingleSignOnService)
                            {
                                configuration.SingleSignOnService = singleSignOnService.Location;
                                configuration.ProtocolBinding = singleSignOnService.Binding;
                            }

                            foreach (EndpointType singleLogoutService in descriptor.SingleLogoutService)
                            {
                                configuration.SingleSignOutService = singleLogoutService.Location;
                            }

                            foreach (IndexedEndpointType artifactResolutionService in descriptor.ArtifactResolutionService)
                            {
                                if (artifactResolutionService.IsDefault)
                                    configuration.SingleSignOutService = artifactResolutionService.Location;
                                else if(string.IsNullOrEmpty(configuration.SingleSignOutService))
                                    configuration.SingleSignOutService = artifactResolutionService.Location;
                            }

                            foreach (KeyDescriptor keyDescriptor in descriptor.KeyDescriptors)
                            {
                                Certificate certificate = certificateFactory.GetCertificate((KeyInfo)keyDescriptor.KeyInfo);
                                if (certificate != null)
                                {
                                    if (!keyDescriptor.UseSpecified || keyDescriptor.Use == KeyTypes.signing)
                                        configuration.SigningCertificate = certificate;
                                    if (!keyDescriptor.UseSpecified || keyDescriptor.Use == KeyTypes.encryption)
                                        configuration.EncryptionCertificate = certificate;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
