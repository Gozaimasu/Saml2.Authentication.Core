using dk.nita.saml20;
using System;
using System.Xml.Serialization;

namespace Saml2.Authentication.Core.Schema.Metadata
{
    [Serializable]
    [XmlType(Namespace = Saml2Constants.METADATA)]
    public class EndpointType
    {
        [XmlAttribute]
        public string Binding { get; set; }

        [XmlAttribute]
        public string Location { get; set; }
    }
}
