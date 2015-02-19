//  Copyright 2008 University of Wisconsin, Conservation Biology Institute
//  Authors:  
//      Robert M. Scheller
//      Jane Foster
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Grids;
using Landis.Biomass;//AgeCohort;
//using Landis.Ecoregions;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Species;
using Landis.Util;
using System.Collections.Generic;
using System;


namespace Landis.Insects
{
    public class Outbreak

    {
        //private static Ecoregions.IDataset ecoregions;
        private static ILandscapeCohorts cohorts;

        private IInsect outbreakParms;

        //collect all 8 relative neighbor locations in array
        private static RelativeLocation[] all_neighbor_locations = new RelativeLocation[]
        {
                new RelativeLocation(-1,0),
                new RelativeLocation(1,0),
                new RelativeLocation(0,-1),
                new RelativeLocation(0,1),
                //new RelativeLocation(-1,-1),
                new RelativeLocation(-1,1),
                new RelativeLocation(1,-1),
                //new RelativeLocation(1,1),
                new RelativeLocation(-2,1),
                new RelativeLocation(2,-1),
                new RelativeLocation(-2,2)

        };

        //---------------------------------------------------------------------
        // Outbreak Constructor
        public Outbreak(IInsect insect)
        {
            this.outbreakParms = insect;
        }


        //---------------------------------------------------------------------
        ///<summary>
        // Go through all active sites and damage them.  Mortality should occur the year FOLLOWING an active year.
        ///</summary>
        public static void Mortality(IInsect insect)
        {
            
            UI.WriteLine("   {0} mortality.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, Model.Core.CurrentTime);


            foreach (ActiveSite site in Model.Core.Landscape) 
            {
                //Try zeroing out biomass removed here @ start of each defoliation mortality year. Remove if doesn't work.
                if (SiteVars.BiomassRemoved[site] > 0)
                  SiteVars.BiomassRemoved[site] = 0;

                PartialDisturbance.ReduceCohortBiomass(site);
                    
                if (SiteVars.BiomassRemoved[site] > 0) 
                {
                        SiteVars.TimeOfLastEvent[site] = Model.Core.CurrentTime;
                        //SiteVars.WoodyDebris[site].AddMass(SiteVars.BiomassRemoved[site], 0.1);
                } 
            }
        }


        //---------------------------------------------------------------------
        // Initialize landscape with patches of defoliation during the first year
        public static void InitializeDefoliationPatches(IInsect insect)
        {

            UI.WriteLine("   Initializing Defoliation Patches... ");   
            SiteVars.InitialOutbreakProb.ActiveSiteValues = 0.0;
            insect.Disturbed.ActiveSiteValues = false;
            
            cohorts = Model.Core.SuccessionCohorts as ILandscapeCohorts;
            
            foreach(ActiveSite site in Model.Core.Landscape)
            {
            
                double suscIndexSum = 0.0;
                double sumBio = 0.0;


                foreach(ISpecies species in Model.Core.Species)
                {
                    ISpeciesCohorts speciesCohorts = cohorts[site][species];
                    
                    if(speciesCohorts == null)
                        continue;
                
                    foreach (ICohort cohort in speciesCohorts) 
                    {
                        suscIndexSum += cohort.Biomass * (insect.SppTable[cohort.Species.Index].Susceptibility);
                        sumBio += cohort.Biomass;
                    }
                }
                
                
                // If no biomass, no chance of defoliation, go to the next site.
                if(suscIndexSum <= 0 || sumBio <=0)
                {
                    SiteVars.InitialOutbreakProb[site] = 0.0;
                    continue;
                }
                
                int suscIndex = (int) Math.Round(suscIndexSum /sumBio) - 1;
                
                if (suscIndex > 2.0 || suscIndex < 0)
                {
                    UI.WriteLine("SuscIndex < 0 || > 2.  Site R/C={0}/{1},suscIndex={2},suscIndexSum={3},sumBio={4}.", site.Location.Row, site.Location.Column, suscIndex,suscIndexSum,sumBio);
                    throw new ApplicationException("Error: SuscIndex is not between 2.0 and 0.0");
                }
                // Assume that there are no neighbors whatsoever:
                DistributionType dist = insect.SusceptibleTable[suscIndex].Distribution_80.Name;


                //UI.WriteLine("suscIndex={0},suscIndexSum={1},cohortBiomass={2}.", suscIndex,suscIndexSum,sumBio);
                double value1 = insect.SusceptibleTable[suscIndex].Distribution_80.Value1;
                double value2 = insect.SusceptibleTable[suscIndex].Distribution_80.Value2;

                double probability = Distribution.GenerateRandomNum(dist, value1, value2);
                if(probability > 1.0 || probability < 0)
                {
                    UI.WriteLine("Initial Defoliation Probility < 0 || > 1.  Site R/C={0}/{1}.", site.Location.Row, site.Location.Column);
                    throw new ApplicationException("Error: Probability is not between 1.0 and 0.0");
                }
                
                SiteVars.InitialOutbreakProb[site] = probability;
                //UI.WriteLine("Susceptiblity index={0}.  Outbreak Probability={1:0.00}.  R/C={2}/{3}.", suscIndex, probability, site.Location.Row, site.Location.Column);
            }

            foreach(ActiveSite site in Model.Core.Landscape)
            {

                //get a random site from the stand
                double randomNum = Landis.Util.Random.GenerateUniform();
                double randomNum2 = Landis.Util.Random.GenerateUniform();
                //Create random variability in outbreak area within a simulation so outbreaks are more variable.
                double initialAreaCalibratorRandomNum = (randomNum2 - 0.5) * insect.InitialAreaCalibrator / 2;

                if(randomNum < SiteVars.InitialOutbreakProb[site] * (insect.InitialAreaCalibrator + initialAreaCalibratorRandomNum))  
                //Start spreading!
                {
            
                    //start with this site (if it's active)
                    ActiveSite currentSite = site;           
            
                    //queue to hold sites to defoliate
                    Queue<ActiveSite> sitesToConsider = new Queue<ActiveSite>();
            
                    //put initial site on queue
                    sitesToConsider.Enqueue(currentSite);
            
                    DistributionType dist = insect.InitialPatchDistr;
                    double targetArea = Distribution.GenerateRandomNum(dist, insect.InitialPatchValue1, insect.InitialPatchValue2);
                    
                    //UI.WriteLine("  Target Patch Area={0:0.0}.", targetArea);
                    double areaSelected = 0.0;
            
                    //loop through stand, defoliating patches of size target area
                    while (sitesToConsider.Count > 0 && areaSelected < targetArea) 
                    {

                        currentSite = sitesToConsider.Dequeue();
                    
                        // Because this is the first year, neighborhood defoliation is given a value.
                        // The value is used in Defoliate.DefoliateCohort()
                        insect.NeighborhoodDefoliation[currentSite] = SiteVars.InitialOutbreakProb[currentSite];
                        areaSelected += Model.Core.CellArea;

                        //Next, add site's neighbors to the list of
                        //sites to consider.  
                        //loop through the site's neighbors enqueueing all the good ones.
                        foreach (RelativeLocation loc in all_neighbor_locations) 
                        {
                            Site neighbor = currentSite.GetNeighbor(loc);

                            //get a neighbor site (if it's non-null and active)
                            if (neighbor != null 
                                && neighbor.IsActive  
                                && !sitesToConsider.Contains((ActiveSite) neighbor)
                                && !insect.Disturbed[neighbor]) 
                            {
                                insect.Disturbed[currentSite] = true;
                                randomNum = Landis.Util.Random.GenerateUniform();
                                //UI.WriteLine("That darn Queue!  randomnum={0}, prob={1}.", randomNum, SiteVars.InitialOutbreakProb[neighbor]);
                                
                                //check if it's a valid neighbor:
                                if (SiteVars.InitialOutbreakProb[neighbor] > randomNum)
                                {
                                    sitesToConsider.Enqueue((ActiveSite) neighbor);
                                }
                            }
                        }
                    } //endwhile
                    
                    //UI.WriteLine("   Initial Patch Area Selected={0:0.0}.", areaSelected);
                } //endif

            } //endfor
        
        
        } //endfunc
        
    }
    
}

