using dk.nita.saml20;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Saml2.Authentication.Core.Schema.Metadata
{
    [Serializable]
    [XmlType(Namespace = Saml2Constants.METADATA)]
    public class IndexedEndpointType : EndpointType
    {
        [XmlAttribute(AttributeName = "index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "isDefault")]
        public bool IsDefault { get; set; }
    }
}
