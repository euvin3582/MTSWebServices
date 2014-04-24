using iTextSharp.text;
using System;
using System.Collections.Generic;

namespace PDFGenChargesForm.Classes
{
    public class ReportOptions
    {
        #region Accessors

        public String PhoneNo { get; set; }

        public String Fax { get; set; }

        public String Email { get; set; }

        public String BillTo { get; set; }

        public DateTime SurgeryDate { get; set; }

        public String CityState { get; set; }

        public String PhysicianName { get; set; }

        public String PONum { get; set; }

        public String PurchasePhone { get; set; }

        public String XD { get; set; }

        public String Sniper { get; set; }

        public String PS { get; set; }

        public String Miss { get; set; }

        public String CrossConn { get; set; }

        public String Other { get; set; }

        public String Facility { get; set; }

        public DateTime CaseDateTime { get; set; }

        public String Physician { get; set; }

        public String ShipTo { get; set; }

        public String CaseType { get; set; }

        public String Levels { get; set; }

        public DateTime EffectiveDate { get; set; }

        public Boolean DeliveryCheck { get; set; }

        public String ConsumedLabels { get; set; }

        public String HospitalSignature { get; set; }
        public Image HospitalSignatureImage { get; set; }

        public String RepresentativeSignature { get; set; }
        public Image RepresentativeSignatureImage { get; set; }
        
        public List<PartDetail> PartLOTList { get; set; }

        #endregion
    }
}