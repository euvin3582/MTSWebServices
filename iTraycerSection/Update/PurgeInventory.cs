using DataLayer.domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTraycerSection.Update
{
    public static class PurgeInventory
    {
        public static bool PurgeInventroy(string trayId, string lotNumber, string partNumber, string qntyUsed, string type)
        {
            UserInfo userInfo = iTraycerSection.Session.Session.userInfo;

            return DataLayer.Controller.PurgeInventory(trayId, lotNumber, partNumber, userInfo.CustomerId.ToString(), userInfo.Id.ToString(), 
                userInfo.Id.ToString(), "", qntyUsed.ToString(), type);
        }
    }
}
