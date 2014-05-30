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
                Session.Session.errorMessage = "A Latitude or Longitude was not provided";
            } else
                return DataLayer.Controller.GetClosesAddressByLatLong(Convert.ToDecimal(Latitude.InnerText), Convert.ToDecimal(Longitude.InnerText));

            return null;
        }
    }
}
