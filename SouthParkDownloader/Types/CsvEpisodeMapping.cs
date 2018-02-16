﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

namespace SouthParkDownloader.Types
{
  class CsvEpisodeMapping : CsvMapping<Episode>
  {
    public CsvEpisodeMapping() : base()
    {
      MapProperty( 0, x => x.Season );
      MapProperty( 1, x => x.Number );
      MapProperty( 2, x => x.Address );
      MapProperty( 3, x => x.Name );
    }
  }
}

