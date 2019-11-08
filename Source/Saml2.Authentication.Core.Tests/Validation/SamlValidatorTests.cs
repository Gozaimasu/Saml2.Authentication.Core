using dk.nita.saml20;
using dk.nita.saml20.Schema.Core;
using Moq;
using Saml2.Authentication.Core.Configuration;
using Saml2.Authentication.Core.Providers;
using Saml2.Authentication.Core.Validation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Xml;
using Xunit;

namespace Saml2.Authentication.Core.Tests.Validation
{
    [ExcludeFromCodeCoverage]
    public class SamlValidatorTests
    {
        [Fact]
        public void GetValidatedAssertion_NullParameters()
        {
            SamlValidator samlValidator = new SamlValidator(null, null);

            ArgumentNullException ex = null;
            ex = Assert.Throws<ArgumentNullException>(() => samlValidator.GetValidatedAssertion(null, "identityProviderName"));
            Assert.Equal("element", ex.ParamName);

            XmlDocument doc = new XmlDocument();
            ex = Assert.Throws<ArgumentNullException>(() => samlValidator.GetValidatedAssertion(doc.CreateElement("xmlElement"), null));
            Assert.Equal("identityProviderName", ex.ParamName);
            
            ex = Assert.Throws<ArgumentNullException>(() => samlValidator.GetValidatedAssertion(doc.CreateElement("xmlElement"), ""));
            Assert.Equal("identityProviderName", ex.ParamName);
        }

        [Fact]
        public void GetValidatedAssertion_NoCertificates()
        {
            Mock<IConfigurationProvider> configurationProviderMock = new Mock<IConfigurationProvider>();
            configurationProviderMock
                .Setup(cp => cp.GetIdentityProviderSigningCertificate(It.IsAny<string>()))
                .Returns((string idp) => null);
            configurationProviderMock
                .Setup(cp => cp.ServiceProviderEncryptionCertificate())
                .Returns(() => null);
            configurationProviderMock
                .Setup(cp => cp.ServiceProviderConfiguration)
                .Returns(() => new ServiceProviderConfiguration { EntityId = "https://Saml2.Authentication.Core.Tests", OmitAssertionSignatureCheck = true });

            Mock<ISamlXmlProvider> samlXmlProviderMock = new Mock<ISamlXmlProvider>();
            samlXmlProviderMock
                .Setup(sxp => sxp.GetAssertion(It.IsAny<XmlElement>(), It.IsAny<AsymmetricAlgorithm>()))
                .Returns((XmlElement element, AsymmetricAlgorithm key) => element);

            SamlValidator samlValidator = new SamlValidator(samlXmlProviderMock.Object, configurationProviderMock.Object);

            XmlDocument doc = new XmlDocument
            {
                PreserveWhitespace = true
            };
            doc.Load("TestsResources/Assertions/Assertion.xml");
            var notOnAfterElement = (XmlElement)doc.GetElementsByTagName(SubjectConfirmationData.ELEMENT_NAME, Saml2Constants.ASSERTION)[0];
            notOnAfterElement.SetAttribute("NotOnOrAfter", DateTime.Now.AddMinutes(1).ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ"));
            samlValidator.GetValidatedAssertion((XmlElement)doc.FirstChild, "UnknownIdP");
        }
    }
}
