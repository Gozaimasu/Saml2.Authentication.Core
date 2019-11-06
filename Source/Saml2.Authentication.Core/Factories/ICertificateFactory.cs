using Saml2.Authentication.Core.Configuration;
using System.Security.Cryptography.Xml;

namespace Saml2.Authentication.Core.Factories
{
    public interface ICertificateFactory
    {
        Certificate GetCertificate(KeyInfoClause keyInfoClause);
        Certificate GetCertificate(KeyInfo keyInfo);
    }
}
