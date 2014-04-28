using DataLayer.domains;
using iTraycerSection.Session;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;

namespace FacadeRestService
{
    public class InitData
    {
        public static String GetInitialCaseData(DateTime? lastSync){
            List<ScheduleInfo> cases = null;

            // get case list depending on user
            if (lastSync == null)
            {
                cases = Session.userInfo.IsSuperUser ?
                        DataLayer.Controller.GetSchedulesByCustomerId(Session.userInfo.CustomerId) :
                        DataLayer.Controller.GetSchedulesByRep(Session.userInfo);
            }
            else
            {
                cases = DataLayer.Controller.GetSchedulesByCustomerIdDateTime(Session.userInfo.CustomerId, lastSync);
            }
            return JsonConvert.SerializeObject(cases);
        }

        public static String GetInitialInventoryData(DateTime? lastSync)
        {
            DataTable inventory = new DataTable();

            // get Inventory list depending on user
            if (lastSync == null)
            {
                inventory = Session.userInfo.IsSuperUser ?
                        DataLayer.Controller.GetAllInventoryInfoByCompanyId(Session.userInfo.CustomerId) :
                        DataLayer.Controller.GetAllInventoryInfoByRepId(Session.userInfo.Id);
            }
            else
            {
                //inventory = DataLayer.Controller.GetInventoryByRepIdDateTime(Session.userInfo.Id, lastSync);
            }
            return JsonConvert.SerializeObject(inventory);
        }

        public static String GetInitialDoctorsData(DateTime? lastSync)
        {
            DataTable doctorsList = new DataTable();

            // get case list depending on user
            if (lastSync == null)
                doctorsList = DataLayer.Controller.GetDocotorHospitalFilterByRepId(Session.userInfo.Id);
            else
                doctorsList = DataLayer.Controller.GetDocotorHospitalFilterByRepId(Session.userInfo.Id, Session.lastSync);
            
            return JsonConvert.SerializeObject(doctorsList);
        }

        public static String GetInitialAddressData(DateTime? lastSync)
        {
            String repAddressInfoList = null;

            // get Address list depending on user role
            if (lastSync == null)
                repAddressInfoList = JsonConvert.SerializeObject(DataLayer.Controller.GetAddressesWithSourceTypeByRepRepRole(Session.userInfo));
            else
                repAddressInfoList = JsonConvert.SerializeObject(DataLayer.Controller.GetAddressesWithSourceTypeByRepRepRole(Session.userInfo, lastSync));

            return JsonConvert.SerializeObject(repAddressInfoList);
        }

        public static String GetInitialStatusTableData(DateTime? lastSync)
        {
            String statusTableCodes = null;

            if (lastSync == null)
                statusTableCodes = JsonConvert.SerializeObject(DataLayer.Controller.GetMTSStatusTable());
            else
                statusTableCodes = JsonConvert.SerializeObject(DataLayer.Controller.GetMTSStatusTable(lastSync));

            return JsonConvert.SerializeObject(statusTableCodes);
        }

        public static String GetAllKitTrayUsageDates(DateTime? lastSync)
        {
            String usageDate = null;

            if (lastSync == null)
                usageDate = JsonConvert.SerializeObject(DataLayer.Controller.GetAllKitTrayUsageDatesByRepId(Session.userInfo.Id));
            else
                usageDate = JsonConvert.SerializeObject(DataLayer.Controller.GetAllKitTrayUsageDatesByRepId(Session.userInfo.Id, lastSync));

            return JsonConvert.SerializeObject(usageDate);
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
            List<string> objectName = new List<String>() { "InitDataLoad", "InitInventory", "InitDoctors", "InitAddresses", "InitStatus", "InitKitAllocation" };
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
                if (objectName[i].Equals("InitDataLoad"))
                {
                    XmlElement InitDataLoad = payload.CreateElement("item");
                    InitDataLoad.InnerXml = "<InitDataLoad type=\"object\">Aggregate</InitDataLoad>";
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
                if (objectName[i].Equals("InitStatus"))
                {
                    XmlElement InitStatus = payload.CreateElement("item");
                    InitStatus.InnerXml = "<InitStatus type=\"object\">Aggregate</InitStatus>";
                    queues.Add(InitStatus);
                    continue;
                }
                if (objectName[i].Equals("InitKitAllocation"))
                {
                    XmlElement InitKitAllocation = payload.CreateElement("item");
                    InitKitAllocation.InnerXml = "<InitKitAllocation type=\"object\">Aggregate</InitKitAllocation>";
                    queues.Add(InitKitAllocation);
                    continue;
                }
            }
            // reasign the new set of queue objects to the original list
            return queues.ToArray();
        }
        #endregion
    }
}