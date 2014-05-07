using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace iTraycerSection.Domain
{
    [DataContract]
    [XmlRoot("root")]
    public class ReceiverEnvelope
    {
        [XmlElement(ElementName = "ReturnKitTrays")]
        [DataMember(Name = "ReturnKitTrays")]
        public string ReturnKitTrays { get; set; }

    }
}
