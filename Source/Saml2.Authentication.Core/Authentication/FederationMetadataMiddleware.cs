using dk.nita.saml20;
using dk.nita.saml20.Schema.Metadata;
using dk.nita.saml20.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Saml2.Authentication.Core.Configuration;
using Saml2.Authentication.Core.Extensions;
using Saml2.Authentication.Core.Providers;
using Saml2.Authentication.Core.Schema.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

namespace Saml2.Authentication.Core.Authentication
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    internal class FederationMetadataMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServiceProviderConfiguration serviceProviderConfiguration;
        private readonly IConfigurationProvider configurationProvider;

        public FederationMetadataMiddleware(RequestDelegate next, IConfigurationProvider configurationProvider)
        {
            _next = next;
            this.configurationProvider = configurationProvider;
            serviceProviderConfiguration = configurationProvider.ServiceProviderConfiguration;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Préparation de l'entityDescriptor
            EntityDescriptor entityDescriptor = new EntityDescriptor()
            {
                ID = serviceProviderConfiguration.Id,
                EntityID = serviceProviderConfiguration.EntityId
            };

            // Ajout du rôle de SPSSO
            entityDescriptor.SPSSORoles.Add(GetSPSSORoleDescriptor(httpContext));

            // Sérialization
            string reponse = Serialization.SerializeToXmlString(entityDescriptor);

            httpContext.Response.ContentType = "application/xml";
            await httpContext.Response.WriteAsync(reponse);
        }

        private SPSSODescriptor GetSPSSORoleDescriptor(HttpContext httpContext)
        {
            // Préparation du rôle de SPSSO
            SPSSODescriptor role = new SPSSODescriptor()
            {
                ProtocolSupportEnumeration = Saml2Constants.PROTOCOL,
            };

            // Ajout de l'information de signature
            role.KeyDescriptors.Add(GetSigningKeyDescriptor());

            // Ajout de l'assertion consumer service
            int index = 0;
            role.AssertionConsumerServices.Add(GetAssertionConsumerService(httpContext, Saml2Constants.ProtocolBindings.HTTP_Post, ref index));
            role.AssertionConsumerServices.Add(GetAssertionConsumerService(httpContext, Saml2Constants.ProtocolBindings.HTTP_Redirect, ref index));

            return role;
        }

        private IndexedEndpointType GetAssertionConsumerService(HttpContext httpContext, string protocolBinding, ref int index)
        {
            return new IndexedEndpointType
            {
                Binding = protocolBinding,
                Location = httpContext.Request.GetBaseUrl() + '/' + serviceProviderConfiguration.AssertionConsumerServiceUrl,
                IsDefault = (index == 0),
                Index = index++
            };
        }

        private KeyDescriptor GetSigningKeyDescriptor()
        {
            // Récupération du certificat
            X509Certificate2 x509Certificate2 = configurationProvider.ServiceProviderSigningCertificate();

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(x509Certificate2));

            // Préparation de la signature
            return new KeyDescriptor
            {
                Use = KeyTypes.signing,
                UseSpecified = true,
                KeyInfo = (dk.nita.saml20.Schema.XmlDSig.KeyInfo)keyInfo
            };
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    internal static class FederationMetadataMiddlewareExtensions
    {
        public static IApplicationBuilder UseFederationMetadataMiddleware(this IApplicationBuilder builder)
        {
            return builder.MapWhen(
                context => context.Request.Path.ToString().EndsWith("FederationMetadata/2007-06/FederationMetadata.xml"),
                appBranch => { appBranch.UseMiddleware<FederationMetadataMiddleware>(); });
        }
    }
}
