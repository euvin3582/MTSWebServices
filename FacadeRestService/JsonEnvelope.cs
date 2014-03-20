using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
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

        [XmlElement(ElementName = "INSERT")]
        [DataMember(Name = "INSERT")]
        public string Insert { get; set; }

        [XmlElement(ElementName = "UPDATE")]
        [DataMember(Name = "UPDATE")]
        public string Update { get; set; }

        [XmlElement(ElementName = "DELETE")]
        [DataMember(Name = "DELETE")]
        public string Delete { get; set; }

        [XmlElement(ElementName = "CREATE")]
        [DataMember(Name = "CREATE")]
        public string Create { get; set; }

        [XmlElement(ElementName = "ServiceQueue")]
        [DataMember(Name = "ServiceQueue")]
        public string ServiceQueue { get; set; }

        [XmlElement(ElementName = "COMMIT")]
        [DataMember(Name = "COMMIT")]
        public string Commit { get; set; }
    }

    [DataContract]
    [XmlRoot("ServiceQueue")]
    public class ServiceQueue
    {
        [XmlElement(ElementName = "ServiceQueue")]
        [DataMember(Name = "COMMIT")]
        public string Commit { get; set; }

    }
}