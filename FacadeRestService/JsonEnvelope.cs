using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FacadeRestService
{
    [DataContract]
    public class JsonEnvelope
    {
        [DataMember(Name = "CoID")]
        public string CoID { get; set; }
        [DataMember(Name = "RepID")]
        public string RepID { get; set; }
        [DataMember(Name = "AppID")]
        public string AppID { get; set; }
        [DataMember(Name = "SyncRequestTime")]
        public string SyncRequestTime { get; set; }
        [DataMember(Name = "SyncResponseTime")]
        public string SyncResponseTime { get; set; }
        [DataMember(Name = "LocationVector")]
        public string LocationVector { get; set; }
        [DataMember(Name = "LocationVector")]
        public string email { get; set; }
        [DataMember(Name = "LocationVector")]
        public string passcode { get; set; }
        [DataMember(Name = "LocationVector")]
        public string token { get; set; }
    }
}