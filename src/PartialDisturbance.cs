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
            int insectIndex = 0;
            double activeInsectCurrentDefoliation = 0.0;
            //double priorInsectDefoliationThisYear = 0.0;
            double lastYearsCumulativeDefoliation = 0.0;
            double cumulativeDefoliation = 0.0;
            int activeInsectStartYear = 0;
            int activeInsectStopYear = 0;
            int tempInsectIndex = 0;

            // New loop, first loop through all insects to find Outbreak start and stop years of current active Insect.
            foreach (IInsect insect in PlugIn.ManyInsect)
            {
                tempInsectIndex++;
                if (tempInsectIndex == PlugIn.activeInsectIndex)
                {
                    activeInsectStartYear = insect.LastStartYear;
                    activeInsectStopYear = insect.LastStopYear;
                    //PlugIn.ModelCore.UI.WriteLine(" Found active outbreak insectIndex = {0}, startYear = {1}, stopYear = {2}.", tempInsectIndex, activeInsectStartYear, activeInsectStopYear);
                }
            }

            foreach (IInsect insect in PlugIn.ManyInsect)
            {
                insectIndex++;
                //PlugIn.ModelCore.UI.WriteLine(" At top of PartialDisturbance loop running insectIndex = {0}.", insectIndex);

                // This statement below stops the model from considering other insect defoliation once that insects outbreak has ended.
                // This means the last year of an outbreak of an earlier insect in the model order is not considered, even though it defoliated
                // cohort in the same prior year under consideration. Try changing to get this working correctly. This will also allow
                // Proper use of insectIndex++ to constrain calculation of mortality til all insects are considered every time...
                /*if (!insect.ActiveOutbreak)
                    continue;*/
              
                int suscIndex = insect.SppTable[sppIndex].Susceptibility - 1;
                int yearBack = 1;
                double annualDefoliation = 0.0;
                //int thisInsectIndex = 0;
                string thisInsect = insect.Name;

                //PlugIn.ModelCore.UI.WriteLine(" {0}  Adding up defoliation on cohort for mortality caused by Insect#{1}, Site R/C={2}/{3}.", thisInsect, PlugIn.activeInsectIndex, currentSite.Location.Row, currentSite.Location.Column);

                // First, check one year back for defoliation on cohort from this insect in manyInsect loop.
                if (insect.HostDefoliationByYear[currentSite].ContainsKey(PlugIn.ModelCore.CurrentTime - yearBack))
                {
                    //thisInsectIndex++;
                    annualDefoliation = insect.HostDefoliationByYear[currentSite][PlugIn.ModelCore.CurrentTime - yearBack][suscIndex];
                    //PlugIn.ModelCore.UI.WriteLine("{0} 1st Host Defoliation By Year:  Time={1}, spp={2}, annualDefoliation={3:0.00000}, cumulativeDefoliationManyInsects={4:0.00000}.", thisInsect, (PlugIn.ModelCore.CurrentTime - yearBack), cohort.Species.Name, annualDefoliation,cumulativeDefoliationManyInsects);
                }
                // If no defoliation one year back, and insect is not in active outbreak, but last outbreak overlaps with activeInsect, 
                // check the most recent year of overlap for defoliation on this cohort...
                else if (insect.LastStopYear >= activeInsectStartYear && !insect.ActiveOutbreak && insectIndex != PlugIn.activeInsectIndex)
                {
                    // Update yearBack to start at the most recent year overlapping outbreak of activeInsect
                    yearBack = PlugIn.ModelCore.CurrentTime - insect.LastStopYear;
                    if (insect.HostDefoliationByYear[currentSite].ContainsKey(PlugIn.ModelCore.CurrentTime - yearBack))
                    {
                        //PlugIn.ModelCore.UI.WriteLine("Special Case {0} 1st Host Defoliation By Year:  Time={1}, spp={2}, annualDefoliation={3:0.00000}, cumulativeDefoliationManyInsects={4:0.00000}.", thisInsect, (PlugIn.ModelCore.CurrentTime - yearBack), cohort.Species.Name, annualDefoliation, cumulativeDefoliationManyInsects);
                        annualDefoliation = insect.HostDefoliationByYear[currentSite][PlugIn.ModelCore.CurrentTime - yearBack][suscIndex];
                    }
                }

                //PlugIn.ModelCore.UI.WriteLine(" {0}  insectIndex={1}, activeInsectIndex={2}.", thisInsect, insectIndex, PlugIn.activeInsectIndex);

                /*
                if (annualDefoliation == 0)
                {
                    thisInsectIndex = insectIndex;
                }
  
                // Give cumulativeDefoliation initial value or update to most recent value. Will be 0 if no other insects defoliated this cohort in prior year, or earlier in the same year. Otherwise, cumulativeDefoliation starts at the most recent level.
                if (cumulativeDefoliationManyInsects > 0)
                {
                    cumulativeDefoliation = cumulativeDefoliationManyInsects;
                }*/

                // For first insect in manyInsect, start tallying cumulative defoliation experienced by cohort over consecutive years and many insects up to current insect.
                /*if (cumulativeDefoliation == 0.0 && insectIndex == 1)
                {
                    cumulativeDefoliation = annualDefoliation;
                    lastYearsCumulativeDefoliation = 0.0;
                    //PlugIn.ModelCore.UI.WriteLine(" {0}, Yr 1 cumulativeDefoliation={1:0.00000},annualDefoliation={2:0.00000}, lastYearsCumulativeDefoliation {3:0.00000}.", thisInsect, cumulativeDefoliation, annualDefoliation, lastYearsCumulativeDefoliation);
                }*/

                if (insectIndex == PlugIn.activeInsectIndex)
                {
                    activeInsectCurrentDefoliation = annualDefoliation;
                    // Only calculate current year cumulative defoliation over previous year for current active insect.
                    cumulativeDefoliation += activeInsectCurrentDefoliation;
                    //lastYearsCumulativeDefoliation = lastYearsCumulativeDefoliation - activeInsectCurrentDefoliation;
                }


                // For cohorts that were defoliated 1 or more years prior by one or more insects, accumulate that defoliation here...
                while (annualDefoliation > 0.0)
                {
                    // If current cohort was defoliated by this or any other insect in prior year, earlier in the spring, acrue for current outbreaks here. 
                    // Only acrue defoliation from 1 year prior if it was caused by insects before current Insect was active within the same year, in order specified in Insect Setup File.
                    /*if (insectIndex <= PlugIn.activeInsectIndex && yearBack == 1 && thisInsectIndex < insectIndex)
                    {
                        thisInsectIndex++;
                        priorInsectDefoliationThisYear += annualDefoliation;
                        //PlugIn.ModelCore.UI.WriteLine(" {0}, priorInsectDefoliationThisYear={1:0.00000},annualDefoliation={2:0.00000}, lastYearsCumulativeDefoliation {3:0.00000}.", thisInsect, priorInsectDefoliationThisYear, annualDefoliation, lastYearsCumulativeDefoliation);

                    }
                    else
                    {
                        yearBack++;
                    }*/
                    // Go back to original formulation to try to simplify again
                    yearBack++;
                    annualDefoliation = 0.0;
                    if (insect.HostDefoliationByYear[currentSite].ContainsKey(PlugIn.ModelCore.CurrentTime - yearBack))
                    {
                        annualDefoliation = insect.HostDefoliationByYear[currentSite][PlugIn.ModelCore.CurrentTime - yearBack][suscIndex];

                        if(annualDefoliation > 0.0)
                        {
                            cumulativeDefoliation += annualDefoliation;
                            lastYearsCumulativeDefoliation += annualDefoliation;
                            //lastYearsCumulativeDefoliation = cumulativeDefoliation - activeInsectCurrentDefoliation;
                            //PlugIn.ModelCore.UI.WriteLine("{0} While Host Defoliation By Year:  Time={1}, spp={2}, annualDefoliation={3:0.00000}.", thisInsect, (PlugIn.ModelCore.CurrentTime - yearBack), cohort.Species.Name, annualDefoliation);
                        }
                    }
                    // cumulativeDefoliation sums defoliation from previous consecutive years and across insects that are active at the same time.
                    if (annualDefoliation > 0.0)
                    {
                        //PlugIn.ModelCore.UI.WriteLine("cumulativeDefoliation={0:0.00000},annualDefoliation={1:0.00000}, lastYearsCumulativeDefoliation {2:0.00000}.", cumulativeDefoliation, annualDefoliation, lastYearsCumulativeDefoliation);
                    }
                }
                // Update cumulativeDefoliationManyInsects - cumulativeDefoliationManyInsects may not be needed anymore
                //cumulativeDefoliationManyInsects = cumulativeDefoliation;

                //PlugIn.ModelCore.UI.WriteLine("Endwhile cumulativeDefoliation={0},annualDefoliation={1}, lastYearsCumulativeDefoliation {2}.", cumulativeDefoliation, annualDefoliation, lastYearsCumulativeDefoliation);

                double slope = insect.SppTable[sppIndex].MortalitySlope;
                double intercept = insect.SppTable[sppIndex].MortalityIntercept;

                if (insect.AnnMort == "7Year")
                {
                    // **** Old Section ****
                    // Defoliation mortality doesn't start until at least 50% cumulative defoliation is reached.
                    // The first year of mortality follows normal background relationships...
                    if (cumulativeDefoliation >= 0.50 && activeInsectCurrentDefoliation > 0.01 && insectIndex == PlugIn.ManyInsect.Count)
                    {
                        if (lastYearsCumulativeDefoliation < 0.50)
                        {
                            //Most mortality studies restrospectively measure mortality for a number of years post disturbance. We need to subtract background mortality to get the yearly estimate. Subtract 7, assuming 1% mortality/year for 7 years, a typical time since disturbance in mortality papers. 
                            percentMortality = ((intercept) * (double)Math.Exp((slope * cumulativeDefoliation * 100)) - 7) / 100;
                            //PlugIn.ModelCore.UI.WriteLine(" {0}, if1 cumulativeDefoliation={1:0.00000}, cohort.Biomass={2}, percentMortality={3:0.00000}, cumDefoManyInsects={4:0.00000}.", thisInsect, cumulativeDefoliation, cohort.Biomass, percentMortality, cumulativeDefoliationManyInsects);
                        }

                        // Second year or more of defoliation mortality discounts the first year's mortality amount.
                        if (lastYearsCumulativeDefoliation >= 0.50 && !insect.SingleOutbreakYear)
                        {
                            double lastYearPercentMortality = ((intercept) * (double)Math.Exp((slope * lastYearsCumulativeDefoliation * 100)) - 7) / 100;
                            percentMortality = ((intercept) * (double)Math.Exp((slope * cumulativeDefoliation * 100)) - 7) / 100;
                            percentMortality -= lastYearPercentMortality;
                            //PlugIn.ModelCore.UI.WriteLine(" {0}, if2 cumulativeDefoliation={1:0.00000}, cohort.Biomass={2}, percentMortality={3:0.0000}, cumDefoManyInsects={4:0.00000}.", thisInsect, cumulativeDefoliation, cohort.Biomass, percentMortality, cumulativeDefoliationManyInsects);
                        }

                        // Special case for when you have only one year of defoliation that is >50%, so no discounting necessary. There is probably a better way to write this.
                        if (activeInsectCurrentDefoliation >= 0.50 && insect.SingleOutbreakYear)
                        {

                            percentMortality = ((intercept) * (double)Math.Exp((slope * cumulativeDefoliation * 100)) - 7) / 100;
                            //PlugIn.ModelCore.UI.WriteLine(" {0}, if3 cumulativeDefoliation={1:0.00000}, cohort.Biomass={2}, percentMortality={3:0.0000}, cumDefoManyInsects={4:0.00000}.", thisInsect, cumulativeDefoliation, cohort.Biomass, percentMortality, cumulativeDefoliationManyInsects);

                        }
                        // **** End Old Section ****
                    }
                }

                else if (insect.AnnMort == "Annual") // Need to update this Annual method to discount cumulative defoliation from other insects in THIS year.
                {
                    //double priorPercentMortalityThisYear = 0.0;
                    // **** New section from JRF ****
                    // Defoliation mortality doesn't start until at least 50% cumulative defoliation is reached, and isn't calculated until loop has cycled through potential defoliation of all insects.
                    // Mortality when cumulative defoliation is <50% follows normal background relationships...No mortality will acrue for defoliation < 1%.
                    if (cumulativeDefoliation >= 0.50 && activeInsectCurrentDefoliation > 0.01 && insectIndex == PlugIn.ManyInsect.Count)
                    {
                        //Most mortality studies restrospectively measure mortality for a number of years post disturbance. 
                        //This model requires annualized mortality relationships and parameters, and will not work correctly 
                        //with longer-term relationships. An earlier version subtracted background mortality from such relationships 
                        //to get the yearly estimate.

                        //Calculate subannual increment of mortality that occurred from prior insect activity in this year. 
                        //Use this to discount mortality of subsequent insects.
                        /*if (lastYearsCumulativeDefoliation >= 0.50 && priorInsectDefoliationThisYear > 0.0)
                        {
                            priorPercentMortalityThisYear = ((double)Math.Exp(slope * (lastYearsCumulativeDefoliation) * 100 + intercept) / 100);
                            percentMortality = Math.Max(0, ((double)Math.Exp(slope * cumulativeDefoliation * 100 + intercept) / 100) - priorPercentMortalityThisYear);
                            //PlugIn.ModelCore.UI.WriteLine(" {0}, if4 priorPercentMortalityThisYear={1:0.00000},annualDefoliation={2:0.00000}, lastYearsCumulativeDefoliation {3:0.00000}.", thisInsect, priorPercentMortalityThisYear, annualDefoliation, lastYearsCumulativeDefoliation);

                        }
                        else
                        {*/
                            //percentMortality = (double)Math.Exp(slope * cumulativeDefoliation * 100 + intercept) / 100;
                            //BRM - Subtract the background mortality from the curve (mortality rate for 0 cumulative defoliation)
                            double pctMort0 = (double)Math.Exp(slope * 0 * 100 + intercept) / 100;
                            percentMortality = Math.Max(0, ((double)Math.Exp(slope * cumulativeDefoliation * 100 + intercept) / 100) - pctMort0);
                            //PlugIn.ModelCore.UI.WriteLine("if5 cumulativeDefoliation={0}, cohort.Biomass={1}, percentMortality={2:0.00}.", cumulativeDefoliation, cohort.Biomass, percentMortality);
                        //}
                    }
                    // **** End new section from JRF ****
                }
                else
                {
                    throw new System.ApplicationException("Error: Mortality parameter is not Annual or 7Year");
                }

                if (percentMortality > 0.0)
                {
                    // Calculate how much biomass is lost to this percent mortality
                    //biomassMortality += (int) ((double) cohort.Biomass * percentMortality);
                    //To get to correct cumulative defoliation, code has to loop through all active insects in any time step. Set equal to the final biomassMortality computed in the loop.
                    biomassMortality = (int)((double)cohort.Biomass * percentMortality);
                    //PlugIn.ModelCore.UI.WriteLine(" In insect loop, biomassMortality={0}, cohort.Biomass={1}, percentMortality={2:0.0000}, cumulativeDefoliation={3:0.00000}.", biomassMortality, cohort.Biomass, percentMortality,cumulativeDefoliation);

                }


            }  // end insect loop

            if (biomassMortality > cohort.Biomass)
                biomassMortality = cohort.Biomass;

            SiteVars.BiomassRemoved[currentSite] += biomassMortality;

            if (biomassMortality > 0)
            {
                //PlugIn.ModelCore.UI.WriteLine(" biomassMortality={0}, BiomassRemoved={1}.", biomassMortality, SiteVars.BiomassRemoved[currentSite]);
            }

            if(biomassMortality > cohort.Biomass || biomassMortality < 0)
            {
                 //PlugIn.ModelCore.UI.WriteLine("Cohort Total Mortality={0}. Cohort Biomass={1}. Site R/C={2}/{3}.", biomassMortality, cohort.Biomass, currentSite.Location.Row, currentSite.Location.Column);
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
