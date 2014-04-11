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
            List<TrayInfo> trays = null;

            // get Inventory list depending on user
            if (lastSync == null)
            {
                trays = Session.userInfo.IsSuperUser ?
                        DataLayer.Controller.GetInventoryByCompanyId(Session.userInfo.CustomerId) :
                        DataLayer.Controller.GetInventoryByRepId(Session.userInfo.Id);
            }
            else
            {
                trays = DataLayer.Controller.GetInventoryByRepIdDateTime(Session.userInfo.Id, lastSync);
            }
            return JsonConvert.SerializeObject(trays);
        }

        public static String GetInitialDoctorsData(DateTime? lastSync)
        {
            DataTable doctorsList = new DataTable();

            // get case list depending on user
            if (lastSync == null)
            {
                doctorsList = DataLayer.Controller.GetDocotorHospitalFilterByRepId(Session.userInfo.Id);
            }
            else
            {
                doctorsList = DataLayer.Controller.GetDocotorHospitalFilterByRepId(Session.userInfo.Id, Session.lastSync);
            }
            
            return JsonConvert.SerializeObject(doctorsList);
        }

        public static String GetInitialAddressData(DateTime? lastSync)
        {
            List<AddressInfo> addresses = null;

            // get Inventory list depending on user
            if (lastSync == null)
            {
                String repList = JsonConvert.SerializeObject(DataLayer.Controller.GetAllRepsList(Session.userInfo));
                String hosList = JsonConvert.SerializeObject(DataLayer.Controller.GetHospitalsList(Session.userInfo));
                String disList = JsonConvert.SerializeObject(DataLayer.Controller.GetDistributorsList(Session.userInfo));
                String data = (String.IsNullOrEmpty(repList) ?  "" : repList) +
                              (String.IsNullOrEmpty(repList) ? "" : hosList) +
                              (String.IsNullOrEmpty(repList) ? "" : disList);
                return data;

            }
            else
            {
                // get sync addresses
            }
            return JsonConvert.SerializeObject(addresses);
        }

        public static XmlNode[] AddSyncObjects(XmlDocument payload, JsonEnvelope requestEnvelope)
        {
            XmlNode[] serviceQueueNodes = null;

            // create service name list to access in switch
            if (requestEnvelope.ServiceQueues.Length > 0)
            {
                serviceQueueNodes = (XmlNode[])requestEnvelope.ServiceQueues[0];
            }
           
            // expand the array to include the init data load objects;
            List<string> objectName = new List<String>() { "InitDataLoad", "InitInventory", "InitDoctors", "InitAddresses" };
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
            }
            // reasign the new set of queue objects to the original list
            return queues.ToArray();
        }
    }
}