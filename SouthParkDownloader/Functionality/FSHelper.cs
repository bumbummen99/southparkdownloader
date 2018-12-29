using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SouthParkDownloader.Functionality
{
    class FSHelper
    {
        public static void CleanDirectory(String directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        public static String RemoveSpecialCharacters(String str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Char c in str)
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
