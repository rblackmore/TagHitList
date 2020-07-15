using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ecom.TagHitList
{
    public static class ASCIIHexCOnvert
    {
        public static string ConvertHexToAsciiString(String hexString)
        {
            if (hexString.Length % 2 != 0)
                return "Hex Value length must be a multiple of 2";

            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    String hs = String.Empty;

                    hs = hexString.Substring(i, 2);
                    uint decval = System.Convert.ToUInt32(hs, 16);
                    char character = System.Convert.ToChar(decval);
                    ascii += character;
                }

                return ascii;
            }
            catch (Exception ex)
            {
                return "Error Converting Hex to ASCII";
            }
        }

        public static string ConvertASCIItoHexString(String asciiString)
        {
            char[] charValues = asciiString.ToCharArray();
            string returnValue = String.Empty;

            foreach (char item in charValues)
            {
                int value = Convert.ToInt32(item);
                returnValue += String.Format("{0:X}", value);
            }

            return returnValue;
        }
    }
}
