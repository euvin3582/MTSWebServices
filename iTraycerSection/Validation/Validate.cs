using System.Configuration;
using System;
using System.Data;
using DataLayer.domains;
using DataLayer.utilities;

namespace iTraycerSection.Validation
{
    public class Validate
    {
        public static UserInfo ValidateUser(string txtUserCode, string txtPassword)
        {
            // use to validate the user name and pass
            UserInfo obj = AuthenticateUser.HasAccessControl(txtUserCode.Trim(), txtPassword.Trim());

            if (obj == null)
            {
                return null;
            }
            return obj;
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

        public static iTraycerSession ValidateGUID(String guid)
        {
            // the time the session lives too
            iTraycerSession its = DataLayer.Controller.GetiTraycerSessionInfo(guid);

            if (its == null)
                return null;

            DateTime expirationTime = its.SessionStartDateTime.AddMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["SessionTimeout"]));

            // 0 = time is equal, 1 = left is greater than right, (< 0 or -1) = time on left is less than time on right
            if (DateTime.Compare(expirationTime, DateTime.UtcNow) >= 0)
            {
                return its;
            }
            return null;
        }
    }
}
