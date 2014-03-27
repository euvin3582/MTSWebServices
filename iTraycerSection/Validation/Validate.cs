using System.Configuration;
using System;
using System.Data;
using DataLayer.domains;

namespace iTraycerSection.Validation
{
    public class Validate
    {
        public static UserInfo ValidateUser(UserInfo userInfo)
        {
            // use to validate the user name and pass
            userInfo = DataLayer.Controller.GetUserInfoFromEmailPassword(userInfo.RepEmail, userInfo.PassCode);

            if (userInfo == null)
            {
                return null;
            }
            return userInfo;
        }

        public static bool ValidateApplicationDeviceInfo(int repId, int coId, iTraycerDeviceInfo itd)
        {
            DataTable DevAppTable = DataLayer.Controller.GetItraycerApplicationDeviceInfoDevId(itd.DeviceId);

            if (DevAppTable.Rows.Count == 0)
            {
                if (DataLayer.Controller.GetiTraycerDeviceInfoByDeviceId(itd.DeviceId) == null)
                {
                    Device.Device.AddDeviceInfo(itd);    
                }

                if (DataLayer.Controller.GetiTraycerApplicationInfoByRepIdCoIdDeviceId(repId, coId, itd.DeviceId) == null)
                {
                    iTraycerApplication ita = new iTraycerApplication();
                    // create iTraycerApplication object
                    ita.RepId = repId;
                    ita.CoId = coId;
                    ita.CreatedDate = DateTime.UtcNow;
                    ita.LastSync = DateTime.UtcNow;
                    ita.DeviceId = itd.DeviceId;
                    ita.LaunchCount = 1;

                    //// insert a new row to the application table
                    if (DataLayer.Controller.InsertiTraycerApplicationInfo(ita) == 0)
                        Console.Write("Fail to insert row into Application Table");
                }
                return true;
            }
            else
            {
                if (DataLayer.Controller.UpdateiTraycerApplicationLaunchCount(repId, coId, itd.DeviceId) > 0)
                    return true;
            }
            return false;
        }

        public static bool ValidateSession(String guid)
        {
            // the time the session lives too
            iTraycerSession its = DataLayer.Controller.GetiTraycerSessionInfo(guid);
            DateTime timeToLive = its.SessionStartDateTime.AddMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["SessionTimeout"]));

            // 0 = time is equal, 1 = left is greater than right, < 0 time on left is less than time on right
            if (DateTime.Compare(timeToLive, DateTime.UtcNow) >= 0)
            {
                Console.ReadKey();
                return true;
            }
            Console.Write("Session expired");
            return false;
        }
    }
}
