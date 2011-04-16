//  Copyright 2008-2010 University of Wisconsin, Portland State University
//  Authors:
//      Jane Foster
//      Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Species;
using System.Collections.Generic;
using System.IO;
using System;

using Landis.RasterIO;
using Edu.Wisc.Forest.Flel.Grids;
using Troschuetz.Random;

namespace Landis.Extension.Insects
{
    ///<summary>
    /// A disturbance plug-in that simulates Biological Agents.
    /// </summary>

    public class PlugIn
        : Landis.PlugIns.PlugIn, Landis.PlugIns.ICleanUp
    {
        public static readonly PlugInType Type = new PlugInType("disturbance:insects");

        private string mapNameTemplate;
        private StreamWriter log;
        private static List<IInsect> manyInsect;
        private bool running;

        //---------------------------------------------------------------------

        public PlugIn()
            : base("Biomass Insects", Type)
        {
        }

        //---------------------------------------------------------------------
        public static List<IInsect> ManyInsect
        {
            get {
                return manyInsect;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Initializes the extension with a data file.
        /// </summary>
        public override void Initialize(string        dataFile,
                                        PlugIns.ICore modelCore)
        {

            Model.Core = modelCore;

            InputParameterParser parser = new InputParameterParser();

            IInputParameters parameters = Data.Load<IInputParameters>(dataFile, parser);

            Timestep = 1; //parameters.Timestep;
            mapNameTemplate = parameters.MapNamesTemplate;
            manyInsect = parameters.ManyInsect;

            SiteVars.Initialize();
            Defoliate.Initialize(parameters);
            GrowthReduction.Initialize(parameters);

            // Add local event handler for cohorts death due to age-only
            // disturbances.
            Biomass.Cohort.AgeOnlyDeathEvent += CohortKilledByAgeOnlyDisturbance;

            foreach(IInsect insect in manyInsect)
            {

                if(insect == null)
                    UI.WriteLine("Insect Parameters NOT loading correctly.");

                insect.Neighbors = GetNeighborhood(insect.NeighborhoodDistance);

                int i=0;

                foreach(RelativeLocation location in insect.Neighbors)
                    i++;

                if(insect.Neighbors != null)
                    UI.WriteLine("   Dispersal Neighborhood = {0} neighbors.", i);

            }


            UI.WriteLine("Opening BiomassInsect log file \"{0}\" ...", parameters.LogFileName);
            try {
                log = Data.CreateTextFile(parameters.LogFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }

            log.AutoFlush = true;
            log.Write("Time,InsectName,StartYear,StopYear,MeanDefoliation,NumSitesDefoliated");
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
            UI.WriteLine("   Processing landscape for Biomass Insect events ...");

            foreach(IInsect insect in manyInsect)
            {

                if(insect.MortalityYear == Model.Core.CurrentTime)
                    Outbreak.Mortality(insect);

                // Copy the data from current to last
                foreach (ActiveSite site in Model.Core.Landscape)
                    insect.LastYearDefoliation[site] = insect.ThisYearDefoliation[site];

                insect.ThisYearDefoliation.ActiveSiteValues = 0.0;
                SiteVars.BiomassRemoved.ActiveSiteValues = 0;

                insect.ActiveOutbreak = false;

                NormalDistribution randVar = new NormalDistribution(RandomNumberGenerator.Singleton);
                randVar.Mu = 0;      // mean
                randVar.Sigma = 1;   // std dev
                double randomNum = randVar.NextDouble();

                ExponentialDistribution randVarE = new ExponentialDistribution(RandomNumberGenerator.Singleton);
                randVarE.Lambda = insect.MeanDuration;      // rate
                double randomNumE = randVarE.NextDouble();

                // First, has enough time passed since the last outbreak?
                double timeBetweenOutbreaks = insect.MeanTimeBetweenOutbreaks + (insect.StdDevTimeBetweenOutbreaks * randomNum);
                //double duration = insect.MeanDuration + (insect.StdDevDuration * randomNum);
                double duration = Math.Round(randomNumE + 1);
                double timeAfterDuration = timeBetweenOutbreaks - duration;

                //UI.WriteLine("Calculated time between = {0}.  inputMeanTime={1}, inputStdTime={2}.", timeBetweenOutbreaks, insect.MeanTimeBetweenOutbreaks, insect.StdDevTimeBetweenOutbreaks);
                //UI.WriteLine("Calculated duration     = {0}.  inputMeanDura={1}, inputStdDura={2}.", duration, insect.MeanDuration, insect.StdDevDuration);
                //UI.WriteLine("Insect Start Time = {0}, Stop Time = {1}.", insect.OutbreakStartYear, insect.OutbreakStopYear);


                if(Model.Core.CurrentTime == 1)
                {
                    //UI.WriteLine("   Year 1:  Setting initial start and stop times.");
                    insect.OutbreakStartYear = (int) (timeBetweenOutbreaks / 2.0) + 1;
                    insect.OutbreakStopYear  = insect.OutbreakStartYear + (int) duration - 1;
                    UI.WriteLine("   {0} is not active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, Model.Core.CurrentTime);
                }
                else if(insect.OutbreakStartYear <= Model.Core.CurrentTime
                    && insect.OutbreakStopYear > Model.Core.CurrentTime)
                {
                    //UI.WriteLine("   An outbreak starts or continues.  Start and stop time do not change.");
                    insect.ActiveOutbreak = true;
                    UI.WriteLine("   {0} is active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, Model.Core.CurrentTime);

                    insect.MortalityYear = Model.Core.CurrentTime + 1;

                }
                else if(insect.OutbreakStopYear <= Model.Core.CurrentTime
                    && timeAfterDuration > Model.Core.CurrentTime - insect.OutbreakStopYear)
                {
                    //UI.WriteLine("   In between outbreaks, reset start and stop times.");
                    insect.ActiveOutbreak = true;
                    UI.WriteLine("   {0} is active.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, Model.Core.CurrentTime);

                    insect.MortalityYear = Model.Core.CurrentTime + 1;
                    insect.OutbreakStartYear = Model.Core.CurrentTime + (int) timeBetweenOutbreaks;
                    insect.OutbreakStopYear = insect.OutbreakStartYear + (int) duration;
                }
                //UI.WriteLine("  Insect Start Time = {0}, Stop Time = {1}.", insect.OutbreakStartYear, insect.OutbreakStopYear);

                if(insect.ActiveOutbreak)
                {
                    //UI.WriteLine("   OutbreakStartYear={0}.", insect.OutbreakStartYear);

                    if(insect.OutbreakStartYear == Model.Core.CurrentTime)
                        // Initialize neighborhoodGrowthReduction with patches
                        Outbreak.InitializeDefoliationPatches(insect);
                    else
                        SiteVars.NeighborhoodDefoliation.ActiveSiteValues = 0;

                    double sumDefoliation = 0.0;
                    int numSites = 0;
                    foreach(ActiveSite site in Model.Core.Landscape)
                    {
                        sumDefoliation += insect.ThisYearDefoliation[site];
                        if(insect.ThisYearDefoliation[site] > 0.0)
                            numSites++;

                    }
                    double meanDefoliation = sumDefoliation / (double) numSites;


                    log.Write("{0},{1},{2},{3},{4},{5}",
                        Model.Core.CurrentTime,
                        insect.Name,
                        insect.OutbreakStartYear,
                        insect.OutbreakStopYear,
                        meanDefoliation,
                        numSites
                        );

                    //foreach (IEcoregion ecoregion in Ecoregions.Dataset)
                    //    log.Write(",{0}", 1);

                    log.WriteLine("");

                }


                //Only write maps if an outbreak is active.
                //if (!insect.ActiveOutbreak)
                //if (insect.OutbreakStartYear <= Model.Core.CurrentTime
                //    && insect.OutbreakStopYear + 1 >= Model.Core.CurrentTime)
                //if (insect.OutbreakStartYear <= Model.Core.CurrentTime)
                //    | insect.MortalityYear = Model.Core.CurrentTime)
                //    continue;

                //----- Write Insect GrowthReduction maps --------
                IOutputRaster<UShortPixel> map = CreateMap((Model.Core.CurrentTime - 1), insect.Name);

                using (map) {
                    UShortPixel pixel = new UShortPixel();
                    foreach (Site site in Model.Core.Landscape.AllSites) {
                        if (site.IsActive)
                                pixel.Band0 = (ushort) (insect.LastYearDefoliation[site] * 100.0);
                        else
                            //  Inactive site
                            pixel.Band0 = 0;

                        map.WritePixel(pixel);
                    }
                }

                //----- Write Initial Patch maps --------
                IOutputRaster<UShortPixel> map2 = CreateMap(Model.Core.CurrentTime, ("InitialPatchMap" + insect.Name));
                using (map2) {
                    UShortPixel pixel = new UShortPixel();
                    foreach (Site site in Model.Core.Landscape.AllSites) {
                        if (site.IsActive)
                        {
                            if (insect.Disturbed[site])
                                pixel.Band0 = (ushort) (SiteVars.InitialOutbreakProb[site] * 100);
                            else
                                pixel.Band0 = 0;
                        }
                        else
                        {
                            //  Inactive site
                            pixel.Band0 = 0;
                        }
                        map2.WritePixel(pixel);
                    }
                }

                //----- Write Biomass Reduction maps --------
                IOutputRaster<UShortPixel> map3 = CreateMap(Model.Core.CurrentTime, ("BiomassRemoved-" + insect.Name));
                using (map3) {
                    UShortPixel pixel = new UShortPixel();
                    foreach (Site site in Model.Core.Landscape.AllSites) {
                        if (site.IsActive)
                        {
                            // TESTING added by RMS
                            if(SiteVars.BiomassRemoved[site] > 0)
                                UI.WriteLine("  Biomass revoved at {0}/{1}: {2}.", site.Location.Row, site.Location.Column, SiteVars.BiomassRemoved[site]);
                            pixel.Band0 = (ushort) (SiteVars.BiomassRemoved[site] / 100);

                        }
                        else
                        {
                            //  Inactive site
                            pixel.Band0 = 0;
                        }
                        map3.WritePixel(pixel);
                    }
                }
            }

        }

        //---------------------------------------------------------------------

        // Event handler when a cohort is killed by an age-only disturbance.
        public void CohortKilledByAgeOnlyDisturbance(object                 sender,
                                                     Biomass.DeathEventArgs eventArgs)
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
        private IOutputRaster<UShortPixel> CreateMap(int currentTime, string MapName)
        {
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, MapName, currentTime);
            UI.WriteLine("   Writing BiomassInsect GrowthReduction map to {0} ...", path);
            return Model.Core.CreateRaster<UShortPixel>(path,
                                                          Model.Core.Landscape.Dimensions,
                                                          Model.Core.LandscapeMapMetadata);
        }

        //---------------------------------------------------------------------
        // Generate a Relative RelativeLocation array of neighbors.
        // Check each cell within a circle surrounding the center point.  This will
        // create a set of POTENTIAL neighbors.  These potential neighbors
        // will need to be later checked to ensure that they are within the landscape
        // and active.

        private static IEnumerable<RelativeLocation> GetNeighborhood(int neighborhoodDistance)
        {
            double CellLength = Model.Core.CellLength;
            UI.WriteLine("   Creating Dispersal Neighborhood List.");

            List<RelativeLocation> neighborhood = new List<RelativeLocation>();

                int neighborRadius = neighborhoodDistance;
                int numCellRadius = (int) (neighborRadius / CellLength);
                UI.WriteLine("   Insect:  NeighborRadius={0}, CellLength={1}, numCellRadius={2}",
                        neighborRadius, CellLength, numCellRadius);
                double centroidDistance = 0;
                double cellLength = CellLength;

                for(int row=(numCellRadius * -1); row<=numCellRadius; row++)
                {
                    for(int col=(numCellRadius * -1); col<=numCellRadius; col++)
                    {
                        centroidDistance = DistanceFromCenter(row, col);

                        //UI.WriteLine("Centroid Distance = {0}.", centroidDistance);
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
            double CellLength = Model.Core.CellLength;
            row = System.Math.Abs(row) * CellLength;
            column = System.Math.Abs(column) * CellLength;
            double aSq = System.Math.Pow(column,2);
            double bSq = System.Math.Pow(row,2);
            return System.Math.Sqrt(aSq + bSq);
        }
        //---------------------------------------------------------------------

        void PlugIns.ICleanUp.CleanUp()
        {
            if (log != null) {
                log.Close();
                log = null;
            }
        }
    }

}
