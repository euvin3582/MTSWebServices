using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace FacadeRestService
{
    [DataContract]
    [XmlRoot("root")]
    public class JsonEnvelope
    {
        [XmlElement(ElementName = "CoID")]
        [DataMember(Name = "CoID")]
        public string CoID { get; set; }

        [XmlElement(ElementName = "RepID")]
        [DataMember(Name = "RepID")]
        public string RepID { get; set; }

        [XmlElement(ElementName = "AppID")] 
        [DataMember(Name = "AppID")]
        public string AppID { get; set; }

        [XmlElement(ElementName = "SyncRequestTime")]
        [DataMember(Name = "SyncRequestTime")]
        public string SyncRequestTime { get; set; }

        [XmlElement(ElementName = "SyncResponseTime")]
        [DataMember(Name = "SyncResponseTime")]
        public string SyncResponseTime { get; set; }

        [XmlElement(ElementName = "LocationVector")]
        [DataMember(Name = "LocationVector")]
        public string LocationVector { get; set; }

        [XmlElement(ElementName = "MtsToken")]
        [DataMember(Name = "MtsToken")]
        public string MtsToken { get; set; }

        [XmlElement(ElementName = "AppLaunchCount")]
        [DataMember(Name = "AppLaunchCount")]
        public string AppLaunchCount { get; set; }

        [XmlElement(ElementName = "SQL")]
        [DataMember(Name = "SQL")]
        public string SQL { get; set; }

        [XmlElement(ElementName = "ServiceQueue")]
        [DataMember(Name = "ServiceQueue")]
        public object[] ServiceQueues { get; set; }

        [XmlElement(ElementName = "Command")]
        [DataMember(Name = "Command")]
        public ArrayList Command { get; set; }

        [XmlElement(ElementName = "Commit")]
        [DataMember(Name = "Commit")]
        public string Commit { get; set; }
    }
}