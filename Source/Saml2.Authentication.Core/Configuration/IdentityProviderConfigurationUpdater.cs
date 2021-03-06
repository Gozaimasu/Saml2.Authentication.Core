﻿using dk.nita.saml20;
using dk.nita.saml20.Schema.Metadata;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Saml2.Authentication.Core.Factories;
using Saml2.Authentication.Core.Schema.Metadata;
using System;
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
        private readonly ILogger logger;
        private readonly int delay = 24 * 60 * 60 * 1000; // 24h

        public IdentityProviderConfigurationUpdater(
            Saml2Configuration configuration, 
            ICertificateFactory certificateFactory,
            ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.certificateFactory = certificateFactory;
            logger = loggerFactory.CreateLogger(typeof(IdentityProviderConfigurationUpdater));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogDebug($"Entering {nameof(ExecuteAsync)}", stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Performing periodic update");
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
            logger.LogInformation($"Updating {configuration.FederationMetadata}");
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
                                    configuration.ArtifactResolveService = artifactResolutionService.Location;
                                else if(string.IsNullOrEmpty(configuration.ArtifactResolveService))
                                    configuration.ArtifactResolveService = artifactResolutionService.Location;
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
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error while processing {configuration.Name} {configuration.FederationMetadata}");
            }
        }
    }
}
