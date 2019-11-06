using dk.nita.saml20;
using dk.nita.saml20.Schema.Metadata;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Saml2.Authentication.Core.Schema.Metadata
{
    [XmlInclude(typeof(SSODescriptor))]
    [XmlInclude(typeof(IDPSSODescriptor))]
    //[XmlInclude(typeof(SPSSODescriptor))]
    [Serializable]
    [XmlType(Namespace = Saml2Constants.METADATA)]
    [XmlRoot(ELEMENT_NAME, Namespace = Saml2Constants.METADATA, IsNullable = false)]
    public class RoleDescriptor
    {
        public RoleDescriptor()
        {
            KeyDescriptors = new List<KeyDescriptor>();
        }

        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public const string ELEMENT_NAME = "RoleDescriptor";

        [XmlAttribute]
        public string ID { get; set; }

        [XmlAttribute(AttributeName = "protocolSupportEnumeration")]
        public string ProtocolSupportEnumeration { get; set; }

        [XmlElement(ElementName = "KeyDescriptor", Type = typeof(KeyDescriptor), IsNullable = true)]
        public List<KeyDescriptor> KeyDescriptors { get; set; }
    }
}
