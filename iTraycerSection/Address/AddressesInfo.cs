using DataLayer.domains;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace iTraycerSection.Address
{
    public class AddressesInfo
    {
        public static DataTable GetAddressByLatLong(XmlDocument payloadChild)
        {
            XmlNode Latitude = payloadChild.SelectSingleNode("//Latitude");
            XmlNode Longitude = payloadChild.SelectSingleNode("//Longitude");

            if (Latitude == null && Longitude == null)
            {
                return null;
            } else
                return DataLayer.Controller.GetClosesAddressByLatLong(String.IsNullOrEmpty(Latitude.InnerText) ?
                                                                        null : (Decimal?)Convert.ToDecimal(Latitude.InnerText), 
                                                                      String.IsNullOrEmpty(Longitude.InnerText) ?
                                                                        null : (Decimal?)Convert.ToDecimal(Longitude.InnerText));
        }
    }
}
