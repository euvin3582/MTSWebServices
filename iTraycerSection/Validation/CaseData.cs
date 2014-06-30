using System.Text.RegularExpressions;
using DataLayer.domains;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTraycerSection.Validation
{
    public class CaseData
    {
        public static bool ValidateCaseSchedule(ScheduleInfo obj)
        {
            bool valid = false;
            foreach (var prop in obj.GetType().GetProperties())
            {
                if(prop.GetValue(obj, null) == null)
                {
                    if (!Regex.IsMatch(prop.Name, "TrayNumber || Surgeon || Hospital || ModifiedDate || PatientId || KitTypeNumber || PartNumber || RequisitionId " +
                        "|| Description || TrayId || ReasonCode"))
                    {
                        Session.Session.errorMessage = String.Format("{0} is a required field to create a case. Value either missing or null. Value: {1}", prop.Name, prop.GetValue(obj, null));
                        break;
                    }
                }
                valid = true;
            }

            return valid;
        }
    }
}
