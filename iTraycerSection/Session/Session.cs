using System;
using System.Configuration;
using System.Data;
using System.Linq;
using DataLayer.domains;
using MTSUtilities.Conversions;

namespace iTraycerSection.Session
{
    public class Session : iTraycerSection.Validation.Validate
    {
        public static UserInfo userInfo = new UserInfo();
        public static String errorMessage = null;

        public static String[] CreateUserSession(String email, String pass)
        {
            userInfo = ValidateUser(email, pass);

            if (userInfo == null)
            {
                errorMessage = "SRVERROR:No valid user was found";
                return null;
            }

            // create session GUID, start session timer
            iTraycerSession its = new iTraycerSession();
            its.RepId = userInfo.Id;
            its.Guid = Guid.NewGuid().ToString();
            its.SessionStartDateTime = DateTime.UtcNow;
            its.UserInfo = Serialization.ObjSerializer(userInfo);

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
    }
}
