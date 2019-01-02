using System;
using System.IO;

namespace SouthParkDLCore.Helpers.IO
{
    public class DirectoryHelper
    {
        public static void DeleteContents(String directory, Boolean fast = true )
        {
            if (fast)
            {
                Directory.Delete(directory, true);
                Directory.CreateDirectory(directory);
            }
            else
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
        }
    }
}
