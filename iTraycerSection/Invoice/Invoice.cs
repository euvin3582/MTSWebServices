using System;
using PDFGenChargesForm.Classes;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DataLayer.domains;
using System.Web.UI;
using System.Resources;
using System.Reflection;

namespace iTraycerSection.Invoice
{
    public class Invoice : System.Web.UI.Page
    {
        public void CreateInvoice(RequisitionOrder reqOrder, String repSig = null, String hosSig = null)
        {
            Image RepSigIMg  = null;
            if(repSig != null)
                RepSigIMg = MTSUtilities.ImageUtilities.Serialization.ImageDecoding(repSig);
            Image HosSigIMg  = null;
            if (hosSig != null)
                HosSigIMg = MTSUtilities.ImageUtilities.Serialization.ImageDecoding(repSig);

            String imageDirectory = Page.Server.MapPath("~\\Images\\");
            bool imgOversize = false;

            ReportOptions reportOption = new ReportOptions();
            reportOption.PhoneNo = "1-877-844-4225";
            reportOption.Fax = "1-877-241-7480";
            reportOption.Email = "customerservice@spinewave.com";
            reportOption.BillTo = "123";
            reportOption.SurgeryDate = DateTime.Now;
            reportOption.CityState = "Dallas, TX";
            reportOption.PhysicianName = "Physician";
            reportOption.PONum = "P O Num";
            reportOption.PurchasePhone = "1234567890";
            reportOption.XD = String.Empty;
            reportOption.Sniper = String.Empty;
            reportOption.PS = String.Empty;
            reportOption.Miss = String.Empty;
            reportOption.CrossConn = String.Empty;
            reportOption.Other = String.Empty;
            reportOption.Facility = String.Empty;
            reportOption.CaseDateTime = DateTime.Now;
            reportOption.Physician = String.Empty;
            reportOption.ShipTo = String.Empty;
            reportOption.CaseType = String.Empty;
            reportOption.Levels = String.Empty;
            reportOption.EffectiveDate = DateTime.Now;
            reportOption.DeliveryCheck = false;
            reportOption.ConsumedLabels = String.Empty;

            if (RepSigIMg != null)
            {
                Image resizeSig = null;
                ImageFormat format = MTSUtilities.ImageUtilities.ImageAttributes.GetImageFormat(RepSigIMg);
                imgOversize = ((RepSigIMg.Width > 300) || RepSigIMg.Height > 60);
                
                // resize the image if needed
                if (imgOversize)
                {
                    resizeSig = MTSUtilities.ImageUtilities.ImageScaling.ScaleImage(RepSigIMg, 300, 60);
                }
                reportOption.HospitalSignatureImage = iTextSharp.text.Image.GetInstance(((resizeSig == null) ? RepSigIMg : resizeSig), format);
            }

            if (HosSigIMg != null)
            {
                Image resizeSig = null;
                ImageFormat format = MTSUtilities.ImageUtilities.ImageAttributes.GetImageFormat(HosSigIMg);
                imgOversize = ((HosSigIMg.Width > 300) || HosSigIMg.Height > 60);

                // resize the image if needed
                if (imgOversize)
                {
                    resizeSig = MTSUtilities.ImageUtilities.ImageScaling.ScaleImage(HosSigIMg, 300, 60);
                }
                reportOption.HospitalSignatureImage = iTextSharp.text.Image.GetInstance(((resizeSig == null) ? HosSigIMg : resizeSig), format);
            }
            reportOption.HospitalSignature = "Dr. Chill";
            reportOption.RepresentativeSignature = "Jose Euvin";

            reportOption.PartLOTList = PartDetail.GetPartDetails();

            PDfHelper.CreatePdfDoc(reportOption, imageDirectory);
        }
    }
}
