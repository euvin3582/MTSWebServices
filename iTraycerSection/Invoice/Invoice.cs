using System;
using PDFGenChargesForm.Classes;
using DataLayer.domains;
using System.Drawing;


namespace iTraycerSection.Invoice
{
    public class Invoice
    {
        public byte[] CreateInvoice(RequisitionOrder reqOrder, Image repSigImg = null, Image hosSigImg = null)
        {
            return CreateInvoice(reqOrder, null, null, repSigImg, hosSigImg);
        }

        public byte[] CreateInvoice(RequisitionOrder reqOrder, String repSig = null, String hosSig = null, Image repSigImg = null, Image hosSigImg = null)
        {
            // get company logo
            CustomerImages cutomerImage = DataLayer.Controller.GetCustomerImage("SpineWaveLogo", 227);
            iTextSharp.text.Image RepSigatureImg = ResizeAndConvert(repSig, repSigImg);
            iTextSharp.text.Image HospitalSignatureImg = ResizeAndConvert(hosSig, hosSigImg);
            iTextSharp.text.Image customerLogo = iTextSharp.text.Image.GetInstance(cutomerImage.CustomerImage, cutomerImage.ImageFormat);

            ReportOptions reportOption = new ReportOptions();
            reportOption.PhoneNo = reqOrder.HosBillingPhone;
            reportOption.Fax = "1-877-241-7480";
            reportOption.Email = "Need to get More Info On This";
            reportOption.BillTo = reqOrder.BillingAddressId.ToString();
            reportOption.SurgeryDate = (DateTime)reqOrder.SurgeryDate;
            reportOption.CityState = reqOrder.HosBillingCity + ", " + reqOrder.HosBillingState;
            reportOption.PhysicianName = reqOrder.PhysicianName;
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
            reportOption.ShipTo = reqOrder.ShippingName;
            reportOption.CaseType = reqOrder.SurgeryType;
            reportOption.Levels = reqOrder.VerdibraeLevel.ToString();
            reportOption.EffectiveDate = DateTime.Now;
            reportOption.DeliveryCheck = false;
            reportOption.ConsumedLabels = String.Empty;
            reportOption.HospitalSignature = reqOrder.PhysicianName;
            reportOption.RepresentativeSignature = reqOrder.RepName;
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
    }
}
