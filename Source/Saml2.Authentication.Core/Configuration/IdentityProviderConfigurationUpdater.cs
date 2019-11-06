using dk.nita.saml20.Schema.Metadata;
using dk.nita.saml20.Schema.XmlDSig;
using Microsoft.Extensions.Hosting;
using Saml2.Authentication.Core.Schema.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Saml2.Authentication.Core.Configuration
{
    internal class IdentityProviderConfigurationUpdater : BackgroundService
    {
        private readonly Saml2Configuration configuration;
        private readonly int delay = 24 * 60 * 60 * 1000; // 24h

        public IdentityProviderConfigurationUpdater(Saml2Configuration configuration)
        {
            this.configuration = configuration;
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
                                KeyInfo keyInfo = keyDescriptor.KeyInfo;
                                for (int i = 0; i < keyInfo.Items.Length; i++)
                                {
                                    if(keyInfo.ItemsElementName[i] == ItemsChoiceType2.X509Data)
                                    {
                                        X509Data x509Data = keyInfo.Items[i] as X509Data;
                                        for (int j = 0; j < x509Data.Items.Length; j++)
                                        {
                                            if(x509Data.ItemsElementName[j] == ItemsChoiceType.X509Certificate)
                                            {
                                                byte[] data = x509Data.Items[j] as byte[];
                                                if (!keyDescriptor.useSpecified || keyDescriptor.use == KeyTypes.signing)
                                                    configuration.Certificate = new Certificate { X509String = Convert.ToBase64String(data) };
                                            }
                                        }
                                    }
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
