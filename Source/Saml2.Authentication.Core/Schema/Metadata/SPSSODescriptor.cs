using dk.nita.saml20;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Saml2.Authentication.Core.Schema.Metadata
{
    [Serializable]
    [XmlType(Namespace = Saml2Constants.METADATA)]
    [XmlRoot(ELEMENT_NAME, Namespace = Saml2Constants.METADATA, IsNullable = false)]
    public class SPSSODescriptor : SSODescriptor
    {
        public SPSSODescriptor()
        {
            AssertionConsumerServices = new List<IndexedEndpointType>();
        }

        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public new const string ELEMENT_NAME = "SPSSODescriptor";

        [XmlElement("AssertionConsumerService", typeof(IndexedEndpointType), IsNullable = true)]
        public List<IndexedEndpointType> AssertionConsumerServices { get; set; }
    }
}
