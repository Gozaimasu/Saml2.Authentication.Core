using dk.nita.saml20;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Saml2.Authentication.Core.Schema.Metadata
{
    [Serializable]
    [XmlType(Namespace = Saml2Constants.METADATA)]
    [XmlRoot(ELEMENT_NAME, Namespace = Saml2Constants.METADATA, IsNullable = false)]
    public class EntityDescriptor
    {
        public EntityDescriptor()
        {
            SPSSORoles = new List<object>();
            IDPSSORoles = new List<object>();
        }

        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public const string ELEMENT_NAME = "EntityDescriptor";

        [XmlAttribute(Namespace = Saml2Constants.METADATA)]
        public string ID { get; set; }

        [XmlAttribute(AttributeName = "entityID")]
        public string EntityID { get; set; }

        [XmlElement(ElementName = "SPSSODescriptor", Type = typeof(SPSSODescriptor), IsNullable = true)]
        public List<object> SPSSORoles { get; set; }

        [XmlElement(ElementName = "IDPSSODescriptor", Type = typeof(IDPSSODescriptor), IsNullable = true)]
        public List<object> IDPSSORoles { get; set; }
    }
}
