using Microsoft.AspNetCore.Builder;
using Saml2.Authentication.Core.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Saml2ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSaml(this IApplicationBuilder builder)
        {
            return builder.UseFederationMetadataMiddleware();
        }
    }
}
