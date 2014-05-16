using System;
using DataLayer.domains;
using System.Drawing;
using System.Configuration;
using System.IO;
using PDFGenChargesForm.Classes;
using System.Collections.Generic;


namespace iTraycerSection.Invoice
{
    public class Invoice
    {
        public byte[] CreateInvoice(List<RequisitionOrder> reqOrders, Image repSigImg = null, Image hosSigImg = null)
        {
            return CreateInvoice(reqOrders, null, null, repSigImg, hosSigImg);
        }

        public byte[] CreateInvoice(List<RequisitionOrder> reqOrders, String repSig = null, String hosSig = null, Image repSigImg = null, Image hosSigImg = null)
        {
            // get company logo
            CustomerImages cutomerImage = DataLayer.Controller.GetCustomerImage("SpineWaveLogo", 227);
            iTextSharp.text.Image RepSigatureImg = ResizeAndConvert(repSig, repSigImg);
            iTextSharp.text.Image HospitalSignatureImg = ResizeAndConvert(hosSig, hosSigImg);
            iTextSharp.text.Image customerLogo = iTextSharp.text.Image.GetInstance(cutomerImage.CustomerImage, cutomerImage.ImageFormat);

            if (reqOrders.Count == 0)
                return null;

            ReportOptions reportOption = new ReportOptions();
            reportOption.PhoneNo = reqOrders[0].HosBillingPhone;
            reportOption.Fax = "NA";
            reportOption.Email = "Need to get More Info On This";
            reportOption.BillTo = reqOrders[0].HosiptalName;
            reportOption.SurgeryDate = (DateTime)reqOrders[0].SurgeryDate;
            reportOption.CityState = reqOrders[0].HosBillingCity + ", " + reqOrders[0].HosBillingState;
            reportOption.PhysicianName = reqOrders[0].PhysicianName;
            reportOption.PONum = "P O Num";
            reportOption.PurchasePhone = reqOrders[0].HosBillingPhone;
            reportOption.XD = String.Empty;
            reportOption.Sniper = String.Empty;
            reportOption.PS = String.Empty;
            reportOption.Miss = String.Empty;
            reportOption.CrossConn = String.Empty;
            reportOption.Other = String.Empty;
            reportOption.Facility = String.Empty;
            reportOption.CaseDateTime = DateTime.Now;
            reportOption.Physician = String.Empty;
            reportOption.ShipTo = reqOrders[0].ShippingName;
            reportOption.CaseType = reqOrders[0].SurgeryType;
            reportOption.Levels = reqOrders[0].VerdibraeLevel.ToString();
            reportOption.EffectiveDate = DateTime.Now;
            reportOption.DeliveryCheck = false;
            reportOption.ConsumedLabels = String.Empty;
            reportOption.HospitalSignature = reqOrders[0].PhysicianName;
            reportOption.RepresentativeSignature = reqOrders[0].RepName;
            reportOption.RepresentativeSignatureImage = (RepSigatureImg != null) ? RepSigatureImg : null;
            reportOption.HospitalSignatureImage = (HospitalSignatureImg != null) ? HospitalSignatureImg  : null;
            reportOption.PartLOTList = PartDetail.GetPartDetails();

            return PDfHelper.CreatePdfDoc(reportOption, (customerLogo != null) ? customerLogo : null);
        }

        public iTextSharp.text.Image ResizeAndConvert(String imgString = null, System.Drawing.Image imgObj = null)
        {
            bool imageOversize = false;
            System.Drawing.Image resizeImage = null;

            if (imgObj == null && imgString != null)
                imgObj = MTSUtilities.ImageUtilities.Serialization.ImageDecoding(imgString);

            if (imgObj != null)
            {
                imageOversize = ((imgObj.Width > 300) || imgObj.Height > 60);

                if (imageOversize)
                    resizeImage = MTSUtilities.ImageUtilities.ImageScaling.ScaleImage(imgObj, 300, 60);
            }
            return iTextSharp.text.Image.GetInstance(imageOversize ? resizeImage : imgObj, MTSUtilities.ImageUtilities.ImageAttributes.GetImageFormat(imgObj));
        }

        public System.Drawing.Image GetCaseSigImage(int caseNumber)
        {
            return null;        
        }

        public void CreatePdfDocumentFromByteArray(Byte[] pdfMs)
        {
            String defaultFilePath = ConfigurationManager.AppSettings["PDFFilePath"];
            Guid guid = Guid.NewGuid();
            String fileName = defaultFilePath + guid.ToString() + ".pdf";

            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                MemoryStream memoryStream = new MemoryStream(pdfMs);
                memoryStream.WriteTo(file);
                file.Close();
                memoryStream.Close();
            }
        }

        public MemoryStream CreatePdfMemStreamFromByteArray(Byte[] pdfMs)
        {
            String defaultFilePath = ConfigurationManager.AppSettings["PDFFilePath"];
            Guid guid = Guid.NewGuid();
            String fileName = defaultFilePath + guid.ToString() + ".pdf";

            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                MemoryStream memoryStream = new MemoryStream(pdfMs);
                return memoryStream;
            }
        }
    }
}
