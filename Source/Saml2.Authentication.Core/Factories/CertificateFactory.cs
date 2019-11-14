using Saml2.Authentication.Core.Configuration;
using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace Saml2.Authentication.Core.Factories
{
    internal class CertificateFactory : ICertificateFactory
    {
        public Certificate GetCertificate(KeyInfoClause keyInfoClause)
        {
            Type type = keyInfoClause.GetType();

            if (type == typeof(KeyInfoX509Data))
                return GetCertificateFromX509Data(keyInfoClause as KeyInfoX509Data);

            throw new NotImplementedException();
        }

        public Certificate GetCertificate(KeyInfo keyInfo)
        {
            IEnumerator keyInfoClauses = keyInfo.GetEnumerator();
            while (keyInfoClauses.MoveNext())
            {
                KeyInfoClause keyInfoClause = keyInfoClauses.Current as KeyInfoClause;
                Certificate certificate = GetCertificate(keyInfoClause);
                if (certificate != null)
                    return certificate;
            }

            return null;
        }

        private Certificate GetCertificateFromX509Data(KeyInfoX509Data x509Data)
        {
            if (x509Data == null)
                throw new ArgumentNullException(nameof(x509Data));

            if (x509Data.Certificates == null)
                return null;

            if (x509Data?.Certificates?.Count == 0)
                return null;

            X509Certificate2 certificate = x509Data.Certificates[0] as X509Certificate2;
            byte[] data = certificate.Export(X509ContentType.Cert);
            return new Certificate { X509String = Convert.ToBase64String(data) };
        }
    }
}
