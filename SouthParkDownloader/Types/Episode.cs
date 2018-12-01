using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SouthParkDownloader.Types
{
  class Episode
  {
    public UInt16 Season { get; set; }
    public UInt16 Number { get; set; }
    public String Address { get; set; }
    public String Name { get; set; }

    public String FileName { get {
                return 'S'+Season + '-' + 'E'+Number + ' ' + Name.Replace('\'', ' ').Replace('"', ' ').Replace("  ", " ");
    } }
  }
}
