using DataLayer.domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace iTraycerSection.Case
{
    public class CaseScheduler
    {
        public static ScheduleInfo CreateScheduleInfoObj(XmlNode node)
        {
            List<XmlNode> nodeList = new List<XmlNode>(){
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
                                node.SelectSingleNode("//OrderSourceId"),
                                node.SelectSingleNode("//KitTypeNumber"),
                                node.SelectSingleNode("//PartNumber"),
                                node.SelectSingleNode("//CreatedDate")};
            return new ScheduleInfo(nodeList);
        }
    }
}
