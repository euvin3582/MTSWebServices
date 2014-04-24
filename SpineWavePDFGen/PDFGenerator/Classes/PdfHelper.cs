using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace PDFGenChargesForm.Classes
{
    /// <summary>
    /// Contains all the functions for pdf creation
    /// </summary>
    public class PDfHelper
    {
        #region Static Accessors

        private static string ImageDirectory { get; set; }

        private static iTextSharp.text.Font FontNormal = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, iTextSharp.text.Font.NORMAL);
        private static iTextSharp.text.Font FontBold = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, iTextSharp.text.Font.BOLD);
        private static iTextSharp.text.Font FontBoldBig = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 16, iTextSharp.text.Font.BOLD);
        private static iTextSharp.text.Font FontBoldItalic = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 10, iTextSharp.text.Font.BOLDITALIC);
        private static iTextSharp.text.Font FontItalic = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, iTextSharp.text.Font.BOLDITALIC);


        #endregion

        #region Methods

        /// <summary>
        /// To create a PDF using iTextSharp
        /// </summary>
        /// <param name="pView"></param>
        public static void CreatePdfDoc(ReportOptions pView, String imageDirectory)
        {
            Document pdfDoc = null;
            PdfWriter writer = null;
            FileStream fs = null;
            Double sumTotal = 0;
            try
            {
                ImageDirectory = imageDirectory;
                Image imageCheck = Image.GetInstance(ImageDirectory + "checkbox_check.png");
                Image imageUncheck = Image.GetInstance(ImageDirectory + "checkbox_uncheck.png");
                imageCheck.ScaleToFit(7.25f, 7.25f);
                imageUncheck.ScaleToFit(7.25f, 7.25f);

                String defaultFilePath = ConfigurationManager.AppSettings["PDFFilePath"];
                Guid guid = Guid.NewGuid();
                String fileName = guid.ToString() + ".pdf";

                defaultFilePath = defaultFilePath + fileName;
                fs = new FileStream(defaultFilePath, FileMode.Create);
                pdfDoc = new Document();
                pdfDoc.SetMargins(33f, 33f, 33f, 33f);
                writer = PdfWriter.GetInstance(pdfDoc, fs);
                pdfDoc.Open();

                pdfDoc.Add(AddHeader());//Add header table
                pdfDoc.Add(AddCustomerInfo(pView));//Add customer info table

                PdfPTable LineSeparator = GetPdfTable(1, false, 0, 100);
                LineSeparator.DefaultCell.BorderColor = BaseColor.WHITE;
                LineSeparator.SpacingAfter = 10f;
                LineSeparator.AddCell(new Paragraph(new Chunk(new LineSeparator(4f, 100f, BaseColor.GRAY, Element.ALIGN_CENTER, 0))));
                pdfDoc.Add(LineSeparator);

                pdfDoc.Add(AddCustomerDetails(pView));//Add customer details table

                pdfDoc.Add(new Paragraph(Constants.IMPLANT_TEXT, FontBoldItalic) { Alignment = Element.ALIGN_CENTER });

                pdfDoc.Add(AddSerialDetails(pView));//Add serial # details table

                pdfDoc.Add(AddPartGridTable(pView.PartLOTList, out sumTotal));/*Add middle grid table */

                pdfDoc.Add(AddTotalDetails(sumTotal)); //Add total price section

                pdfDoc.Add(AddConsumedLabel(pView));//Add consumed label table

                pdfDoc.Add(AddSignatureDetails(pView));//Add signature table

                Phrase phrase = new Phrase();
                phrase.Add(new Chunk(pView.DeliveryCheck == true ? imageCheck : imageUncheck, 0, 0));
                phrase.Add(new Chunk(Constants.DELIVERY_TEXT, FontBoldItalic));
                pdfDoc.Add(new Paragraph(phrase));

                pdfDoc.Add(AddReplenishmentDetails());//Add replineshment details table

                pdfDoc.Add(AddLoanInventoryInfo(pView));//Add loan inventory info table

                pdfDoc.Add(new Paragraph(Constants.FOOTER_FIRST_LINE, FontNormal) { Alignment = Element.ALIGN_LEFT });
                pdfDoc.Add(new Paragraph(Constants.FOOTER_SECOND_LINE + (pView.EffectiveDate == DateTime.MinValue ? "" : pView.EffectiveDate.ToString("MM/dd/yyyy")), FontNormal) { Alignment = Element.ALIGN_LEFT });
                pdfDoc.Add(new Paragraph(Constants.FOOTER_THIRD_LINE, FontNormal) { Alignment = Element.ALIGN_LEFT });

                pdfDoc.Close();
                fs.Close();
                fs.Dispose();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// To Create header image and header title
        /// </summary>
        /// <returns>HeaderTable</returns>
        private static PdfPTable AddHeader()
        {
            Image headerLogo = Image.GetInstance(ImageDirectory + "spinewave.png");
            headerLogo.ScaleToFit(125f, 100f);
            PdfPTable HeaderTable = GetPdfTable(2, false, 0, 100);
            HeaderTable.SetWidths(new float[] { 40f, 60f });

            PdfPCell LogoCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            LogoCell.AddElement(new Chunk(headerLogo, 0, 0));
            PdfPCell TitleCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            TitleCell.AddElement(new Paragraph(Constants.SURGERY_USAGE, FontBoldBig) { Alignment = Element.ALIGN_RIGHT });

            HeaderTable.AddCell(LogoCell);
            HeaderTable.AddCell(TitleCell);
            return HeaderTable;
        }

        /// <summary>
        /// To add customer information
        /// </summary>
        /// <returns>CustomerInfoTable</returns>
        private static PdfPTable AddCustomerInfo(ReportOptions pView)
        {
            PdfPTable CustomerInfoTable = GetPdfTable(2, false, 0, 100);

            CustomerInfoTable.AddCell(GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0));

            PdfPCell PhoneCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            PhoneCell.AddElement(new Paragraph(Constants.PHONE_NUMBER + pView.PhoneNo, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            CustomerInfoTable.AddCell(PhoneCell);

            CustomerInfoTable.AddCell(GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0));

            PdfPCell FaxCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            FaxCell.AddElement(new Paragraph(Constants.FAX + pView.Fax, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            CustomerInfoTable.AddCell(FaxCell);

            PdfPCell AddressCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            AddressCell.AddElement(new Paragraph(Constants.ADDRESS, FontNormal));
            CustomerInfoTable.AddCell(AddressCell);

            PdfPCell EmailCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            EmailCell.AddElement(new Paragraph(Constants.EMAIL + pView.Email, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            CustomerInfoTable.AddCell(EmailCell);

            CustomerInfoTable.AddCell(GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0));
            CustomerInfoTable.AddCell(GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0));

            return CustomerInfoTable;
        }

        /// <summary>
        /// To add customer details
        /// </summary>
        /// <param name="pView"></param>
        /// <returns>CustomerDetailsTable</returns>
        private static PdfPTable AddCustomerDetails(ReportOptions pView)
        {
            PdfPTable CustomerDetailsTable = GetPdfTable(1, false, 0, 100);
            CustomerDetailsTable.DefaultCell.BorderColor = BaseColor.WHITE;

            PdfPTable CustomerDetailsFirstTable = GetPdfTable(4, false, 0, 100);
            CustomerDetailsFirstTable.SetWidths(new int[4] { 10, 40, 20, 30 });
            CustomerDetailsFirstTable.SpacingAfter = 10f;


            PdfPCell BillToCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            BillToCell.AddElement(new Paragraph(Constants.BILL_TO, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            CustomerDetailsFirstTable.AddCell(BillToCell);

            CustomerDetailsFirstTable.AddCell(GetBorderedCell(pView.BillTo, FontNormal));

            PdfPCell SurgeryDateCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            SurgeryDateCell.BorderColorLeft = BaseColor.BLACK;
            SurgeryDateCell.BorderWidthLeft = 0.1f;
            SurgeryDateCell.AddElement(new Paragraph(Constants.SURGERY_DATE, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            CustomerDetailsFirstTable.AddCell(SurgeryDateCell);

            CustomerDetailsFirstTable.AddCell(GetBorderedCell(pView.SurgeryDate == DateTime.MinValue ? "" : pView.SurgeryDate.ToString("MM/dd/yyyy"), FontNormal));

            PdfPTable CustomerDetailsSecondTable = GetPdfTable(4, false, 0, 100);
            CustomerDetailsSecondTable.SetWidths(new int[4] { 10, 40, 20, 30 });
            CustomerDetailsSecondTable.SpacingAfter = 10f;

            PdfPCell CityStateCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            CityStateCell.AddElement(new Paragraph(Constants.CITY_STATE, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            CustomerDetailsSecondTable.AddCell(CityStateCell);

            CustomerDetailsSecondTable.AddCell(GetBorderedCell(pView.CityState, FontNormal));

            PdfPCell PhysicianNameCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            PhysicianNameCell.BorderColorLeft = BaseColor.BLACK;
            PhysicianNameCell.BorderWidthLeft = 0.1f;
            PhysicianNameCell.AddElement(new Paragraph(Constants.PHYSICIAN_NAME, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            CustomerDetailsSecondTable.AddCell(PhysicianNameCell);

            CustomerDetailsSecondTable.AddCell(GetBorderedCell(pView.PhysicianName, FontNormal));

            PdfPTable CustomerDetailsThirdTable = GetPdfTable(4, false, 0, 100);
            CustomerDetailsThirdTable.SetWidths(new int[4] { 10, 40, 20, 30 });
            CustomerDetailsThirdTable.SpacingAfter = 10f;

            PdfPCell PONumCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            PONumCell.AddElement(new Paragraph(Constants.PO_NUM, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            CustomerDetailsThirdTable.AddCell(PONumCell);

            CustomerDetailsThirdTable.AddCell(GetBorderedCell(pView.PONum, FontNormal));

            PdfPCell PurchasePhoneCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            PurchasePhoneCell.BorderColorLeft = BaseColor.BLACK;
            PurchasePhoneCell.BorderWidthLeft = 0.1f;
            PurchasePhoneCell.AddElement(new Paragraph(Constants.PURCHASE_PHONE, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            CustomerDetailsThirdTable.AddCell(PurchasePhoneCell);

            CustomerDetailsThirdTable.AddCell(GetBorderedCell(pView.PurchasePhone, FontNormal));

            CustomerDetailsTable.AddCell(CustomerDetailsFirstTable);
            CustomerDetailsTable.AddCell(CustomerDetailsSecondTable);
            CustomerDetailsTable.AddCell(CustomerDetailsThirdTable);

            return CustomerDetailsTable;
        }

        /// <summary>
        /// To add Serial details
        /// </summary>
        /// <param name="pView"></param>
        /// <returns>SerialDetailsTable</returns>
        private static PdfPTable AddSerialDetails(ReportOptions pView)
        {
            PdfPTable SerialDetailsTable = GetPdfTable(7, false, 0, 100);
            SerialDetailsTable.SetWidths(new int[7] { 10, 15, 15, 15, 15, 15, 15 });
            SerialDetailsTable.SpacingBefore = 10f;
            SerialDetailsTable.SpacingAfter = 10f;

            SerialDetailsTable.AddCell(GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0));
            SerialDetailsTable.AddCell(new PdfPCell(new Phrase(Constants.XD, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER, FixedHeight = 17f });
            SerialDetailsTable.AddCell(new PdfPCell(new Phrase(Constants.SNIPER, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            SerialDetailsTable.AddCell(new PdfPCell(new Phrase(Constants.PS, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            SerialDetailsTable.AddCell(new PdfPCell(new Phrase(Constants.MISS, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            SerialDetailsTable.AddCell(new PdfPCell(new Phrase(Constants.CROSS_CONN, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            SerialDetailsTable.AddCell(new PdfPCell(new Phrase(Constants.OTHER, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            PdfPCell SetSerialCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            SetSerialCell.AddElement(new Paragraph(Constants.SERIAL_NO, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            SerialDetailsTable.AddCell(SetSerialCell);

            SerialDetailsTable.AddCell(new PdfPCell((new Phrase(pView.XD, FontNormal))) { FixedHeight = 17f });
            SerialDetailsTable.AddCell(new Phrase(pView.Sniper, FontNormal));
            SerialDetailsTable.AddCell(new Phrase(pView.PS, FontNormal));
            SerialDetailsTable.AddCell(new Phrase(pView.Miss, FontNormal));
            SerialDetailsTable.AddCell(new Phrase(pView.CrossConn, FontNormal));
            SerialDetailsTable.AddCell(new Phrase(pView.Other, FontNormal));

            return SerialDetailsTable;
        }


        /// <summary>
        /// To create a dynamic table for unit items
        /// </summary>
        /// <param name="list<Part_Details>"</param>
        /// output <param name="sumTotal"></param>
        /// <returns>partGridTable</returns>
        /// </summary>
        private static PdfPTable AddPartGridTable(List<PartDetail> listPartDetails, out Double sumTotal)
        {
            sumTotal = 0;
            PdfPTable partGridTable = GetPdfTable(6, true, 530, 0);
            partGridTable.SetWidths(new float[] { 4f, 4f, 4f, 14f, 5f, 5f });

            //Table Here
            partGridTable.AddCell(new PdfPCell(new Phrase(Constants.QUANTITY, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            partGridTable.AddCell(new PdfPCell(new Phrase(Constants.CATALOG_NUMBER, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            partGridTable.AddCell(new PdfPCell(new Phrase(Constants.LOT_NUMBER, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            partGridTable.AddCell(new PdfPCell(new Phrase(Constants.DESCRIPTION, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            partGridTable.AddCell(new PdfPCell(new Phrase(Constants.UNIT_PRICE, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });
            partGridTable.AddCell(new PdfPCell(new Phrase(Constants.TOTAL_AMOUNT, FontBold)) { HorizontalAlignment = PdfPCell.ALIGN_CENTER });

            foreach (PartDetail p in listPartDetails)
            {
                partGridTable.AddCell(new PdfPCell(new Phrase(p.Quantity.ToString(), FontNormal)) { FixedHeight = 17f });
                partGridTable.AddCell(new Phrase(p.CatalogNumber, FontNormal));

                partGridTable.AddCell(new Phrase(p.LotNumber, FontNormal));
                partGridTable.AddCell(new Phrase(p.Description, FontNormal));
                partGridTable.AddCell(new Phrase("$" + p.UnitPrice.ToString(), FontNormal));
                partGridTable.AddCell(new Phrase("$" + (p.UnitPrice * p.Quantity), FontNormal));
                sumTotal += (p.UnitPrice * p.Quantity);
            }

            return partGridTable;
        }

        /// <summary>
        /// To create GetTotalDetails table
        /// </summary>
        /// <param name="sumTotal"></param>
        /// <returns>totaldetails</returns>
        private static PdfPTable AddTotalDetails(double sumTotal)
        {
            PdfPTable totaldetails = GetPdfTable(3, true, 530, 0);
            totaldetails.HorizontalAlignment = Rectangle.ALIGN_RIGHT;

            //Total Details Table start
            totaldetails.SetWidths(new float[] { 26f, 5f, 5f });

            totaldetails.AddCell(new PdfPCell(new Phrase(Constants.CONSUMED_IMPLANT_SERIALS, FontItalic)) { FixedHeight = 17f, BorderColor = BaseColor.WHITE, BorderColorTop = BaseColor.BLACK, BorderWidthTop = 0.1f });
            totaldetails.AddCell(new Phrase("Total:", FontBold));
            totaldetails.AddCell(new Phrase(new Chunk("$" + sumTotal.ToString(), FontNormal)));

            return totaldetails;
        }

        /// <summary>
        /// To add Consumed implant serial labels
        /// </summary>
        /// <returns>tableConsumedLabel</returns>
        private static PdfPTable AddConsumedLabel(ReportOptions pView)
        {
            PdfPTable tableConsumedLabel = GetPdfTable(1, true, 530, 0);

            tableConsumedLabel.AddCell(new PdfPCell(new Phrase(pView.ConsumedLabels, FontNormal)) { FixedHeight = 60f, BorderColorTop = BaseColor.GRAY, BorderWidthTop = 3f });

            return tableConsumedLabel;
        }

        /// <summary>
        /// To add Signature details
        /// </summary>
        /// <param name="pView"></param>
        /// <returns>SignatureTable</returns>
        private static PdfPTable AddSignatureDetails(ReportOptions pView)
        {
            PdfPTable SignatureTable = GetPdfTable(3, false, 0, 100);
            SignatureTable.SetWidths(new int[3] { 45, 5, 50 });
            SignatureTable.SpacingBefore = 10f;

            PdfPCell HospitalCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            HospitalCell.AddElement(new Paragraph(new Chunk(pView.HospitalSignature, FontNormal)) { Alignment = Rectangle.ALIGN_CENTER });
            HospitalCell.AddElement(new Paragraph(new Chunk(pView.HospitalSignatureImage, -30, -20)) { Alignment = Rectangle.ALIGN_LEFT });
            HospitalCell.AddElement(new Paragraph(new Chunk(new LineSeparator(1.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0))));
            HospitalCell.AddElement(new Paragraph(Constants.HOSPITAL_SIGNATURE, FontNormal) { Alignment = Rectangle.ALIGN_CENTER });

            SignatureTable.AddCell(HospitalCell);

            SignatureTable.AddCell(new PdfPCell(new Phrase(String.Empty)) { BorderColor = BaseColor.WHITE });

            PdfPCell RepresentativeCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            RepresentativeCell.AddElement(new Paragraph(new Chunk(pView.RepresentativeSignature, FontNormal)) { Alignment = Rectangle.ALIGN_CENTER });
            RepresentativeCell.AddElement(new Paragraph(new Chunk(pView.RepresentativeSignatureImage, -30, -20)) { Alignment = Rectangle.ALIGN_LEFT });
            RepresentativeCell.AddElement(new Paragraph(new Chunk(new LineSeparator(1.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0))));
            RepresentativeCell.AddElement(new Paragraph(Constants.REPRESENTATIVE_SIGNATURE, FontNormal) { Alignment = Rectangle.ALIGN_CENTER });
            SignatureTable.AddCell(RepresentativeCell);

            return SignatureTable;
        }

        /// <summary>
        /// To add Replenishment details
        /// </summary>
        /// <returns>ReplenishmentTable</returns>
        private static PdfPTable AddReplenishmentDetails()
        {
            PdfPTable ReplenishmentTable = GetPdfTable(2, false, 0, 100);
            ReplenishmentTable.SetWidths(new int[2] { 40, 60 });
            ReplenishmentTable.SpacingBefore = 10f;

            ReplenishmentTable.AddCell(new PdfPCell(new Phrase(Constants.REPLENISHMENT_NEEDED, FontItalic)) { FixedHeight = 22f, BorderColorTop = BaseColor.GRAY, BorderWidthTop = 3f, BorderColorRight = BaseColor.WHITE, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            ReplenishmentTable.AddCell(new PdfPCell(new Phrase(Constants.LOANER_INVENTORY, FontItalic)) { FixedHeight = 22f, BorderColorTop = BaseColor.GRAY, BorderWidthTop = 3f, BorderColorLeft = BaseColor.WHITE, BorderColorRight = BaseColor.WHITE, VerticalAlignment = Element.ALIGN_MIDDLE });

            return ReplenishmentTable;
        }

        /// <summary>
        /// To add Loan Inventory details
        /// </summary>
        /// <param name="pView"></param>
        /// <returns>LoanInventoryInfoTable</returns>
        private static PdfPTable AddLoanInventoryInfo(ReportOptions pView)
        {
            PdfPTable LoanInventoryInfoTable = GetPdfTable(4, false, 0, 100);
            LoanInventoryInfoTable.SetWidths(new int[4] { 10, 40, 20, 30 });
            LoanInventoryInfoTable.SpacingBefore = 10f;

            PdfPCell FacilityCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            FacilityCell.AddElement(new Paragraph(Constants.FACILITY, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            LoanInventoryInfoTable.AddCell(FacilityCell);

            LoanInventoryInfoTable.AddCell(GetBorderedCell(pView.Facility, FontNormal));

            PdfPCell CaseDateCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            CaseDateCell.BorderColorLeft = BaseColor.BLACK;
            CaseDateCell.BorderWidthLeft = 0.1f;
            CaseDateCell.AddElement(new Paragraph(Constants.CASE_DATE, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            LoanInventoryInfoTable.AddCell(CaseDateCell);

            LoanInventoryInfoTable.AddCell(GetBorderedCell(pView.CaseDateTime == DateTime.MinValue ? "" : pView.CaseDateTime.ToString("MM/dd/yyyy"), FontNormal));

            PdfPCell PhysicianCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            PhysicianCell.AddElement(new Paragraph(Constants.PHYSICIAN, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            LoanInventoryInfoTable.AddCell(PhysicianCell);

            LoanInventoryInfoTable.AddCell(GetBorderedCell(pView.Physician, FontNormal));

            PdfPCell CaseTimeCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            CaseTimeCell.BorderColorLeft = BaseColor.BLACK;
            CaseTimeCell.BorderWidthLeft = 0.1f;
            CaseTimeCell.AddElement(new Paragraph(Constants.CASE_TIME, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            LoanInventoryInfoTable.AddCell(CaseTimeCell);

            LoanInventoryInfoTable.AddCell(GetBorderedCell(pView.CaseDateTime == DateTime.MinValue ? "" : pView.CaseDateTime.ToString("h:mm tt"), FontNormal));

            PdfPCell ShipToCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            ShipToCell.Rowspan = 2;
            ShipToCell.AddElement(new Paragraph(Constants.SHIP_TO, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            LoanInventoryInfoTable.AddCell(ShipToCell);

            PdfPCell ShipToValueCell = GetPdfTableCell(Rectangle.BOX, BaseColor.BLACK, 0);
            ShipToValueCell.Rowspan = 2;
            ShipToValueCell.AddElement(new Paragraph(pView.ShipTo, FontNormal));
            LoanInventoryInfoTable.AddCell(ShipToValueCell);

            PdfPCell CaseTypeCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            CaseTypeCell.BorderColorLeft = BaseColor.BLACK;
            CaseTypeCell.BorderWidthLeft = 0.1f;
            CaseTypeCell.AddElement(new Paragraph(Constants.CASE_TYPE, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            LoanInventoryInfoTable.AddCell(CaseTypeCell);

            LoanInventoryInfoTable.AddCell(GetBorderedCell(pView.CaseType, FontNormal));

            PdfPCell LevelsCell = GetPdfTableCell(Rectangle.BOX, BaseColor.WHITE, 0);
            LevelsCell.BorderColorLeft = BaseColor.BLACK;
            LevelsCell.BorderWidthLeft = 0.1f;
            LevelsCell.AddElement(new Paragraph(Constants.LEVELS, FontNormal) { Alignment = Element.ALIGN_RIGHT });
            LoanInventoryInfoTable.AddCell(LevelsCell);

            LoanInventoryInfoTable.AddCell(GetBorderedCell(pView.Levels, FontNormal));

            return LoanInventoryInfoTable;

        }


        /// <summary>
        /// To add table cell with border
        /// </summary>
        /// <param name="value"></param>
        /// <param name="font"></param>
        /// <returns>PdfPCell</returns>
        private static PdfPCell GetBorderedCell(String value, Font font)
        {
            PdfPCell pdfPCell = new PdfPCell();
            pdfPCell.EnableBorderSide(Rectangle.RIGHT_BORDER);
            pdfPCell.AddElement(new Phrase(value, font));

            return pdfPCell;
        }


        /// <summary>
        /// To create a PDF table
        /// </summary>
        /// <param name="size"></param>
        /// <param name="lockedWidth"></param>
        /// <param name="totalWidth"></param>
        /// <param name="widthInPercentage"></param>
        /// <returns>PdfPTable</returns>
        private static PdfPTable GetPdfTable(int size, bool lockedWidth = false, float totalWidth = 0, float widthInPercentage = 0)
        {
            PdfPTable pdfPTable = new PdfPTable(size);
            pdfPTable.LockedWidth = lockedWidth;

            if (totalWidth != 0)
                pdfPTable.TotalWidth = totalWidth;

            if (widthInPercentage != 0)
                pdfPTable.WidthPercentage = widthInPercentage;

            return pdfPTable;
        }

        /// <summary>
        /// To create a PDF table cell
        /// </summary>
        /// <param name="border"></param>
        /// <param name="borderColor"></param>
        /// <param name="left"></param>
        /// <returns>PdfPCell</returns>
        private static PdfPCell GetPdfTableCell(int border, BaseColor borderColor, float left = 0)
        {
            PdfPCell cell = new PdfPCell();
            cell.Border = border;
            cell.BorderColor = borderColor;
            cell.PaddingLeft = left;

            return cell;
        }

        #endregion
    }
}