using DataLayer.domains;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTraycerSection.Conflict
{
    public class ConflictResolution
    {
        private static DataTable GetCaseScheduledInfoByCompanyId(int companyId)
        {
            return DataLayer.Controller.GetAllKitTrayUsageDatesByCompanyId(companyId);
        }

        public static bool CheckForSchedulingConflict(ScheduleInfo obj)
        {
            DataTable schedules = GetCaseScheduledInfoByCompanyId(obj.CompanyId);

            DataRow[] scheduleRows = schedules.Select(String.Format("DoctorId = {0} AND LocationID = {1}", obj.SurgeonId, obj.HospitalInfo.Id));

            return false;
        }
    }
}
