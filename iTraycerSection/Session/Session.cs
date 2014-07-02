using System;
using System.Configuration;
using DataLayer.domains;
using MTSUtilities.Conversions;

namespace iTraycerSection.Session
{
    public class Session : iTraycerSection.Validation.Validate
    {
        public static UserInfo userInfo = new UserInfo();
        public static String deviceId = null;
        public static String errorMessage = null;
        public static DateTime? lastSync = null;
        public static Boolean sessionValid = false;

        public static String[] CreateUserSession(String email, String pass, String deviceId)
        {
            userInfo = ValidateUser(email, pass);

            if (userInfo.RepEmail == null)
            {
                errorMessage = "SRVERROR:Fail to login";
                return null;
            }

            // create session GUID, start session timer
            iTraycerSession its = new iTraycerSession();
            its.RepId = userInfo.Id;
            its.Guid = Guid.NewGuid().ToString();
            its.SessionStartDateTime = DateTime.UtcNow;
            its.UserInfo = Serialization.ObjSerializer(userInfo);
            its.DeviceId = deviceId;

            if (DataLayer.Controller.InsertiTraycerSessionInfo(its) == 0)
            {
                errorMessage = "SRVERROR:Fail to insert row in session table";
                return null;
            }
            return new string[] { userInfo.CustomerId.ToString(), userInfo.Id.ToString(), its.Guid + ":" + ConfigurationManager.AppSettings["SessionTimeout"]};
        }

        public static Boolean ValidateSession(String guid)
        {
            if(!String.IsNullOrEmpty(guid))
                guid = guid.Split(':')[0];

            iTraycerSession its = ValidateGUID(guid);

            if (its != null)
            {
                userInfo = (UserInfo)MTSUtilities.Conversions.Serialization.ObjDeSerializer(its.UserInfo);
                lastSync = GetLastSyncTime(Session.userInfo, its.DeviceId);
                sessionValid = true;
                return true;
            }
            else
            {
                errorMessage = "SRVERROR:Token does not exists";
            }

            DataLayer.Controller.DeleteiTraycerSession(guid);
            errorMessage = "SRVERROR:Token expired";
            return false;
        }

        public static DateTime? GetLastSyncTime(UserInfo userInfo, String deviceId)
        {
            return DataLayer.Controller.GetiTraycerLastSyncDateTime(userInfo, deviceId);
        }

        public static bool UpdateLastSyncTime(UserInfo userInfo, String deviceId, DateTime sync)
        {
            return DataLayer.Controller.UpdateiTraycerSyncTime(userInfo, deviceId, sync) > 0;
        }

        public static bool UpdateSessionStartTime(string guid)
        {
            return DataLayer.Controller.UpdateiTraycerSyncTime(userInfo, deviceId, sync) > 0;
        }

        public static bool UpdateApplicationInfo(UserInfo userInfo, String deviceId)
        {
            return DataLayer.Controller.UpdateiTraycerApplicationInfo(userInfo, deviceId) > 0;
        }
    }
}
