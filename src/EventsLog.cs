using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Landis.Library.Metadata;
using Landis.Core;

namespace Landis.Extension.Insects
{
    public class EventsLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Time")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Insect Name")]
        public string InsectName { set; get; }

        [DataFieldAttribute(Desc = "Start Year")]
        public int StartYear { set; get; }

        [DataFieldAttribute(Desc = "Stop Year")]
        public int StopYear { set; get; }

        [DataFieldAttribute(Desc = "Mean Defoliation")]
        public double MeanDefoliation { set; get; }

        [DataFieldAttribute(Desc = "Number Sites Defoliated 0_33")]
        public int NumSites0_33 { set; get; }

        [DataFieldAttribute(Desc = "Number Sites Defoliated 33_66")]
        public int NumSites33_66 { set; get; }

        [DataFieldAttribute(Desc = "Number Sites Defoliated 66_100")]
        public int NumSites66_100 { set; get; }

        [DataFieldAttribute(Desc = "Number Outbreak Initial Sites")]
        public int InitialSites { set; get; }

        [DataFieldAttribute(Desc = "Mortality Biomass")]
        public int LastBiomassRemoved { set; get; }
    }
}
