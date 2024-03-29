﻿using DataLayer.domains;
using iTraycerSection.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

namespace iTraycerSection.InitData
{
    public class InitData
    {
        public static String GetInitialCaseData(DateTime? lastSync){
            List<ScheduleInfo> cases = null;

            if (lastSync == null)
            {
                cases = Session.Session.userInfo.IsSuperUser ?
                        DataLayer.Controller.GetSchedulesByCustomerId(Session.Session.userInfo.CustomerId) :
                        DataLayer.Controller.GetSchedulesByRep(Session.Session.userInfo);
            }
            else
            {
                cases = DataLayer.Controller.GetSchedulesByCustomerIdDateTime(Session.Session.userInfo.CustomerId, lastSync);
            }
            return cases.Count > 0 ? JsonConvert.SerializeObject(cases) : null;
        }

        public static String GetInitialInventoryData(DateTime? lastSync)
        {
            DataTable inventory = null;

            if (lastSync == null)
            {
                inventory = Session.Session.userInfo.IsSuperUser ?
                        DataLayer.Controller.GetAllInventoryInfoByCompanyId(Session.Session.userInfo.CustomerId) :
                        DataLayer.Controller.GetAllInventoryInfoByRepId(Session.Session.userInfo.Id);
            }
            else
            {
                inventory = Session.Session.userInfo.IsSuperUser ?
                        DataLayer.Controller.GetAllInventoryInfoByCompanyId(Session.Session.userInfo.CustomerId, Session.Session.lastSync) :
                        DataLayer.Controller.GetAllInventoryInfoByRepId(Session.Session.userInfo.Id, Session.Session.lastSync);
            }
            return SerializeTable(inventory);
        }

        public static String GetInitialDoctorsData(DateTime? lastSync)
        {
            DataTable doctorsList = null;

            if (lastSync == null)
                doctorsList = DataLayer.Controller.GetDocotorHospitalFilterByRepId(Session.Session.userInfo.Id);
            else
                doctorsList = DataLayer.Controller.GetDocotorHospitalFilterByRepId(Session.Session.userInfo.Id, Session.Session.lastSync);

            return SerializeTable(doctorsList);
        }

        public static String GetInitialAddressData(DateTime? lastSync)
        {
            DataTable repAddressInfoList = null;

            if (lastSync == null)
                repAddressInfoList = DataLayer.Controller.GetAddressesWithSourceTypeByRepRepRole(Session.Session.userInfo);
            else
                repAddressInfoList = DataLayer.Controller.GetAddressesWithSourceTypeByRepRepRole(Session.Session.userInfo, lastSync);

            return SerializeTable(repAddressInfoList);
        }

        public static String GetInitialStatusTableData(DateTime? lastSync)
        {
            DataTable statusTableCodes = null;

            if (lastSync == null)
                statusTableCodes = DataLayer.Controller.GetMTSStatusTable();
            else
                statusTableCodes = DataLayer.Controller.GetMTSStatusTable(lastSync);

            return SerializeTable(statusTableCodes);
        }

        public static String GetInitialTrayTypesBySurgeryType(int customerId)
        {
            DataTable surgeryTypeKits = DataLayer.Controller.GetInitialTrayTypesBySurgeryType(customerId);

            return surgeryTypeKits.Rows.Count > 0 ? JsonConvert.SerializeObject(surgeryTypeKits) : null;
        }

        public static String GetAllKitTrayUsageDates(DateTime? lastSync = null)
        {
            DataTable usageDateTable = null;

            if (lastSync == null)
                usageDateTable = DataLayer.Controller.GetAllKitTrayUsageDatesByCompanyId(Session.Session.userInfo.CustomerId);
            else
                usageDateTable = DataLayer.Controller.GetAllKitTrayUsageDatesByCompanyId(Session.Session.userInfo.CustomerId, lastSync);

            return SerializeTable(usageDateTable);
        }
        #region SyncData Object Aggregation
        public static XmlNode[] AddSyncObjects(XmlDocument payload, JsonEnvelope requestEnvelope)
        {
            XmlNode[] serviceQueueNodes = null;

            // create service name list to access in switch
            if (requestEnvelope.ServiceQueues.Length > 0)
            {
                serviceQueueNodes = (XmlNode[])requestEnvelope.ServiceQueues[0];
            }
           
            // expand the array to include the init data load objects;
            List<string> objectName = new List<String>() { "InitCases", "InitInventory", "InitDoctors", "InitAddresses", "InitStatus", "InitKitAllocation", 
                                                            "UpdateSyncTime" };
            List<XmlNode> queues = new List<XmlNode>();
            queues.AddRange(serviceQueueNodes);

            // use to figure out which objects are not in the list of request queues
            for (int i = 0; i < objectName.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < serviceQueueNodes.Length; j++)
                {

                    if (serviceQueueNodes[j].FirstChild.Name.Equals(objectName[i]))
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    objectName.Remove(objectName[i]);
                    i = 0;
                }
            }

            // add the missing objects to the list of serviceQueues to get Init Data changes
            for (int i = 0; i < objectName.Count; i++)
            {
                XmlElement payloadElement = payload.DocumentElement;
                if (objectName[i].Equals("InitCases"))
                {
                    XmlElement InitDataLoad = payload.CreateElement("item");
                    InitDataLoad.InnerXml = "<InitCases type=\"object\">Aggregate</InitCases>";
                    queues.Add(InitDataLoad);
                    continue;
                }
                if (objectName[i].Equals("InitInventory"))
                {
                    XmlElement InitInventory = payload.CreateElement("item");
                    InitInventory.InnerXml = "<InitInventory type=\"object\">Aggregate</InitInventory>";
                    queues.Add(InitInventory);
                    continue;
                }
                if (objectName[i].Equals("InitDoctors"))
                {
                    XmlElement InitDoctors = payload.CreateElement("item");
                    InitDoctors.InnerXml = "<InitDoctors type=\"object\">Aggregate</InitDoctors>";
                    queues.Add(InitDoctors);
                    continue;
                }
                if (objectName[i].Equals("InitAddresses"))
                {
                    XmlElement InitAddresses = payload.CreateElement("item");
                    InitAddresses.InnerXml = "<InitAddresses type=\"object\">Aggregate</InitAddresses>";
                    queues.Add(InitAddresses);
                    continue;
                }

                if (objectName[i].Equals("InitKitAllocation"))
                {
                    XmlElement InitKitAllocation = payload.CreateElement("item");
                    InitKitAllocation.InnerXml = "<InitKitAllocation type=\"object\">Aggregate</InitKitAllocation>";
                    queues.Add(InitKitAllocation);
                    continue;
                }
                if (objectName[i].Equals("UpdateSyncTime"))
                {
                    XmlElement UpdateSyncTime = payload.CreateElement("item");
                    UpdateSyncTime.InnerXml = "<UpdateSyncTime type=\"object\">Aggregate</UpdateSyncTime>";
                    queues.Add(UpdateSyncTime);
                    continue;
                }
            }
            // reasign the new set of queue objects to the original list
            return queues.ToArray();
        }
        #endregion

        public static String SerializeTable(DataTable table)
        {
            return (table.Rows.Count > 0) ? JsonConvert.SerializeObject(table) : null;
        }

        public static bool isQualifiedRole()
        {
            if (Session.Session.userInfo.IsDS || Session.Session.userInfo.IsCS)
            {
                Session.Session.errorMessage = "Unable to get data from user role: " + Session.Session.userInfo.Role;
                return false;
            }
            return true;
        }
    }
}