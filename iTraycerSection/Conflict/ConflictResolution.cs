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
        public DataTable GetCaseScheduledInfoByCompanyId(int companyId)
        {
            return DataLayer.Controller.GetAllKitTrayUsageDatesByCompanyId(companyId);
        }

        public bool CheckForSchedulingConflict(int companyId)
        {
            DataTable schedules = GetCaseScheduledInfoByCompanyId(companyId);

            DataRow[] scheduleRows = schedules.Select("");

            return false;
        }
    }
}
