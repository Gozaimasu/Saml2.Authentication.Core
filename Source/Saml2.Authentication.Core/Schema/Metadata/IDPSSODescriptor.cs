using dk.nita.saml20;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Saml2.Authentication.Core.Schema.Metadata
{
    [Serializable]
    [XmlType(Namespace = Saml2Constants.METADATA)]
    [XmlRoot(ELEMENT_NAME, Namespace = Saml2Constants.METADATA, IsNullable = false)]
    public class IDPSSODescriptor : SSODescriptor
    {
        public IDPSSODescriptor()
        {
            SingleSignOnService = new List<EndpointType>();
        }

        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public new const string ELEMENT_NAME = "IDPSSODescriptor";

        [XmlElement("SingleSignOnService", typeof(EndpointType), IsNullable = true)]
        public List<EndpointType> SingleSignOnService { get; set; }
    }
}
