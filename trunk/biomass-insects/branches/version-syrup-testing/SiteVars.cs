//  Copyright 2006 University of Wisconsin
//  Authors:  
//      Jane Foster
//      Robert M. Scheller
//  Version 1.0
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Biomass;
using Landis.Landscape;
using System.Collections.Generic;

namespace Landis.Insects
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
        //private static ISiteVar<double> summaryLastDefoliation;
        
        //growthReduction by year:  year and (int) percentage (0-100) that can later be translated into a fraction
        //private static ISiteVar<Dictionary<int,double>> defoliationByYear;  


        //---------------------------------------------------------------------

        public static void Initialize()
        {
            outbreakVariables       = Model.Core.Landscape.NewSiteVar<Outbreak>();
            timeOfLastEvent         = Model.Core.Landscape.NewSiteVar<int>();
            biomassRemoved = Model.Core.Landscape.NewSiteVar<int>();
            //neighborhoodDefoliation = Model.Core.Landscape.NewSiteVar<double>();
            initialOutbreakProb     = Model.Core.Landscape.NewSiteVar<double>();
            //sumLastDefoliation      = Model.Core.Landscape.NewSiteVar<double>();
            //defoliationByYear   = Model.Core.Landscape.NewSiteVar<Dictionary<int,double>>();

            //Model.Core.RegisterSiteVar(SiteVars.DefoliationByYear, "Insect.GrowthReduction");
            
            //SiteVars.NeighborhoodDefoliation.ActiveSiteValues = 0.0;
            SiteVars.TimeOfLastEvent.ActiveSiteValues = -10000;
            SiteVars.InitialOutbreakProb.ActiveSiteValues = 0.0;
            //SiteVars.SumLastDefoliation.ActiveSiteValues = 0.0;
            //SiteVars.Disturbed.ActiveSiteValues = false;
            
            //Initialize outbreaks:
            foreach (ActiveSite site in Model.Core.Landscape) 
            {
                SiteVars.OutbreakVars = null; //new Outbreak();
                //SiteVars.DefoliationByYear[site] = new Dictionary<int, double>();
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
        /*public static ISiteVar<double> SumLastDefoliation
        {
            get {
                return sumLastDefoliation;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Dictionary<int,double>> DefoliationByYear
        {
            get
            {
                return defoliationByYear;
            }
            set
            {
                defoliationByYear = value;
            }
        }*/

        //---------------------------------------------------------------------
        public static ISiteVar<int> TimeOfLastEvent
        {
            get {
                return timeOfLastEvent;
            }
        }
        //---------------------------------------------------------------------
        /*
        public static ISiteVar<bool> Disturbed
        {
            get {
                return disturbed;
            }
        }*/
    }
}
