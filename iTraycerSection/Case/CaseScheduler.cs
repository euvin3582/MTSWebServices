using DataLayer.domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace iTraycerSection.Case
{
    public class CaseScheduler
    {
        public static ScheduleInfo CreateScheduleInfoObj(XmlNode node)
        {
            try
            {
                List<XmlNode> nodeList = new List<XmlNode>(){
                                node.SelectSingleNode("//LocalID"),
                                node.SelectSingleNode("//CaseId"),
                                node.SelectSingleNode("//SurgeonId"),
                                node.SelectSingleNode("//SurgeryDate"),
                                node.SelectSingleNode("//DeliverByDate"),
                                node.SelectSingleNode("//VerdibraeLevel"),
                                node.SelectSingleNode("//ModifiedDate"),
                                node.SelectSingleNode("//SurgeryType"),
                                node.SelectSingleNode("//MedicalRecordNumber"),
                                node.SelectSingleNode("//PatientId"),
                                node.SelectSingleNode("//SurgeryStatus"),
                                node.SelectSingleNode("//LocationId"),
                                node.SelectSingleNode("//LoanerFlag"),
                                node.SelectSingleNode("//KitTypeNumber"),
                                node.SelectSingleNode("//PartNumber"),
                                node.SelectSingleNode("//CreatedDate")};
                return new ScheduleInfo(nodeList);
            }
            catch (Exception)
            {
                Session.Session.errorMessage = "Fail to create node list";
                return null;
            }
        }

        public static int CreateDoctor(ScheduleInfo obj)
        {
            DoctorInfo newDoctor = new DoctorInfo();

            if (obj.Surgeon == null || !Regex.IsMatch(obj.Surgeon, @"^[\p{L}\p{M}' \.\-]+$"))
            {
                Session.Session.errorMessage = "Invalid Doctor name";
                return -1;
            }

            string[] name = Regex.Split(obj.Surgeon, @"^[.,!?;:]\s*|\s+");

            if (name.Length > 1)
            {
                newDoctor.FirstName = name[0];
                newDoctor.LastName = name[1];
                newDoctor.DoctorName = obj.Surgeon;
                newDoctor.HospitalId = obj.LocationId;
                newDoctor.DoctorType = obj.SurgeryType;
                newDoctor.Distributor = iTraycerSection.Session.Session.userInfo.CustomerId;
            }
            return DataLayer.Controller.InsertDoctor(newDoctor);
        }
    }
}
