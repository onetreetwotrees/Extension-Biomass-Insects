//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

using Landis.Extension.Succession.Biomass;
using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;
using System.Collections.Generic;

namespace Landis.Extension.Insects
{
    ///<summary>
    /// Site Variables for a disturbance plug-in that simulates Biological Agents.
    /// </summary>
    public static class SiteVars
    {
        private static ISiteVar<Outbreak> outbreakVariables;
        private static ISiteVar<int> timeOfLastEvent;
        private static ISiteVar<int> biomassRemoved;
        //private static ISiteVar<double> neighborhoodDefoliation;
        private static ISiteVar<double> initialOutbreakProb;
        private static ISiteVar<ISiteCohorts> cohorts;
        private static ISiteVar<int> cohortsPartiallyDamaged;


        //---------------------------------------------------------------------

        public static void Initialize()
        {
            outbreakVariables       = PlugIn.ModelCore.Landscape.NewSiteVar<Outbreak>();
            timeOfLastEvent         = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            biomassRemoved          = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            //neighborhoodDefoliation = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            initialOutbreakProb     = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            cohorts                 = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.BiomassCohorts");
            cohortsPartiallyDamaged = PlugIn.ModelCore.Landscape.NewSiteVar<int>();

            //SiteVars.NeighborhoodDefoliation.ActiveSiteValues = 0.0;
            SiteVars.TimeOfLastEvent.ActiveSiteValues = -10000;
            SiteVars.InitialOutbreakProb.ActiveSiteValues = 0.0;

            //Initialize outbreaks:
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                SiteVars.OutbreakVars = null; //new Outbreak();
            }

            if (cohorts == null)
            {
                string mesg = string.Format("Cohorts are empty.  Please double-check that this extension is compatible with your chosen succession extension.");
                throw new System.ApplicationException(mesg);
            }


        }
        //---------------------------------------------------------------------

        public static ISiteVar<Outbreak> OutbreakVars
        {
            get {
                return outbreakVariables;
            }
            set {
                outbreakVariables = value;
            }

        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> BiomassRemoved
        {
            get {
                return biomassRemoved;
            }
            set
            {
                biomassRemoved = value;
            }
        }
        //---------------------------------------------------------------------
        /*public static ISiteVar<double> NeighborhoodDefoliation
        {
            get {
                return neighborhoodDefoliation;
            }
        }*/

        //---------------------------------------------------------------------
        public static ISiteVar<double> InitialOutbreakProb
        {
            get {
                return initialOutbreakProb;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<int> CohortsPartiallyDamaged
        {
            get
            {
                return cohortsPartiallyDamaged;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<int> TimeOfLastEvent
        {
            get {
                return timeOfLastEvent;
            }
        }
    }
}
