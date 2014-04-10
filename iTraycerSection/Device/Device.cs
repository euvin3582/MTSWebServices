using DataLayer.domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTraycerSection.Device
{
    public class Device
    {
        public static bool AddDeviceInfo(iTraycerDeviceInfo itd)
        {
            return DataLayer.Controller.InsertiTraycerDeviceInfo(itd) == 1;
        }

        public static bool AddApplicationInfo(iTraycerApplication ita)
        {
            return DataLayer.Controller.InsertiTraycerApplicationInfo(ita) == 1;
        }

        public static bool CheckIfExist(String deviceId)
        {
            return DataLayer.Controller.GetiTraycerApplicationDeviceInfoDevId(deviceId).Rows.Count == 1;
        }

        
        //we can do other things here like remove device, update device, etc
    }
}
