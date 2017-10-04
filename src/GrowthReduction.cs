//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;
using System.Collections.Generic;
using System;


namespace Landis.Extension.Insects
{
    public class GrowthReduction
    {

        private static IEnumerable<IInsect> manyInsect;
        //---------------------------------------------------------------------

        public static void Initialize(IInputParameters parameters)
        {
            manyInsect = parameters.ManyInsect;

            // Assign the method below to the CohortGrowthReduction delegate in
            // biomass-cohorts/Biomass.CohortGrowthReduction.cs
            CohortGrowthReduction.Compute = ReduceCohortGrowth;

        }


        //---------------------------------------------------------------------
        // This method replaces the delegate method.  It is called every year when
        // ACT_ANPP is calculated, for each cohort.  Therefore, this method is operating at
        // an ANNUAL time step and separate from the normal extension time step.

        public static double ReduceCohortGrowth(ICohort cohort, ActiveSite site)//, int siteBiomass)
        {
            // PlugIn.ModelCore.UI.WriteLine("   Calculating cohort growth reduction due to insect defoliation...");

            double summaryGrowthReduction = 0.0;
            int sppIndex = cohort.Species.Index;
            bool currentActiveOutbreak = false;

            // New loop, first loop through all insects to find out if any of them have an active outbreak.
            foreach (IInsect insect in PlugIn.ManyInsect)
            {
                if (!(currentActiveOutbreak) && insect.ActiveOutbreak)
                {
                    currentActiveOutbreak = true;
                    //PlugIn.ModelCore.UI.WriteLine(" At least one active outbreak reducing growth, insect = {0}, startYear = {1}, stopYear = {2}.", insect.Name, insect.LastStartYear, insect.LastStopYear);
                }

            }

            // Only check insect histories for current defoliation if there is at least one current active outbreak. Otherwise there is no current defoliation to reduce growth.
            if (currentActiveOutbreak)
            {

                foreach (IInsect insect in PlugIn.ManyInsect)
                {
                    //if (!insect.ActiveOutbreak)
                    //    continue;
                    /*if (insect.HostDefoliationByYear[site].Count == 0)
                    {
                        PlugIn.ModelCore.UI.WriteLine("Checking for prior defoliation:  Time={0}, spp={1}, insect={2}, dictionaryCount={3}.", (PlugIn.ModelCore.CurrentTime), cohort.Species.Name, insect.Name, insect.HostDefoliationByYear[site].Count);
                    }
                    // Check for any prior defoliation on cohort by this insect. If below dictionary is empty, the code will throw an error, so don't proceed. Also stop if growth is already reduced by 100%.
                    if (insect.HostDefoliationByYear[site].Count > 0 && summaryGrowthReduction < 1)
                        continue;*/

                    int suscIndex = insect.SppTable[sppIndex].Susceptibility - 1;
                    //if (suscIndex < 0) suscIndex = 0;

                    int yearBack = 0;
                    double annualDefoliation = 0.0;

                    if (insect.HostDefoliationByYear[site].ContainsKey(PlugIn.ModelCore.CurrentTime - yearBack))
                    {
                        // PlugIn.ModelCore.UI.WriteLine("Host Defoliation By Year:  Time={0}, suscIndex={1}, spp={2}.", (PlugIn.ModelCore.CurrentTime - yearBack), suscIndex+1, cohort.Species.Name);
                        //annualDefoliation += insect.HostDefoliationByYear[site][PlugIn.ModelCore.CurrentTime - yearBack][suscIndex];
                        annualDefoliation = insect.HostDefoliationByYear[site][PlugIn.ModelCore.CurrentTime - yearBack][suscIndex];
                        //PlugIn.ModelCore.UI.WriteLine("1st Host Defoliation By Year:  Time={0}, suscIndex={1}, spp={2}, annualDefoliation={3:0.000000}.", (PlugIn.ModelCore.CurrentTime - yearBack), suscIndex + 1, cohort.Species.Name, annualDefoliation);
                    }

                    double cumulativeDefoliation = annualDefoliation;
                    //PlugIn.ModelCore.UI.WriteLine("1st Cumulative Defoliation by insect:  Time={0}, cumulativeDefoliation={1:0.000000}, spp={2}, insect={3}, annualDefoliation={4:0.000000}.", (PlugIn.ModelCore.CurrentTime - yearBack), cumulativeDefoliation, cohort.Species.Name, insect.Name, annualDefoliation);

                    while (annualDefoliation > 0)
                    {
                        yearBack++;
                        annualDefoliation = 0.0;
                        if (insect.HostDefoliationByYear[site].ContainsKey(PlugIn.ModelCore.CurrentTime - yearBack))
                        {
                            // PlugIn.ModelCore.UI.WriteLine("Host Defoliation By Year:  Time={0}, suscIndex={1}, spp={2}.", (PlugIn.ModelCore.CurrentTime - yearBack), suscIndex+1, cohort.Species.Name);
                            annualDefoliation = insect.HostDefoliationByYear[site][PlugIn.ModelCore.CurrentTime - yearBack][suscIndex];
                            cumulativeDefoliation += annualDefoliation;
                            //PlugIn.ModelCore.UI.WriteLine("Additional Cumulative Defoliation by insect:  Time={0}, cumulativeDefoliation={1:0.000000}, spp={2}, insect={3} annualDefoliation={4:0.000000}.", (PlugIn.ModelCore.CurrentTime - yearBack), cumulativeDefoliation, cohort.Species.Name, insect.Name, annualDefoliation);

                        }
                    }

                    if (cumulativeDefoliation <= (0.0001 * (yearBack + 1)))
                    {
                        cumulativeDefoliation = 0.0;
                    }

                    double slope = insect.SppTable[sppIndex].GrowthReduceSlope;
                    double intercept = insect.SppTable[sppIndex].GrowthReduceIntercept;

                    double growthReduction = 1.0 - (cumulativeDefoliation * slope + intercept);
                    // Sum total growth reduction caused by multiple insects in this year. This is returned and applied to ANPP in Succession Extension.
                    summaryGrowthReduction += growthReduction;
                    // PlugIn.ModelCore.UI.WriteLine("Time={0}, Spp={1}, SummaryGrowthReduction={2:0.00}.", PlugIn.ModelCore.CurrentTime,cohort.Species.Name, summaryGrowthReduction);
                    //PlugIn.ModelCore.UI.WriteLine("Time={0}, Spp={1}, SummaryGrowthReduction={2:0.00}, cumulativeDefoliation={3:0.000000}, insect={4}, Site R/C={5}/{6}.", PlugIn.ModelCore.CurrentTime, cohort.Species.Name, summaryGrowthReduction, cumulativeDefoliation, insect.Name, site.Location.Row, site.Location.Column);

                } // end loop over insects
            }
            if (summaryGrowthReduction > 1.0)  // Cannot exceed 100%
                summaryGrowthReduction = 1.0;

            if(summaryGrowthReduction > 1.0 || summaryGrowthReduction < 0)
            {
                 PlugIn.ModelCore.UI.WriteLine("Cohort Total Growth Reduction = {0:0.00}.  Site R/C={1}/{2}.", summaryGrowthReduction, site.Location.Row, site.Location.Column);
                throw new ApplicationException("Error: Total Growth Reduction is not between 1.0 and 0.0");
            }

            return summaryGrowthReduction;
        }



    }

}
