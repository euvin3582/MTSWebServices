using System;

namespace PDFGenChargesForm.Classes
{
    public class Constants
    {
        #region Constant strings

        //Header Left and Right Table Section
        public static String SURGERY_USAGE = "SURGERY USAGE CHARGE FORM";
        public static String ADDRESS = "3 ENTERPRISE DRIVE, SUITE 210 SHELTON, CT 06484";
        public static String PHONE_NUMBER = "CUSTOMER SERVICE: ";
        public static String FAX = "FAX: ";
        public static String EMAIL = "EMAIL: ";
        public static String BILL_TO = "BILL TO\t:\t";
        public static String SURGERY_DATE = "SURGERY DATE\t:\t";
        public static String CITY_STATE = "CITY, STATE\t:\t";
        public static String PHYSICIAN_NAME = "PHYSICIAN NAME\t:\t";
        public static String PO_NUM = "P.O. NUM\t:\t";
        public static String PURCHASE_PHONE = "PURCHASE PHONE\t:\t";
        public static String IMPLANT_TEXT = "YOU MUST INDICATE THE IMPLANT SET SERIAL# FOR EACH SET USED IN THE CASE.";

        //Serial # details
        public static String XD = "XD";
        public static String SNIPER = "SNIPER (BOTH)";
        public static String PS = "PS (BOTH)";
        public static String MISS = "MISS (BOTH)";
        public static String CROSS_CONN = "CROSS CONN";
        public static String OTHER = "OTHER";
        public static String SERIAL_NO = "SET SERIAL#:";

        //Part Lot Number table labels
        public static String LOT_NUMBER = "LOT NO";
        public static String DESCRIPTION = "DESCRIPTION";
        public static String CATALOG_NUMBER = "CATALOG NO";
        public static String QUANTITY = "QUANTITY";
        public static String UNIT_PRICE = "UNIT PRICE";
        public static String TOTAL_AMOUNT = "TOTAL AMOUNT";

        public static String CONSUMED_IMPLANT_SERIALS = "PLACE ALL CONSUMED IMPLANT SERIAL# LABELS BELOW";

        //Signature table details
        public static String HOSPITAL_SIGNATURE = "AUTHORIZED HOSPITAL SIGNATURE/TITLE (REQUIRED)";
        public static String REPRESENTATIVE_SIGNATURE = "SPINE WAVE REPRESENTATIVE NAME / SIGNATURE";

        //Delivery details
        public static String DELIVERY_TEXT = "\t\tI accept delivery for the above listed items";
        public static String REPLENISHMENT_NEEDED = "Y__ N__ IS REPLENISHMENT NEEDED?";
        public static String LOANER_INVENTORY = "IF LOANER INVENTORY WAS USED, COMPLETE ALL INFORMATION BELOW.";

        //Footer table
        public static String FACILITY = "FACILITY\t:\t";
        public static String CASE_DATE = "CASE DATE\t:\t";
        public static String PHYSICIAN = "PHYSICIAN\t:\t";
        public static String CASE_TIME = "CASE TIME\t:\t";
        public static String SHIP_TO = "SHIP TO\t:\t";
        public static String CASE_TYPE = "CASE TYPE\t:\t";
        public static String LEVELS = "LEVELS\t:\t";
        public static String FOOTER_FIRST_LINE = "19-1012 Rev A.";
        public static String FOOTER_SECOND_LINE = "Effective Date:\t";
        public static String FOOTER_THIRD_LINE = "Approved Per ECO P12-222. Signatures on file.";

        #endregion
    }
}