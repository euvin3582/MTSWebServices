using DataLayer.domains;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace iTraycerSection.Conflict
{
    public class ConflictResolution
    {
        public static bool CheckForConflict(String conflictType, Object obj)
        {
            bool isConflict = false;

            switch(conflictType){
                // Same doctor, same location, same time - Conflict gets resolved by creation date
                case "SCheduledConflict":
                    DataTable scheduleConflict = DataLayer.Controller.GetAllKitTrayUsageDatesByCompanyId(((ScheduleInfo)obj).CompanyId);
                    isConflict =  SCheduledConflict(scheduleConflict, (ScheduleInfo)obj, "MTSSurgeryDetails", "RepId");
                    break;

                case "OtherConflict":
                    break;
            }
            return isConflict;
        }

        // Same doctor, same location, same time
        private static bool SCheduledConflict(DataTable schedules, ScheduleInfo obj, String tableName, String conflictColumn)
        {
            // remove duplicates
            DataView view = new DataView(schedules);
            schedules = view.ToTable(true, "CaseId", "RepID", "DoctorId", "LocationID",	"SurgeryDate", "ModifiedDate");

            // find conflicting rows
            DataRow[] scheduleRows = schedules.Select(String.Format("DoctorId = '{0}' AND LocationID = '{1}' AND SurgeryDate = '{2}'", obj.SurgeonId, obj.HospitalInfo.Id, obj.SurgeryDate));

            if (scheduleRows.Length == 0)
                return false;

            string conflictProposedVal = GetConflictProperty(obj, conflictColumn);

            if (conflictProposedVal != null)
                DataLayer.Controller.InsertMultipleConflict(scheduleRows, tableName, obj.RepId, conflictColumn, conflictProposedVal, obj.CreatedDate);

            return true;
        }

        private static String GetConflictProperty(Object obj, String conflictColumn)
        {
            Type objType = obj.GetType();
            IList<PropertyInfo> props = null;

            if (objType != null)
            {
                props = new List<PropertyInfo>(objType.GetProperties());
            }

            if (props != null)
            {
                foreach (PropertyInfo prop in props)
                {
                    if (prop != null && prop.Name.Equals(conflictColumn))
                    {
                        return prop.GetValue(obj, null).ToString();
                    }
                }
            }
            return null;
        }
    }
}
