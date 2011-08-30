//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System.IO;
using System;


namespace Landis.Extension.Insects
{
    ///<summary>
    /// A disturbance plug-in that simulates Biological Agents.
    /// </summary>

    public class PlugIn
        : ExtensionMain
    {
        public static readonly ExtensionType Type = new ExtensionType("disturbance:insects");
        public static readonly string ExtensionName = "Biomass Insects";

        private string mapNameTemplate;
        private StreamWriter log;
        private static List<IInsect> manyInsect;
        private IInputParameters parameters;
        private static ICore modelCore;
        private bool running;

        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, Type)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }

        //---------------------------------------------------------------------
        public static List<IInsect> ManyInsect
        {
            get {
                return manyInsect;
            }
        }
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            SiteVars.Initialize();
            InputParameterParser parser = new InputParameterParser();
            parameters = mCore.Load<IInputParameters>(dataFile, parser);

            // Add local event handler for cohorts death due to age-only
            // disturbances.
            Cohort.AgeOnlyDeathEvent += CohortKilledByAgeOnlyDisturbance;

        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Initializes the extension with a data file.
        /// </summary>
        public override void Initialize()
        {
            Timestep = parameters.Timestep;

            Timestep = 1; //parameters.Timestep;
            mapNameTemplate = parameters.MapNamesTemplate;
            manyInsect = parameters.ManyInsect;

            SiteVars.Initialize();
            Defoliate.Initialize(parameters);
            GrowthReduction.Initialize(parameters);

            if (Landis.Extension.Succession.Biomass.PlugIn.SuccessionTimeStep != 1)
                PlugIn.ModelCore.Log.WriteLine("  CAUTION!  If using Biomass Insects, Biomass Succession should be operating at an ANNUAL time step.");

            foreach(IInsect insect in manyInsect)
            {

                if(insect == null)
                    PlugIn.ModelCore.Log.WriteLine("  Caution!  Insect Parameters NOT loading correctly.");

                insect.Neighbors = GetNeighborhood(insect.NeighborhoodDistance);

                int i=0;

                foreach(RelativeLocation location in insect.Neighbors)
                    i++;

                if(insect.Neighbors != null)
                    PlugIn.ModelCore.Log.WriteLine("   Biomass Insects:  Dispersal Neighborhood = {0} neighbors.", i);

            }


            PlugIn.ModelCore.Log.WriteLine("   Opening BiomassInsect log file \"{0}\" ...", parameters.LogFileName);
            try {
                log = PlugIn.ModelCore.CreateTextFile(parameters.LogFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }

            log.AutoFlush = true;
            log.Write("Time,InsectName,StartYear,StopYear,MeanDefoliation,NumSitesDefoliated0_33,NumSitesDefoliated33_66,NumSitesDefoliated66_100,NumOutbreakInitialSites");
            //foreach (IEcoregion ecoregion in Ecoregions.Dataset)
            //      log.Write(",{0}", ecoregion.MapCode);
            log.WriteLine("");

        }

        //---------------------------------------------------------------------
        ///<summary>
        /// Run the BDA extension at a particular timestep.
        ///</summary>
        public override void Run()
        {

            running = true;
            PlugIn.ModelCore.Log.WriteLine("   Processing landscape for Biomass Insect events ...");

            foreach(IInsect insect in manyInsect)
            {
                SiteVars.BiomassRemoved.ActiveSiteValues = 0;
                SiteVars.InitialOutbreakProb.ActiveSiteValues = 0.0;

                if(insect.MortalityYear == PlugIn.ModelCore.CurrentTime)
                    Outbreak.Mortality(insect);

                // Copy the data from current to last
                foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                    insect.LastYearDefoliation[site] = insect.ThisYearDefoliation[site];

                insect.ActiveOutbreak = false;

                PlugIn.ModelCore.NormalDistribution.Mu = 0.0;
                PlugIn.ModelCore.NormalDistribution.Sigma = 1.0;
                double randomNum = PlugIn.ModelCore.NormalDistribution.NextDouble();

                PlugIn.ModelCore.ExponentialDistribution.Lambda = insect.MeanDuration;      // rate
                double randomNumE = PlugIn.ModelCore.ExponentialDistribution.NextDouble();

                // First, has enough time passed since the last outbreak?
                double timeBetweenOutbreaks = insect.MeanTimeBetweenOutbreaks + (insect.StdDevTimeBetweenOutbreaks * randomNum);
                double duration = System.Math.Round(randomNumE + 1);
                double timeAfterDuration = timeBetweenOutbreaks - duration;

                //PlugIn.ModelCore.Log.WriteLine("Calculated time between = {0}.  inputMeanTime={1}, inputStdTime={2}.", timeBetweenOutbreaks, insect.MeanTimeBetweenOutbreaks, insect.StdDevTimeBetweenOutbreaks);
                //PlugIn.ModelCore.Log.WriteLine("Calculated duration     = {0}.  inputMeanDura={1}, inputStdDura={2}.", duration, insect.MeanDuration, insect.StdDevDuration);
                //PlugIn.ModelCore.Log.WriteLine("Insect Start Time = {0}, Stop Time = {1}.", insect.OutbreakStartYear, insect.OutbreakStopYear);


                if(PlugIn.ModelCore.CurrentTime == 1)
                {
                    //PlugIn.ModelCore.Log.WriteLine("   Year 1:  Setting initial start and stop times.");
                    insect.OutbreakStartYear = (int) (timeBetweenOutbreaks / 2.0) + 1;
                    insect.OutbreakStopYear  = insect.OutbreakStartYear + (int) duration - 1;
                    PlugIn.ModelCore.Log.WriteLine("   {0} is not active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, PlugIn.ModelCore.CurrentTime);
                }
                else if(insect.OutbreakStartYear <= PlugIn.ModelCore.CurrentTime
                    && insect.OutbreakStopYear > PlugIn.ModelCore.CurrentTime)
                {
                    //PlugIn.ModelCore.Log.WriteLine("   An outbreak starts or continues.  Start and stop time do not change.");
                    insect.ActiveOutbreak = true;
                    PlugIn.ModelCore.Log.WriteLine("   {0} is active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, PlugIn.ModelCore.CurrentTime);

                    insect.MortalityYear = PlugIn.ModelCore.CurrentTime + 1;

                }
                else if(insect.OutbreakStopYear <= PlugIn.ModelCore.CurrentTime
                    && timeAfterDuration > PlugIn.ModelCore.CurrentTime - insect.OutbreakStopYear)
                {
                    //PlugIn.ModelCore.Log.WriteLine("   In between outbreaks, reset start and stop times.");
                    insect.ActiveOutbreak = true;
                    PlugIn.ModelCore.Log.WriteLine("   {0} is active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, PlugIn.ModelCore.CurrentTime);

                    insect.MortalityYear = PlugIn.ModelCore.CurrentTime + 1;
                    insect.OutbreakStartYear = PlugIn.ModelCore.CurrentTime + (int) timeBetweenOutbreaks;
                    insect.OutbreakStopYear = insect.OutbreakStartYear + (int) duration;
                }
                //PlugIn.ModelCore.Log.WriteLine("  Insect Start Time = {0}, Stop Time = {1}.", insect.OutbreakStartYear, insect.OutbreakStopYear);

                if(insect.ActiveOutbreak)
                {
                    //PlugIn.ModelCore.Log.WriteLine("   OutbreakStartYear={0}.", insect.OutbreakStartYear);

                    if(insect.OutbreakStartYear == PlugIn.ModelCore.CurrentTime)
                        // Initialize neighborhoodGrowthReduction with patches
                        Outbreak.InitializeDefoliationPatches(insect);
                    else
                        insect.NeighborhoodDefoliation.ActiveSiteValues = 0;

                }

                // Now report on the previous year's defoliation, that which has been processed
                // through biomass succession.

                double sumDefoliation = 0.0;
                int numSites0_33 = 0;
                int numSites33_66 = 0;
                int numSites66_100 = 0;
                int numInitialSites = 0;
                foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                {
                        sumDefoliation += insect.LastYearDefoliation[site];
                        if (insect.LastYearDefoliation[site] > 0.0 && insect.LastYearDefoliation[site] <= 33.0)
                            numSites0_33++;
                        if (insect.LastYearDefoliation[site] > 33.0 && insect.LastYearDefoliation[site] <= 66.0)
                            numSites33_66++;
                        if (insect.LastYearDefoliation[site] > 66.0 && insect.LastYearDefoliation[site] <= 100.0)
                            numSites66_100++;
                        if (insect.Disturbed[site] && SiteVars.InitialOutbreakProb[site] > 0)
                            numInitialSites++;
                }

                double meanDefoliation = 0.0;
                if (numSites0_33 + numSites33_66 + numSites66_100 > 0)
                    meanDefoliation = sumDefoliation / (double) (numSites0_33 + numSites33_66 + numSites66_100);
                //PlugIn.ModelCore.Log.WriteLine("   sumDefoliation={0}, numSites={1}.", sumDefoliation, numSites);

                log.Write("{0},{1},{2},{3},{4:0.00},{5},{6},{7},{8}",
                        PlugIn.ModelCore.CurrentTime-1,  //0
                        insect.Name,  //1
                        insect.OutbreakStartYear,  //2
                        insect.OutbreakStopYear,  //3
                        meanDefoliation, //4
                        numSites0_33, //5
                        numSites33_66,  //6
                        numSites66_100, //7
                        numInitialSites //8
                        );

                    //foreach (IEcoregion ecoregion in Ecoregions.Dataset)
                    //    log.Write(",{0}", 1);

                log.WriteLine("");

                //----- Write Insect GrowthReduction maps --------
                string path = MapNames.ReplaceTemplateVars(mapNameTemplate, insect.Name, PlugIn.ModelCore.CurrentTime - 1);
                using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path, modelCore.Landscape.Dimensions))
                {
                    ShortPixel pixel = outputRaster.BufferPixel;

                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites) {
                        if (site.IsActive)
                            pixel.MapCode.Value = (short) (insect.LastYearDefoliation[site] * 100.0);
                        else
                            //  Inactive site
                            pixel.MapCode.Value = 0;

                        outputRaster.WriteBufferPixel();
                    }
                }

                //----- Write Initial Patch maps --------
                string path2 = MapNames.ReplaceTemplateVars(mapNameTemplate, ("InitialPatchMap-" + insect.Name), PlugIn.ModelCore.CurrentTime - 1);
                using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path2, modelCore.Landscape.Dimensions))
                {
                    ShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites) {
                        if (site.IsActive)
                        {
                            if (insect.Disturbed[site])
                                pixel.MapCode.Value = (short)(SiteVars.InitialOutbreakProb[site] * 100);
                            else
                                pixel.MapCode.Value = 0;
                        }
                        else
                        {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }

                //----- Write Biomass Reduction maps --------
                string path3 = MapNames.ReplaceTemplateVars(mapNameTemplate, ("BiomassRemoved-" + insect.Name), PlugIn.ModelCore.CurrentTime - 1);
                using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path3, modelCore.Landscape.Dimensions))
                {
                    ShortPixel pixel = outputRaster.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        if (site.IsActive)
                        {
                            pixel.MapCode.Value = (short) (SiteVars.BiomassRemoved[site] / 100);  // convert to Mg/ha
                        }
                        else
                        {
                            //  Inactive site
                            pixel.MapCode.Value = 0;
                        }
                        outputRaster.WriteBufferPixel();
                    }
                }
                
                insect.ThisYearDefoliation.ActiveSiteValues = 0.0;  //reset this year to 0 for all sites
            
            }


        }

        //---------------------------------------------------------------------

        // Event handler when a cohort is killed by an age-only disturbance.
        public void CohortKilledByAgeOnlyDisturbance(object                 sender,
                                                     DeathEventArgs eventArgs)
        {
            // If this plug-in is not running, then some base disturbance
            // plug-in killed the cohort.
            if (! running)
                return;

            SiteVars.BiomassRemoved[eventArgs.Site] += eventArgs.Cohort.Biomass;
        }
        //---------------------------------------------------------------------
        private void LogEvent(int   currentTime)
        {
            log.Write("{0}", currentTime);
            log.WriteLine("");
        }

        //---------------------------------------------------------------------
        // Generate a Relative RelativeLocation array of neighbors.
        // Check each cell within a circle surrounding the center point.  This will
        // create a set of POTENTIAL neighbors.  These potential neighbors
        // will need to be later checked to ensure that they are within the landscape
        // and active.

        private static IEnumerable<RelativeLocation> GetNeighborhood(int neighborhoodDistance)
        {
            double CellLength = PlugIn.ModelCore.CellLength;
            PlugIn.ModelCore.Log.WriteLine("   Creating Dispersal Neighborhood List.");

            List<RelativeLocation> neighborhood = new List<RelativeLocation>();

                int neighborRadius = neighborhoodDistance;
                int numCellRadius = (int) (neighborRadius / CellLength);
                PlugIn.ModelCore.Log.WriteLine("   Insect:  NeighborRadius={0}, CellLength={1}, numCellRadius={2}",
                        neighborRadius, CellLength, numCellRadius);
                double centroidDistance = 0;
                double cellLength = CellLength;

                for(int row=(numCellRadius * -1); row<=numCellRadius; row++)
                {
                    for(int col=(numCellRadius * -1); col<=numCellRadius; col++)
                    {
                        centroidDistance = DistanceFromCenter(row, col);

                        //PlugIn.ModelCore.Log.WriteLine("Centroid Distance = {0}.", centroidDistance);
                        if(centroidDistance  <= neighborRadius)
                            //if(row!=0 || col!=0)
                                neighborhood.Add(new RelativeLocation(row,  col));
                    }
                }

            return neighborhood;
        }

        //-------------------------------------------------------
        //Calculate the distance from a location to a center
        //point (row and column = 0).
        private static double DistanceFromCenter(double row, double column)
        {
            double CellLength = PlugIn.ModelCore.CellLength;
            row = System.Math.Abs(row) * CellLength;
            column = System.Math.Abs(column) * CellLength;
            double aSq = System.Math.Pow(column,2);
            double bSq = System.Math.Pow(row,2);
            return System.Math.Sqrt(aSq + bSq);
        }
    }

}
