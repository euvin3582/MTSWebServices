using DataLayer.domains;
using MTSUtility.;
using System;
using System.Configuration;
using System.Data;
using System.Linq;

namespace iTarycerSection.Session
{
    public class Session : Validation.Validate
    {
        public static void CreateUserSession(String email, String pass, iTraycerDeviceInfo itd)
        {
            // encrypt password before storing it
            string hashPass = DataLayer.Controller.CreateHash(pass);
            UserInfo userInfo = new UserInfo();
            userInfo.RepEmail = email;
            userInfo.PassCode = hashPass;

            userInfo = ValidateUser(userInfo);

            if (userInfo == null)
            {
                Console.Write("No valid user was found");
                return;
            }

            if (!ValidateApplicationDeviceInfo(userInfo, itd))
            {
                Console.Write("No valid device was specified");
                return;
            }

            // create session GUID, start session timer
            iTraycerSession its = new iTraycerSession();
            its.RepId = userInfo.Id;
            its.Guid = Guid.NewGuid().ToString();
            its.SessionStartDateTime = DateTime.UtcNow;
            its.UserInfo = Common.ObjSerializer(userInfo);

            if (DataLayer.Controller.InsertiTraycerSessionInfo(its) == 0)
            {
                Console.Write("Fail to insert create row in session table");
                return;
            }

            return 
        }
    }
}
