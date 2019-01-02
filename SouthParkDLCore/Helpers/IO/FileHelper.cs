using System;
using System.Collections.Generic;
using System.Text;

namespace SouthParkDLCore.Helpers.IO
{
    class FileHelper
    {
        public static String EscapeFileName( String fileName )
        {
            StringBuilder sb = new StringBuilder();
            foreach (Char c in fileName)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        
    }
}
