//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

using Landis.Core;
using Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;

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

        ActiveSite Landis.Library.BiomassCohorts.IDisturbance.CurrentSite
        {
            get
            {
                return currentSite;
            }
        }

        //---------------------------------------------------------------------

        ExtensionType IDisturbance.Type
        {
            get
            {
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

        int IDisturbance.ReduceOrKillMarkedCohort(ICohort cohort)
        {
            int biomassMortality = 0;
            double percentMortality = 0.0;
            int sppIndex = cohort.Species.Index;
            double cumulativeDefoliationManyInsects = 0.0;

            foreach (IInsect insect in PlugIn.ManyInsect)
            {

                if (!insect.ActiveOutbreak)
                    continue;

                int suscIndex = insect.SppTable[sppIndex].Susceptibility - 1;
                string thisInsect = insect.Name;
                int yearBack = 1;
                double annualDefoliation = 0.0;

                if (insect.HostDefoliationByYear[currentSite].ContainsKey(PlugIn.ModelCore.CurrentTime - yearBack) && insect.ThisYearDefoliation[currentSite] > 0) // Added 2nd constraint because "while" statement below didn't behave as intended when 2+ insects outbreaking at once.
                {
                    annualDefoliation = insect.HostDefoliationByYear[currentSite][PlugIn.ModelCore.CurrentTime - yearBack][suscIndex];
                    // PlugIn.ModelCore.UI.WriteLine("Host Defoliation By Year:  Time={0}, suscIndex={1}, spp={2}, annualDefoliation={3}.", (PlugIn.ModelCore.CurrentTime - yearBack), suscIndex + 1, cohort.Species.Name, annualDefoliation);
                }
                // Give cumulativeDefoliation initial value. Will be 0 if no other insects defoliated this cohort in this year or prior year. Otherwise, cumulativeDefoliation starts at the most recent level.
                double cumulativeDefoliation = cumulativeDefoliationManyInsects;
                double lastYearsCumulativeDefoliation = cumulativeDefoliationManyInsects;

                // For first insect in manyInsect, start tallying cumulative defoliation experienced by cohort over consecutive years.
                // If current cohort has not been defoliated by this or other insect in prior year, initialize for current outbreaks here.
                if (cumulativeDefoliation == 0.0)
                {
                    cumulativeDefoliation = annualDefoliation;
                    //lastYearsCumulativeDefoliation = annualDefoliation;
                    lastYearsCumulativeDefoliation = 0.0;
                }

                while (annualDefoliation > 0.0)
                {
                    yearBack++;
                    annualDefoliation = 0.0;
                    if (insect.HostDefoliationByYear[currentSite].ContainsKey(PlugIn.ModelCore.CurrentTime - yearBack))
                    {
                        annualDefoliation = insect.HostDefoliationByYear[currentSite][PlugIn.ModelCore.CurrentTime - yearBack][suscIndex];

                        if(annualDefoliation > 0.0)
                        {
                            PlugIn.ModelCore.UI.WriteLine("Host Defoliation By Year:  Time={0}, spp={2}, defoliation={3}, insect={4}.", (PlugIn.ModelCore.CurrentTime - yearBack), suscIndex+1, cohort.Species.Name, annualDefoliation, thisInsect);
                        }
                        // cumulativeDefoliation sums defoliation from previous consecutive years and across insects that are active at the same time.
                        cumulativeDefoliation += annualDefoliation;
                        PlugIn.ModelCore.UI.WriteLine("cumulativeDefoliation={0:0.000},annualDefoliation={1:0.000}, lastYearsCumulativeDefoliation {2:0.000}.", cumulativeDefoliation, annualDefoliation, lastYearsCumulativeDefoliation);
                        
                    }
                }
                // Update cumulativeDefoliationManyInsects
                cumulativeDefoliationManyInsects += cumulativeDefoliation;

                //PlugIn.ModelCore.UI.WriteLine("cumulativeDefoliation={0},annualDefoliation={1}, lastYearsCumulativeDefoliation {2}.", cumulativeDefoliation, annualDefoliation, lastYearsCumulativeDefoliation);

                double slope = insect.SppTable[sppIndex].MortalitySlope;
                double intercept = insect.SppTable[sppIndex].MortalityIntercept;
                double yearDefoliationDiff = cumulativeDefoliation - lastYearsCumulativeDefoliation;

                if (insect.AnnMort == "7Year")
                {
                    // **** Old Section ****
                    // Defoliation mortality doesn't start until at least 50% cumulative defoliation is reached.
                    // The first year of mortality follows normal background relationships...
                    if (cumulativeDefoliation >= 0.50 && lastYearsCumulativeDefoliation < 0.50)
                    {
                        //Most mortality studies restrospectively measure mortality for a number of years post disturbance. We need to subtract background mortality to get the yearly estimate. Subtract 7, assuming 1% mortality/year for 7 years, a typical time since disturbance in mortality papers. 
                        percentMortality = ((intercept) * (double)Math.Exp((slope * cumulativeDefoliation * 100)) - 7) / 100;
                        PlugIn.ModelCore.UI.WriteLine("cumulativeDefoliation={0}, cohort.Biomass={1}, percentMortality={2:0.00}.", cumulativeDefoliation, cohort.Biomass, percentMortality);
                    }

                    // Second year or more of defoliation mortality discounts the first year's mortality amount.
                    if (cumulativeDefoliation >= 0.50 && lastYearsCumulativeDefoliation >= 0.50 && cumulativeDefoliation != lastYearsCumulativeDefoliation)
                    {
                        double lastYearPercentMortality = ((intercept) * (double)Math.Exp((slope * lastYearsCumulativeDefoliation * 100)) - 7) / 100;
                        percentMortality = ((intercept) * (double)Math.Exp((slope * cumulativeDefoliation * 100)) - 7) / 100;                        
                        percentMortality -= lastYearPercentMortality;
                        PlugIn.ModelCore.UI.WriteLine(" {0}, cumulativeDefoliation={1:0.000}, cohort.Biomass={2:0.000}, percentMortality={3:0.00}.", thisInsect, cumulativeDefoliation, cohort.Biomass, percentMortality);
                    }

                    // Special case for when you have only one year of defoliation that is >50%, so no discounting necessary. There is probably a better way to write this.
                    if (cumulativeDefoliation >= 0.50 && lastYearsCumulativeDefoliation >= 0.50 && yearDefoliationDiff < 0.00000000000000000001)
                    {
                        percentMortality = ((intercept) * (double)Math.Exp((slope * cumulativeDefoliation * 100)) - 7) / 100;
                    }
                    // **** End Old Section ****
                }
                else if (insect.AnnMort == "Annual") // Need to update this Annual method to discount cumulative defoliation from other insects in THIS year.
                {
                    // **** New section from JRF ****
                    // Defoliation mortality doesn't start until at least 50% cumulative defoliation is reached.
                    // The first year of mortality follows normal background relationships...
                    if (cumulativeDefoliation >= 0.50)
                    {
                        //Most mortality studies restrospectively measure mortality for a number of years post disturbance. This model requires annualized mortality relationships and parameters, and will not work correctly with longer-term relationships. An earlier version subtracted background mortality from such relationships to get the yearly estimate.

                        //percentMortality = ((intercept) * (double)Math.Exp((slope * cumulativeDefoliation * 100))) / 100;
                        percentMortality = (double)Math.Exp(slope * cumulativeDefoliation * 100 + intercept) / 100;
                        // PlugIn.ModelCore.UI.WriteLine("cumulativeDefoliation={0}, cohort.Biomass={1}, percentMortality={2:0.00}.", cumulativeDefoliation, cohort.Biomass, percentMortality);
                    }
                    // **** End new section from JRF ****
                }
                else
                {
                    throw new System.ApplicationException("Error: Mortality parameter is not Annual or 7Year");     
                }

                if (percentMortality > 0.0)
                {
                    biomassMortality += (int) ((double) cohort.Biomass * percentMortality);
                    // PlugIn.ModelCore.UI.WriteLine("biomassMortality={0}, cohort.Biomass={1}, percentMortality={2:0.00}.", biomassMortality, cohort.Biomass, percentMortality);

                }


            }  // end insect loop

            if (biomassMortality > cohort.Biomass)
                biomassMortality = cohort.Biomass;

            SiteVars.BiomassRemoved[currentSite] += biomassMortality;
            // PlugIn.ModelCore.UI.WriteLine("biomassMortality={0}, BiomassRemoved={1}.", biomassMortality, SiteVars.BiomassRemoved[currentSite]);

            if(biomassMortality > cohort.Biomass || biomassMortality < 0)
            {
                 PlugIn.ModelCore.UI.WriteLine("Cohort Total Mortality={0}. Cohort Biomass={1}. Site R/C={2}/{3}.", biomassMortality, cohort.Biomass, currentSite.Location.Row, currentSite.Location.Column);
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
            SiteVars.Cohorts[site].ReduceOrKillBiomassCohorts(singleton);
        }
    }
}
