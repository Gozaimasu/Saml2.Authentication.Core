using System;
using System.Xml.Serialization;
using dk.nita.saml20.Schema.XEnc;
using dk.nita.saml20.Schema.XmlDSig;

namespace dk.nita.saml20.Schema.Metadata
{
    /// <summary>
    /// The &lt;KeyDescriptor&gt; element provides information about the cryptographic key(s) that an entity uses
    /// to sign data or receive encrypted keys, along with additional cryptographic details.
    /// </summary>
    [Serializable]
    [XmlType(Namespace = Saml2Constants.METADATA)]
    [XmlRoot(ELEMENT_NAME, Namespace = Saml2Constants.METADATA, IsNullable = false)]
    public class KeyDescriptor
    {
        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public const string ELEMENT_NAME = "KeyDescriptor";

        /// <summary>
        /// The XML Signature element KeyInfo. Can be implicitly converted to the .NET class System.Security.Cryptography.Xml.KeyInfo.
        /// </summary>
        [XmlElement(Namespace = Saml2Constants.XMLDSIG)]
        public KeyInfo KeyInfo { get; set; }


        /// <summary>
        /// Gets or sets the encryption method.
        /// Optional element specifying an algorithm and algorithm-specific settings supported by the entity.
        /// The exact content varies based on the algorithm supported. See [XMLEnc] for the definition of this
        /// element's xenc:EncryptionMethodType complex type.
        /// </summary>
        /// <value>The encryption method.</value>
        [XmlElement("EncryptionMethod")]
        public EncryptionMethod[] EncryptionMethod { get; set; }


        /// <summary>
        /// Gets or sets the use.
        /// Optional attribute specifying the purpose of the key being described. Values are drawn from the
        /// KeyTypes enumeration, and consist of the values encryption and signing.
        /// </summary>
        /// <value>The use.</value>
        [XmlAttribute(AttributeName = "use")]
        public KeyTypes Use { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether [use specified].
        /// </summary>
        /// <value><c>true</c> if [use specified]; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        public bool UseSpecified { get; set; }
    }
}