using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Insects
{
    public class EventsLog
    {
        //log.Write("Time,InsectName,StartYear,StopYear,MeanDefoliation,NumSitesDefoliated0_33,NumSitesDefoliated33_66,NumSitesDefoliated66_100,NumOutbreakInitialSites,MortalityBiomass");

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time {set; get;}

        [DataFieldAttribute(Desc = "Insect Name")]
        public string InsectName { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Start Year")]
        public int StartYear { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Stop Year")]
        public int StopYear { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Percentage, Desc = "Average Defoliation", Format = "0.00")]
        public double MeanDefoliation { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Sites Defoliated 0-33%")]
        public int NumSitesDefoliated0_33 { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Sites Defoliated 33-66%")]
        public int NumSitesDefoliated33_66 { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Sites Defoliated 66-100%")]
        public int NumSitesDefoliated66_100 { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Outbreak Initiation Sites")]
        public int NumOutbreakInitialSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.g_B_m2, Desc = "Biomass lost due to insect mortality", Format="0.00")]
        public double MortalityBiomass { set; get; }

    }
}
