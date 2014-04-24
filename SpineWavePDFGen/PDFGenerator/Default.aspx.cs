using System;
using PDFGenChargesForm.Classes;
using System.Drawing;
using System.Drawing.Imaging;

namespace PDFGenChargesForm
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Default values

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
            reportOption.HospitalSignature = "Dr. Chill";
            reportOption.RepresentativeSignature = "Jose Euvin";

            reportOption.PartLOTList = PartDetail.GetPartDetails();

            String imageDirectory = Page.Server.MapPath("~\\Images\\");
             
            string img = "AAEAAAD/////AQAAAAAAAAAMAgAAAFFTeXN0ZW0uRHJhd2luZywgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWIwM2Y1ZjdmMTFkNTBhM2EFAQAAABVTeXN0ZW0uRHJhd2luZy5CaXRtYXABAAAABERhdGEHAgIAAAAJAwAAAA8DAAAA/hQAAAKJUE5HDQoaCgAAAA1JSERSAAABwgAAAEsIBgAAAOPQh+EAAAABc1JHQgCuzhzpAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwwAADsMBx2+oZAAAFJNJREFUeF7t3QV0XFW7xnH4cKe4uxct7i1StDgFFlrctbi7u0Ox4u7uFHd39+LusO/97ZuTmy9MmkkyE5v3v9asQGcyOXPmnP28vodLQRAEQVDDhBAGQRAENU0IYRAEQVDThBAGQRAENU0IYRAEQVDThBAGQRAENU0IYRAEQVDThBAGQRAENU0IYRAEQVDThBAGQRAENU0IYRAEQVDThBAGQRAENU0IYRAEQVDThBAGQRAENU0IYRAEQVDThBAGQRAENU0IYRAEQVDThBAGQRAENU0IYRAEQVDThBAGQRAENU0IYTfhjz/+SO+//3564IEH0vnnn58OPPDAtOGGG6Z55pknTT755Gn00UdP//nPf/JjhBFG+NejeG6MMcZIo402Wn5MP/30ackll0wDBgxIF110UXrxxRfTb7/9VvcXgyAIugchhF2IX3/9NT3xxBPp3HPPTZtvvnkad9xx03DDDZcfI488cpp55plTv3790u67754uvfTSLFxfffVV/r1//vmn7l3K5++//04//PBDeuutt9I555yTttxyyzT//POnscceO80444xp4403Th9//HHdq4MgCLomIYSdlJ9++inddtttaeutt64XPJ7bZJNNltZdd93soX333Xd1r25fiOqff/6ZhXauueZKyy67bDr55JPTN998U/eKIGgbDLiDDjoojT/++PXG3kwzzZTeeOONulcEQeUIIewkEBFiQlSEMUcZZZQ0xxxzpO222y59+umnda/qXBBEHuG+++6bFlxwwfTyyy+nv/76q1XeZxCIXDz99NPp6KOPTlNPPXW+plxbQ4cOzc8TwYUXXjgLJKEMgkoRQtiB8OgGDRqUVl555ZybY/12hZuct3rHHXdkkZZH3H///dNnn30WAhi0mu+//z5HOVZdddXUq1evkp7fBx98kNZff/203377pSFDhmThDIJKEELYAXz77bfp9NNPT/PNN1+adNJJO734Eb6bbropbbHFFtlSH2ussVKfPn3SMcccE5Z5UBEIHw9w1113Ta+99loOvTfm888/z0aXXLWf/j8IKkEIYTsjzMmTEvacffbZOyznwXtTAfrll1+mxx57LA0ePDgdcMABqX///vm4VI2OOeaYadRRR82e6i677JJuv/32LHyKaIKgErieGIIiC/LhrjGGVykIn9ced9xx6c4772zydUHQUkII2xnCp/LSTU+Aqt2OYLGwuOy5555Z5Aiw/GPDatNZZpklrbjiilnsbr311vTuu+/mdowgqCbuBZXOww8/fFmFMJ6PHGFQDUII2xk385xzzpkWWmihHN754osv6p4JgtpAQRXvjtElH9icsAmTykHfcsstZb0+CFpKCGEHUISDWMP77LNPeuihh9Ivv/xS92wQdE6KthmFLc8991wWMj2thxxySBo4cGDOIa+++uppvfXWS1tttVXaeeedc0/rTjvtlCMOSy+9dFpllVXSXnvtlZZbbrn6IQ/6Ug19EJmYddZZ63/Kn8tHTzvttGnCCSfMYXrP+X39tKIdEaYPKkEIYQfCOxQSEqIcb7zx8oJxzTXX5Ab2CE0G1YZnpm1HP6jWnW222SYts8wyaZJJJskCJE8sbOn6HGmkkdKUU06ZJxWpctbi4Fp9+OGH00svvZQ9PO/XHHLk2267bc5D9+zZs77lpincI8L5PEHphIsvvjiLr8pRYqrgzPSjCy+8MBfZ/P7773W/GQTlE0LYSWBlX3755WnHHXfMeRCLkAVIWwVLWjXdJZdc0mFN9EHXR8+naUCmAhE2AxommmiiLCY8OmLy+OOPVyU6UXiTRJeoGf9nYMTPP/9c94r/poiaGNhw2WWXDTNvTfyef/75dMIJJ6Qddtghph0FLSaEsJPjJn/99dfTDTfckOeHWrQmnnjiXOQiVGRRYcnfeOON6e233w5PMvgvGkYdhBq17fDg2ntmbCFsjmXvvfdODz74YJOC65hd55tsskm66667mhTLhjAQzz777Cz0hWcYfa1BuYQQdmGElEzdUH3Kol900UXTOOOMk0YcccQ0wQQT5JDSBhtskC1lw7jLWVCC7sWP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";

            Image sig = MTSUtilities.ImageUtilities.Serialization.ImageDecoding(img);
            Image resizeSig = null;
            ImageFormat format = MTSUtilities.ImageUtilities.ImageAttributes.GetImageFormat(sig);
            bool imgOversize = (sig.Height >= 75) || (sig.Width >= 450);

            if (imgOversize){
                resizeSig = MTSUtilities.ImageUtilities.ImageScaling.ScaleImage(sig, 300, 60);
            }

            reportOption.HospitalSignatureImage = iTextSharp.text.Image.GetInstance((resizeSig == null) ? sig : resizeSig, format);
            reportOption.RepresentativeSignatureImage = iTextSharp.text.Image.GetInstance((resizeSig == null) ? sig : resizeSig, format);
            PDfHelper.CreatePdfDoc(reportOption, imageDirectory);

            #endregion
        }
    }
}