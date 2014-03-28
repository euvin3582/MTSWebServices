﻿using System;
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

        public static String[] CreateUserSession(String email, String pass)
        {
            userInfo = ValidateUser(email, pass);

            if (userInfo == null)
            {
                Console.Write("No valid user was found");
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
                Console.Write("Fail to insert create row in session table");
                return null;
            }
            return new string[] { userInfo.CustomerId.ToString(), userInfo.Id.ToString(), its.Guid + ":" + ConfigurationManager.AppSettings["SessionTimeout"]};
        }
    }
}
