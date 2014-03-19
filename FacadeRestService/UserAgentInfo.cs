using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FacadeRestService
{
    public class UserAgentInfo
    {
        public UserAgentInfo(string userAgent)
        {
            UserAgent = userAgent;
        }

        [DataMember]
        public string UserAgent { get; set; }

    }
}