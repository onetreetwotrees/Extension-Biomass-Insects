//  Copyright 2006 University of Wisconsin, Conservation Biology Institute
//  Authors:
//      Jane Foster
//      Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Biomass;
using Landis.Landscape;
using Landis.PlugIns;
using System.Collections.Generic;
using System;

namespace Landis.Extension.Insects
{
    /// <summary>
    /// A biomass disturbance that handles partial thinning of cohorts.
    /// </summary>
    public class PartialDisturbance
        : IDisturbance
    {
        private static PartialDisturbance singleton;

        private static ActiveSite currentSite;

        //---------------------------------------------------------------------

        ActiveSite IDisturbance.CurrentSite
        {
            get {
                return currentSite;
            }
        }
        //---------------------------------------------------------------------

        PlugInType IDisturbance.Type
        {
            get {
                return PlugIn.Type;
            }
        }

        //---------------------------------------------------------------------

        static PartialDisturbance()
        {
            singleton = new PartialDisturbance();
        }

        //---------------------------------------------------------------------

        public PartialDisturbance()
        {
        }

        //---------------------------------------------------------------------

        int IDisturbance.Damage(ICohort cohort)
        {
            int biomassMortality = 0;
            double percentMortality = 0.0;
            int sppIndex = cohort.Species.Index;

            foreach (IInsect insect in PlugIn.ManyInsect)
            {
                if (!insect.ActiveOutbreak)
                    continue;

                int suscIndex = insect.SppTable[sppIndex].Susceptibility - 1;

                int yearBack = 1;
                double annualDefoliation = 0.0;

                if (insect.HostDefoliationByYear[currentSite].ContainsKey(Model.Core.CurrentTime - yearBack))
                {
                    //UI.WriteLine("Host Defoliation By Year:  Time={0}, suscIndex={1}, spp={2}.", (Model.Core.CurrentTime - yearBack), suscIndex+1, cohort.Species.Name);
                    annualDefoliation = insect.HostDefoliationByYear[currentSite][Model.Core.CurrentTime - yearBack][suscIndex];
                    //UI.WriteLine("Test");
                }
                double cumulativeDefoliation = annualDefoliation;
                double lastYearsCumulativeDefoliation = annualDefoliation;

                 while (annualDefoliation > 0.0)
                {
                    yearBack++;
                    annualDefoliation = 0.0;
                    if (insect.HostDefoliationByYear[currentSite].ContainsKey(Model.Core.CurrentTime - yearBack))
                    {
                        annualDefoliation = insect.HostDefoliationByYear[currentSite][Model.Core.CurrentTime - yearBack][suscIndex];

                        if(annualDefoliation > 0.0)
                        {
                            //UI.WriteLine("Host Defoliation By Year:  Time={0}, spp={2}, defoliation={3:0.00}.", (Model.Core.CurrentTime - yearBack), suscIndex+1, cohort.Species.Name, annualDefoliation);
                        }

                        cumulativeDefoliation += annualDefoliation;
                    }
                }

                //UI.WriteLine("cumulativeDefoliation={0},annualDefoliation={1}.", cumulativeDefoliation,annualDefoliation);

                double slope = insect.SppTable[sppIndex].MortalitySlope;
                double intercept = insect.SppTable[sppIndex].MortalityIntercept;

                //percentMortality = (cumulativeDefoliation * slope + intercept);
                //double lastYearsCumulativeDefoliation = cumulativeDefoliation - annualDefoliation;
                double lastYearPercentMortality = ((intercept) * (double)Math.Exp((slope * lastYearsCumulativeDefoliation * 100))) / 100;
                percentMortality = ((intercept) * (double) Math.Exp((slope * cumulativeDefoliation * 100)))/100;
                       //UI.WriteLine("cumulativeDefoliation={0}, cohort.Biomass={1}, percentMortality={2:0.00}.", cumulativeDefoliation, cohort.Biomass, percentMortality);
                percentMortality -= lastYearPercentMortality;

                if (percentMortality > 0.0)
                {
                    biomassMortality += (int)((double) cohort.Biomass * percentMortality);
                    //UI.WriteLine("biomassMortality={0}, cohort.Biomass={1}, percentMortality={2:0.00}.", biomassMortality, cohort.Biomass, percentMortality);

                }
            }  // end insect loop

            if (biomassMortality > cohort.Biomass)
                biomassMortality = cohort.Biomass;

            SiteVars.BiomassRemoved[currentSite] += biomassMortality;
            //UI.WriteLine("biomassMortality={0}, BiomassRemoved={1}.", biomassMortality, SiteVars.BiomassRemoved[currentSite]);

            if(biomassMortality > cohort.Biomass || biomassMortality < 0)
            {
                UI.WriteLine("Cohort Total Mortality={0}. Cohort Biomass={1}. Site R/C={2}/{3}.", biomassMortality, cohort.Biomass, currentSite.Location.Row, currentSite.Location.Column);
                throw new System.ApplicationException("Error: Total Mortality is not between 0 and cohort biomass");
            }

            return biomassMortality;

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Reduces the biomass of cohorts that have been marked for partial
        /// reduction.
        /// </summary>
        public static void ReduceCohortBiomass(ActiveSite site)
        {
            currentSite = site;
            Model.LandscapeCohorts[site].DamageBy(singleton);
        }
    }
}
