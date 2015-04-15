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
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);

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

            // ARJAN
            //if (Landis.Extension.Succession.Biomass.PlugIn.SuccessionTimeStep != 1)
            //     PlugIn.ModelCore.UI.WriteLine("  CAUTION!  If using Biomass Insects, Biomass Succession should be operating at an ANNUAL time step.");

            foreach(IInsect insect in manyInsect)
            {

                if(insect == null)
                     PlugIn.ModelCore.UI.WriteLine("  Caution!  Insect Parameters NOT loading correctly.");

                insect.Neighbors = GetNeighborhood(insect.NeighborhoodDistance);

                int i=0;

                foreach(RelativeLocation location in insect.Neighbors)
                    i++;

                //if(insect.Neighbors != null)
                    // PlugIn.ModelCore.UI.WriteLine("   Biomass Insects:  Dispersal Neighborhood = {0} neighbors.", i);
                insect.LastBioRemoved = 0;

            }


             PlugIn.ModelCore.UI.WriteLine("   Opening BiomassInsect log file \"{0}\" ...", parameters.LogFileName);
            try {
                log = Landis.Data.CreateTextFile(parameters.LogFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }

            log.AutoFlush = true;
            log.Write("Time,InsectName,StartYear,StopYear,MeanDefoliation,NumSitesDefoliated0_33,NumSitesDefoliated33_66,NumSitesDefoliated66_100,NumOutbreakInitialSites,MortalityBiomassKg");
            //foreach (IEcoregion ecoregion in Ecoregions.Dataset)
            //      log.Write(",{0}", ecoregion.MapCode);
            log.WriteLine("");

        }

        //---------------------------------------------------------------------
        ///<summary>
        /// Run Biomass Insects extension at a particular timestep.
        ///</summary>
        public override void Run()
        {

            running = true;
             PlugIn.ModelCore.UI.WriteLine("   Processing landscape for Biomass Insect events ...");

            foreach(IInsect insect in manyInsect)
            {
                //SiteVars.BiomassRemoved.ActiveSiteValues = 0;
                //SiteVars.InitialOutbreakProb.ActiveSiteValues = 0.0;

                if(insect.MortalityYear == PlugIn.ModelCore.CurrentTime)
                    Outbreak.Mortality(insect);

                // Copy the data from current to last
                foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                    insect.LastYearDefoliation[site] = insect.ThisYearDefoliation[site];

                insect.ThisYearDefoliation.ActiveSiteValues = 0.0;

                insect.ActiveOutbreak = false;
                insect.SingleOutbreakYear = false;

                PlugIn.ModelCore.NormalDistribution.Mu = 0.0;
                PlugIn.ModelCore.NormalDistribution.Sigma = 1.0;
                double randomNum = PlugIn.ModelCore.NormalDistribution.NextDouble();

                PlugIn.ModelCore.ExponentialDistribution.Lambda = insect.MeanDuration;      // rate
                double randomNumE = PlugIn.ModelCore.ExponentialDistribution.NextDouble();

                // First, has enough time passed since the last outbreak? This is calculated each year, 
                // but only used in last year of an outbreak to generate next outbreak characteristics.
                double timeBetweenOutbreaks = insect.MeanTimeBetweenOutbreaks + (insect.StdDevTimeBetweenOutbreaks * randomNum);
                double duration = System.Math.Round(randomNumE + 1);
                double timeAfterDuration = timeBetweenOutbreaks - duration;

                //if (timeAfterDuration <= 1.0)
                //{
                //    PlugIn.ModelCore.UI.WriteLine("Time Between Outbreaks TOO SMALL:  {0}, {1}, {2}.", timeBetweenOutbreaks, duration, timeAfterDuration);
                //    throw new ApplicationException("Error: New timeAfterDuration is <= 1, Adjust timing parameters");
                //}

                // Users can parameterize model to have overlapping outbreaks, but then patches will not initialize correctly. 
                // Do below to prevent overlapping outbreaks of same insect. This will affect the realized distribution of time between outbreaks somewhat. 
                while (timeAfterDuration <= 1.0)
                {
                    timeBetweenOutbreaks = timeBetweenOutbreaks + 1;
                    timeAfterDuration = timeBetweenOutbreaks - duration;
                }

                PlugIn.ModelCore.UI.WriteLine("Calculated time between = {0}.  inputMeanTime={1}, inputStdTime={2}., timeAftDur={3}.", timeBetweenOutbreaks, insect.MeanTimeBetweenOutbreaks, insect.StdDevTimeBetweenOutbreaks,timeAfterDuration);
                //PlugIn.ModelCore.UI.WriteLine("Calculated duration     = {0}.  inputMeanDura={1}, inputStdDura={2}.", duration, insect.MeanDuration, insect.StdDevDuration);
                //PlugIn.ModelCore.UI.WriteLine("Insect Start Time = {0}, Stop Time = {1}.", insect.OutbreakStartYear, insect.OutbreakStopYear);

                // The logic below determines whether an outbreak is active. And sets a new outbreak duration and timeBetweenOutbreaks
                // for the next outbreak if is the last year of an outbreak.

                // The very first outbreak is set first.
                if(PlugIn.ModelCore.CurrentTime == 1)
                {
                     PlugIn.ModelCore.UI.WriteLine("   Year 1:  Setting initial start and stop times.");
                     double randomNum1 = PlugIn.ModelCore.GenerateUniform();
                    //insect.OutbreakStartYear = (int) (timeBetweenOutbreaks / 2.0) + 1;
                     insect.OutbreakStartYear = Math.Max(2, (int) (randomNum1 * timeBetweenOutbreaks + 1)); // New, try making 1st start year more random. 1st outbreak has to occur > year1 to for InitializeDefoliationPatches to work properly.
                     insect.OutbreakStopYear  = insect.OutbreakStartYear + (int) duration - 1;
                    // PlugIn.ModelCore.UI.WriteLine("   {0} is not active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, PlugIn.ModelCore.CurrentTime);
                }
                else if(insect.OutbreakStartYear <= PlugIn.ModelCore.CurrentTime
                    && insect.OutbreakStopYear > PlugIn.ModelCore.CurrentTime) // first year of a multiyear outbreak
                {
                    // PlugIn.ModelCore.UI.WriteLine("   An outbreak starts or continues.  Start and stop time do not change.");
                    insect.ActiveOutbreak = true;
                    // PlugIn.ModelCore.UI.WriteLine("   {0} is active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, PlugIn.ModelCore.CurrentTime);

                    insect.MortalityYear = PlugIn.ModelCore.CurrentTime + 1;
                    insect.LastStartYear = insect.OutbreakStartYear; // Added here for Brian's log file.
                    insect.LastStopYear = insect.OutbreakStopYear;  // Added here for Brian's log file.
                }
                //Special case for single year outbreak.
                else if (insect.OutbreakStartYear == PlugIn.ModelCore.CurrentTime
                    && insect.OutbreakStopYear == PlugIn.ModelCore.CurrentTime)
                {
                    insect.ActiveOutbreak = true;
                    //UI.WriteLine("   {0} is active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, Model.Core.CurrentTime);

                    if (insect.OutbreakStartYear == insect.OutbreakStopYear) // shouldn't need this. JRF
                        insect.SingleOutbreakYear = true;
                    insect.MortalityYear = PlugIn.ModelCore.CurrentTime + 1;
                    insect.LastStartYear = insect.OutbreakStartYear; // Added here for Brian's log file.
                    insect.LastStopYear = insect.OutbreakStopYear;  // Added here for Brian's log file.
                    insect.OutbreakStartYear = PlugIn.ModelCore.CurrentTime + (int)timeBetweenOutbreaks;
                    insect.OutbreakStopYear = insect.OutbreakStartYear + (int)duration - 1;
                }

                else if(insect.OutbreakStopYear <= PlugIn.ModelCore.CurrentTime
                    && timeAfterDuration > PlugIn.ModelCore.CurrentTime - insect.OutbreakStopYear)
                {
                    // PlugIn.ModelCore.UI.WriteLine("   In between outbreaks, reset start and stop times.");
                    insect.ActiveOutbreak = true;
                    // PlugIn.ModelCore.UI.WriteLine("   {0} is active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, PlugIn.ModelCore.CurrentTime);

                    insect.MortalityYear = PlugIn.ModelCore.CurrentTime + 1;
                    insect.LastStartYear = insect.OutbreakStartYear; // Added here for Brian's log file.
                    insect.LastStopYear = insect.OutbreakStopYear;  // Added here for Brian's log file.
                    insect.OutbreakStartYear = PlugIn.ModelCore.CurrentTime + (int) timeBetweenOutbreaks;
                    insect.OutbreakStopYear = insect.OutbreakStartYear + (int) duration - 1;
                }
                PlugIn.ModelCore.UI.WriteLine("  Insect Start Time = {0}, Stop Time = {1}.", insect.OutbreakStartYear, insect.OutbreakStopYear);

                // Now that logic determining when an outbreak will be active is done, tell model what to do when outbreak is occurring.
                if(insect.ActiveOutbreak)
                {
                   //  PlugIn.ModelCore.UI.WriteLine("   OutbreakStartYear={0}.", insect.OutbreakStartYear);

                    if (insect.OutbreakStartYear == PlugIn.ModelCore.CurrentTime || insect.SingleOutbreakYear)
                        // Initialize neighborhoodGrowthReduction with patches
                        Outbreak.InitializeDefoliationPatches(insect);
                    else
                        insect.NeighborhoodDefoliation.ActiveSiteValues = 0;

                }

                // Now report on the previous year's defoliation, that which has been processed
                // through biomass succession. Calculations for logfile.

                double sumDefoliation = 0.0;
                double meanDefoliation = 0.0;
                int numSites0_33 = 0;
                int numSites33_66 = 0;
                int numSites66_100 = 0;
                int numInitialSites = 0;
                int numSitesActive = 0; // Just get a sum of all active sites to calculate mean defoliation accurately for log file.

                // ONly calculate for log file when outbreak or mortality is active <- Modified, JRF, add to log file each year.
                if (insect.ActiveOutbreak || (insect.LastStopYear + 1 >= PlugIn.ModelCore.CurrentTime)  || (insect.LastBioRemoved > 0))
                {
                    foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                    {
                        sumDefoliation += insect.LastYearDefoliation[site];
                        if (insect.LastYearDefoliation[site] > 0.0 && insect.LastYearDefoliation[site] <= 0.33)
                            numSites0_33++;
                        if (insect.LastYearDefoliation[site] > 0.33 && insect.LastYearDefoliation[site] <= 0.66)
                            numSites33_66++;
                        if (insect.LastYearDefoliation[site] > 0.66 && insect.LastYearDefoliation[site] <= 1.0)
                            numSites66_100++;
                        if (insect.Disturbed[site] && SiteVars.InitialOutbreakProb[site] > 0)
                            numInitialSites++;
                        numSitesActive++;
                    }
                    if (insect.OutbreakStartYear == PlugIn.ModelCore.CurrentTime)
                        insect.InitialSites = numInitialSites;

                    if (numSites0_33 + numSites33_66 + numSites66_100 > 0)
                        meanDefoliation = sumDefoliation / (double)numSitesActive;
                     //PlugIn.ModelCore.UI.WriteLine("   sumDefoliation={0}, numSites={1}.", sumDefoliation, numSites0_33 + numSites33_66 + numSites66_100);
                }

                int totalBioRemoved = 0;
                foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                {
                    totalBioRemoved += SiteVars.BiomassRemoved[site]; // kg across all defoliated sites
                }

                // PlugIn.ModelCore.UI.WriteLine("   totalBioRemoved={0}.", totalBioRemoved);


                // ONly add to log & output maps during outbreak <- Modified, JRF, add to log file each year.
                //if ((insect.ActiveOutbreak && insect.OutbreakStartYear < PlugIn.ModelCore.CurrentTime) || (meanDefoliation > 0) || (insect.LastBioRemoved > 0))
                //{
                    if (meanDefoliation > 0)
                    {

                            log.Write("{0},{1},{2},{3},{4:0.00},{5},{6},{7},{8},{9}",
                                    PlugIn.ModelCore.CurrentTime-1,  //0
                                    insect.Name,  //1
                                    insect.LastStartYear,  //2
                                    insect.LastStopYear,  //3
                                    meanDefoliation, //4
                                    numSites0_33, //5
                                    numSites33_66,  //6
                                    numSites66_100, //7
                                    insect.InitialSites, //8
                                    insect.LastBioRemoved //9
                                    );
                    }
                    else
                    {
                        log.Write("{0},{1},{2},{3},{4:0.00},{5},{6},{7},{8},{9}",
                                PlugIn.ModelCore.CurrentTime-1,  //0
                                insect.Name,  //1
                                0,  //2
                                0,  //3
                                meanDefoliation, //4
                                numSites0_33, //5
                                numSites33_66,  //6
                                numSites66_100, //7
                                insect.InitialSites, //8
                                insect.LastBioRemoved //9
                                );
                    }

                    //foreach (IEcoregion ecoregion in Ecoregions.Dataset)
                    //    log.Write(",{0}", 1);

                    log.WriteLine("");


                    //----- Write Insect Defoliation/GrowthReduction maps --------
                    string path = MapNames.ReplaceTemplateVars(mapNameTemplate, insect.Name, PlugIn.ModelCore.CurrentTime - 1);
                    using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path, modelCore.Landscape.Dimensions))
                    {
                        ShortPixel pixel = outputRaster.BufferPixel;

                        foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                                pixel.MapCode.Value = (short)(insect.LastYearDefoliation[site] * 100.0);
                            else
                                //  Inactive site
                                pixel.MapCode.Value = 0;

                            outputRaster.WriteBufferPixel();
                        }
                    }

                    //----- Write Initial Patch maps --------
                    string path2 = MapNames.ReplaceTemplateVars(mapNameTemplate, ("InitialPatchMap" + insect.Name), PlugIn.ModelCore.CurrentTime);
                    using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path2, modelCore.Landscape.Dimensions))
                    {
                        ShortPixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                        {
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
                            //Zero out the InitialOutbreakProb after output maps are written.
                            SiteVars.InitialOutbreakProb[site] = 0;
                        }
                    }

                    //----- Write Biomass Reduction maps --------
                    string path3 = MapNames.ReplaceTemplateVars(mapNameTemplate, ("BiomassRemoved" + insect.Name), PlugIn.ModelCore.CurrentTime);
                    using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(path3, modelCore.Landscape.Dimensions))
                    {
                        ShortPixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                            {
                                if (SiteVars.BiomassRemoved[site] > 0)
                                    pixel.MapCode.Value = (short)(SiteVars.BiomassRemoved[site] / 100);  // convert to Mg/ha
                                else
                                    pixel.MapCode.Value = 0;
                            }
                            else
                            {
                                //  Inactive site
                                pixel.MapCode.Value = 0;
                            }
                            outputRaster.WriteBufferPixel();
                            //Zero out the BiomassRemoved after the last insect mortality event in a given year.
                            SiteVars.BiomassRemoved[site] = 0;
                        }
                    }
                //}

                //insect.ThisYearDefoliation.ActiveSiteValues = 0.0;  //reset this year to 0 for all sites, this was already done at the top of loop to initialize defoliation patchs, Outbreak.cs
                insect.LastBioRemoved = totalBioRemoved; //Assign variables for the logfile
                //if (insect.OutbreakStopYear <= PlugIn.ModelCore.CurrentTime
                //        && timeAfterDuration > PlugIn.ModelCore.CurrentTime - insect.OutbreakStopYear)
                //{
                    //insect.LastStartYear = insect.OutbreakStartYear;
                    //insect.LastStopYear = insect.OutbreakStopYear;
                    //insect.OutbreakStartYear = PlugIn.ModelCore.CurrentTime + (int)timeBetweenOutbreaks;
                    //insect.OutbreakStopYear = insect.OutbreakStartYear + (int)duration;
                    // PlugIn.ModelCore.UI.WriteLine("  Insect Start Time = {0}, Stop Time = {1}.", insect.OutbreakStartYear, insect.OutbreakStopYear);

                //}
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
             PlugIn.ModelCore.UI.WriteLine("   Creating Dispersal Neighborhood List.");

            List<RelativeLocation> neighborhood = new List<RelativeLocation>();

                int neighborRadius = neighborhoodDistance;
                int numCellRadius = (int) (neighborRadius / CellLength);
                // PlugIn.ModelCore.UI.WriteLine("   Insect:  NeighborRadius={0}, CellLength={1}, numCellRadius={2}",
                //        neighborRadius, CellLength, numCellRadius);
                double centroidDistance = 0;
                double cellLength = CellLength;

                for(int row=(numCellRadius * -1); row<=numCellRadius; row++)
                {
                    for(int col=(numCellRadius * -1); col<=numCellRadius; col++)
                    {
                        centroidDistance = DistanceFromCenter(row, col);

                        // PlugIn.ModelCore.UI.WriteLine("Centroid Distance = {0}.", centroidDistance);
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
