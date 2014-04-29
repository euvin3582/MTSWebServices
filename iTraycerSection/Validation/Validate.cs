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

        public static bool ValidateApplicationDeviceInfo(int repId, int coId, iTraycerDeviceInfo itd, int launchCount)
        {
            DataTable DevAppTable = DataLayer.Controller.GetiTraycerApplicationDeviceInfoDevId(itd.DeviceId);

            if (DevAppTable.Rows.Count == 1)
            {
                if (DataLayer.Controller.UpdateiTraycerApplicationLaunchCount(repId, coId, itd.DeviceId, launchCount) > 0)
                    return true;
            }
            else
            {
                Session.Session.errorMessage = "SRVERROR:Fail to update launch count";
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
