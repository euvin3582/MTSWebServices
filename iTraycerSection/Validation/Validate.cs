using System.Configuration;
using DataLayer.domains;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTarycerSection.Validation
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

        public static bool ValidateApplicationDeviceInfo(UserInfo userInfo, iTraycerDeviceInfo itd)
        {
            DataTable DevAppTable = DataLayer.Controller.GetItraycerApplicationDeviceInfoByRepCoDevId(userInfo.Id, userInfo.CustomerId, itd.DeviceId);

            if (DevAppTable.Rows.Count == 0)
            {
                if (DataLayer.Controller.GetiTraycerDeviceInfoByDeviceId(itd.DeviceId) == null)
                {
                    if (DataLayer.Controller.InsertiTraycerDeviceInfo(itd) == 0)
                        Console.Write("Fail to write to db");
                }

                if (DataLayer.Controller.GetiTraycerApplicationInfoByRepIdCoIdDeviceId(userInfo.Id, userInfo.CustomerId, itd.DeviceId) == null)
                {
                    iTraycerApplication ita = new iTraycerApplication();
                    // create iTraycerApplication object
                    ita.RepId = userInfo.Id;
                    ita.CoId = userInfo.CustomerId;
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
                if (DataLayer.Controller.UpdateiTraycerApplicationLaunchCount(userInfo.Id, userInfo.CustomerId, itd.DeviceId) > 0)
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
