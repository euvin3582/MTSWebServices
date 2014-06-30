using DataLayer.domains;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace iTraycerSection.Case
{
    public class CaseScheduler
    {
        public static ScheduleInfo CreateScheduleInfoObj(XmlNode node)
        {
            try
            {
                ScheduleInfo obj = new ScheduleInfo();

                
                obj.Id = String.IsNullOrEmpty(node.SelectSingleNode("//CaseId").InnerText) ? -1 : Convert.ToInt32(node.SelectSingleNode("//CaseId").InnerText);
                obj.LocalId = String.IsNullOrEmpty(node.SelectSingleNode("//LocalId").InnerText) ? null : (Int32?)Convert.ToInt32(node.SelectSingleNode("//LocalId").InnerText);
                obj.SurgeonInfo = new DoctorInfo(); // create new docInfo object
                obj.SurgeonId = String.IsNullOrEmpty(node.SelectSingleNode("//SurgeonId").InnerText) ? -1 : Convert.ToInt32(node.SelectSingleNode("//SurgeonId").InnerText);
                obj.SurgeryDate = Convert.ToDateTime(node.SelectSingleNode("//SurgeryDate").InnerText);
                obj.DeliverByDate = String.IsNullOrEmpty(node.SelectSingleNode("//DeliverByDate").InnerText) ? obj.SurgeryDate.AddDays(-1) : (DateTime?)Convert.ToDateTime(node.SelectSingleNode("//DeliverByDate").InnerText);
                obj.VerdibraeLevel = String.IsNullOrEmpty(node.SelectSingleNode("//VerdibraeLevel").InnerText) ? null : node.SelectSingleNode("//VerdibraeLevel").InnerText;
                obj.ModifiedDate = String.IsNullOrEmpty(node.SelectSingleNode("//ModifiedDate").InnerText) ? DateTime.Now : (DateTime?)Convert.ToDateTime(node.SelectSingleNode("//ModifiedDate").InnerText);
                obj.ModifiedFrom = 'M';
                obj.SurgeryType = node.SelectSingleNode("//SurgeryType").InnerText;
                obj.MedicalRecordNumber = String.IsNullOrEmpty(node.SelectSingleNode("//MedicalRecordNumber").InnerText) ? null : node.SelectSingleNode("//MedicalRecordNumber").InnerText;
                obj.PatientId = String.IsNullOrEmpty(node.SelectSingleNode("//PatientId").InnerText) ? null : node.SelectSingleNode("//PatientId").InnerText;
                obj.SurgeryStatus = String.IsNullOrEmpty(node.SelectSingleNode("//SurgeryStatus").InnerText) ? 7 : Convert.ToInt32(node.SelectSingleNode("//SurgeryStatus").InnerText);
                obj.HospitalInfo = new HospitalInfo();
                obj.LocationId = String.IsNullOrEmpty(node.SelectSingleNode("//LocationId").InnerText) ? -1 : Convert.ToInt32(node.SelectSingleNode("//LocationId").InnerText);
                obj.HospitalInfo.Id = obj.LocationId;
                obj.LoanerFlag = String.IsNullOrEmpty(node.SelectSingleNode("//LoanerFlag").InnerText) ? 'N' : Convert.ToChar(node.SelectSingleNode("//LoanerFlag").InnerText);
                obj.OrderSourceId = 4;
                obj.KitTypeNumber = String.IsNullOrEmpty(node.SelectSingleNode("//KitTypeNumber").InnerText) ? null : node.SelectSingleNode("//KitTypeNumber").InnerText;
                obj.PartNumber = String.IsNullOrEmpty(node.SelectSingleNode("//PartNumber").InnerText) ? null : node.SelectSingleNode("//PartNumber").InnerText;
                obj.CreatedDate = String.IsNullOrEmpty(node.SelectSingleNode("//CreatedDate").InnerText) ? DateTime.Now : Convert.ToDateTime(node.SelectSingleNode("//CreatedDate").InnerText);
            
                return obj;
            }
            catch (Exception ex)
            {
                Session.Session.errorMessage = "Fail to create schedule object from JSON create case data";
                MTSLoggerLib.Entities.Logger.MobileLog mobileErrorLogger = new MTSLoggerLib.Entities.Logger.MobileLog();
                mobileErrorLogger.SrvErrorException = ex.Message;
                mobileErrorLogger.SrvErrorMsg = Session.Session.errorMessage;
                MTSUtilities.Logger.Log.MOBILEToDB(mobileErrorLogger);
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
