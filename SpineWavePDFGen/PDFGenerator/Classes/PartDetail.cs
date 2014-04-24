using System;
using System.Collections.Generic;

namespace PDFGenChargesForm.Classes
{
    public class PartDetail
    {
        #region Accessors

        public Int32 Quantity { get; set; }

        public String CatalogNumber { get; set; }

        public String LotNumber { get; set; }

        public String Description { get; set; }

        public Double UnitPrice { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// To set Part Lot table details
        /// </summary>
        /// <returns>List<PartDetail></returns>
        public static List<PartDetail> GetPartDetails()
        {
            List<PartDetail> partList = new List<PartDetail>();

            PartDetail part = new PartDetail();
            part.LotNumber = "1";
            part.CatalogNumber = "1";
            part.Description = "ABCD";
            part.Quantity = 2;
            part.UnitPrice = 200.0;
            partList.Add(part);

            part = new PartDetail();
            part.LotNumber = "2";
            part.CatalogNumber = "2";
            part.Description = "ABCD";
            part.Quantity = 3;
            part.UnitPrice = 100.0;
            partList.Add(part);

            part = new PartDetail();
            part.LotNumber = "3";
            part.CatalogNumber = "3";
            part.Description = "ABCD";
            part.Quantity = 1;
            part.UnitPrice = 300.0;
            partList.Add(part);

            part = new PartDetail();
            part.LotNumber = "4";
            part.CatalogNumber = "4";
            part.Description = "ABCD";
            part.Quantity = 3;
            part.UnitPrice = 100.0;
            partList.Add(part);

            part = new PartDetail();
            part.LotNumber = "5";
            part.CatalogNumber = "5";
            part.Description = "ABCD";
            part.Quantity = 2;
            part.UnitPrice = 200.0;
            partList.Add(part);

            part = new PartDetail();
            part.LotNumber = "6";
            part.CatalogNumber = "6";
            part.Description = "ABCD";
            part.Quantity = 2;
            part.UnitPrice = 200.0;
            partList.Add(part);

            return partList;
        }

        #endregion
    }
}