using dk.nita.saml20;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Saml2.Authentication.Core.Schema.Metadata
{
    [Serializable]
    [XmlType(Namespace = Saml2Constants.METADATA)]
    [XmlRoot(ELEMENT_NAME, Namespace = Saml2Constants.METADATA, IsNullable = false)]
    public class SSODescriptor : RoleDescriptor
    {
        public SSODescriptor()
        {
            SingleLogoutService = new List<EndpointType>();
        }

        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public new const string ELEMENT_NAME = "SSODescriptor";

        [XmlElement("SingleLogoutService", typeof(EndpointType), IsNullable = true)]
        public List<EndpointType> SingleLogoutService { get; set; }

        [XmlElement("ArtifactResolutionService", typeof(IndexedEndpointType), IsNullable = true)]
        public List<IndexedEndpointType> ArtifactResolutionService { get; set; }
    }
}
