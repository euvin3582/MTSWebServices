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
            DataTable devAppTable = DataLayer.Controller.GetiTraycerApplicationDeviceInfoDevId(itd.DeviceId);

            if (devAppTable.Rows.Count == 1)
            {
                if (DataLayer.Controller.UpdateiTraycerApplicationLaunchCount(repId, coId, itd.DeviceId, launchCount) > 0)
                    return true;
            }
            else
            {
                iTraycerSection.Session.Session.errorMessage = "SRVERROR:Fail to update launch count";
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
                MTSLoggerLib.Entities.Logger.MobileLog mobileErrorLogger = new MTSLoggerLib.Entities.Logger.MobileLog();

                // update session token expiration date time
                if (Session.Session.UpdateSessionStartTime(guid))
                    return its;
                else
                {
                    mobileErrorLogger.RepId = Session.Session.userInfo.Id;
                    mobileErrorLogger.CompanyId = Session.Session.userInfo.CustomerId;
                    mobileErrorLogger.RequestTime = DateTime.Now;
                    mobileErrorLogger.DeviceId = Session.Session.deviceId;
                    mobileErrorLogger.ErrorObjectName = "ValidateGUID";
                    mobileErrorLogger.SrvErrorMsg = "Fail to update TTL for Session Token";
                    MTSUtilities.Logger.Log.MOBILEToDB(mobileErrorLogger);
                }
            }
            return null;
        }
    }
}
