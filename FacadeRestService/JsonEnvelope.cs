﻿using System;
using System.Collections;
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
        public ServiceQueue ServiceQueues { get; set; }

        [XmlElement(ElementName = "Commit")]
        [DataMember(Name = "Commit")]
        public string Commit { get; set; }
    }

    [DataContract]
    [XmlRoot("ServiceQueue")]
    public class ServiceQueue
    {
        [XmlElement(ElementName = "ServiceName")]
        [DataMember(Name = "ServiceName")]
        public string ServiceName { get; set; }

        [XmlElement(ElementName = "Parameters")]
        [DataMember(Name = "Parameters")]
        public Parameter Parameters { get; set; }

        [XmlElement(ElementName = "Response")]
        [DataMember(Name = "Response")]
        public ArrayList Response { get; set; }

        [XmlElement(ElementName = "ResponseMessage")]
        [DataMember(Name = "ResponseMessage")]
        public string ResponseMessage { get; set; }

        [XmlElement(ElementName = "EC_Code")]
        [DataMember(Name = "EC_Code")]
        public string ECCode { get; set; }
    }

    [DataContract]
    [XmlRoot("Parameters")]
    public class Parameter
    {
        [XmlElement(ElementName = "Email")]
        [DataMember(Name = "Email")]
        public string Email { get; set; }

        [XmlElement(ElementName = "Password")]
        [DataMember(Name = "Password")]
        public string Password { get; set; }

        [XmlElement(ElementName = "DeviceID")]
        [DataMember(Name = "DeviceID")]
        public string DeviceID { get; set; }

        [XmlElement(ElementName = "DevicePlatform")]
        [DataMember(Name = "DevicePlatform")]
        public string DevicePlatform { get; set; }

        [XmlElement(ElementName = "DeviceOSVersion")]
        [DataMember(Name = "DeviceOSVersion")]
        public string DeviceOSVersion { get; set; }
    }
}