//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

using Landis.SpatialModeling;
using Landis.Extension.Succession.Biomass;
using Landis.Core;
using Landis.Library.BiomassCohorts;
using System.Collections.Generic;
using System;


namespace Landis.Extension.Insects
{
    public class Outbreak

    {
        //private static Ecoregions.IDataset ecoregions;
        //private static ILandscapeCohorts cohorts;

        private IInsect outbreakParms;

        //collect all 8 relative neighbor locations in array
        private static RelativeLocation[] all_neighbor_locations = new RelativeLocation[]
        {
                new RelativeLocation(-1,0),
                new RelativeLocation(1,0),
                new RelativeLocation(0,-1),
                new RelativeLocation(0,1),
                new RelativeLocation(-1,-1),
                new RelativeLocation(-1,1),
                new RelativeLocation(1,-1),
                new RelativeLocation(1,1)
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
            
            PlugIn.ModelCore.Log.WriteLine("   {0} mortality.  StartYear={1}, StopYear={2}, CurrentYear={3}.", insect.Name, insect.OutbreakStartYear, insect.OutbreakStopYear, PlugIn.ModelCore.CurrentTime);


            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) 
            {

                PartialDisturbance.ReduceCohortBiomass(site);
                    
                if (SiteVars.BiomassRemoved[site] > 0) 
                {
                        SiteVars.TimeOfLastEvent[site] = PlugIn.ModelCore.CurrentTime;
                } 
            }
        }


        //---------------------------------------------------------------------
        // Initialize landscape with patches of defoliation during the first year
        public static void InitializeDefoliationPatches(IInsect insect)
        {

            PlugIn.ModelCore.Log.WriteLine("   Initializing Defoliation Patches... ");   
            SiteVars.InitialOutbreakProb.ActiveSiteValues = 0.0;
            insect.Disturbed.ActiveSiteValues = false;
            
            foreach(ActiveSite site in PlugIn.ModelCore.Landscape)
            {
            
                double suscIndexSum = 0.0;
                double sumBio = 0.0;


                foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                    //foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    //ISpeciesCohorts speciesCohorts = SiteVars.Cohorts[site][species];
                    
                    //if(speciesCohorts == null)
                    //    continue;
                
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
                    PlugIn.ModelCore.Log.WriteLine("SuscIndex < 0 || > 2.  Site R/C={0}/{1},suscIndex={2},suscIndexSum={3},sumBio={4}.", site.Location.Row, site.Location.Column, suscIndex,suscIndexSum,sumBio);
                    throw new ApplicationException("Error: SuscIndex is not between 2.0 and 0.0");
                }
                // Assume that there are no neighbors whatsoever:
                DistributionType dist = insect.SusceptibleTable[suscIndex].Distribution_80.Name;


                //PlugIn.ModelCore.Log.WriteLine("suscIndex={0},suscIndexSum={1},cohortBiomass={2}.", suscIndex,suscIndexSum,sumBio);
                double value1 = insect.SusceptibleTable[suscIndex].Distribution_80.Value1;
                double value2 = insect.SusceptibleTable[suscIndex].Distribution_80.Value2;

                double probability = Distribution.GenerateRandomNum(dist, value1, value2);
                if(probability > 1.0 || probability < 0)
                {
                    PlugIn.ModelCore.Log.WriteLine("Initial Defoliation Probility < 0 || > 1.  Site R/C={0}/{1}.", site.Location.Row, site.Location.Column);
                    throw new ApplicationException("Error: Probability is not between 1.0 and 0.0");
                }
                
                SiteVars.InitialOutbreakProb[site] = probability;
                //PlugIn.ModelCore.Log.WriteLine("Susceptiblity index={0}.  Outbreak Probability={1:0.00}.  R/C={2}/{3}.", suscIndex, probability, site.Location.Row, site.Location.Column);
            }

            foreach(ActiveSite site in PlugIn.ModelCore.Landscape)
            {

                //get a random site from the stand
                double randomNum = Landis.Util.Random.GenerateUniform();
                
                if(randomNum < SiteVars.InitialOutbreakProb[site] * insect.InitialAreaCalibrator)  
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
                    
                    //PlugIn.ModelCore.Log.WriteLine("  Target Patch Area={0:0.0}.", targetArea);
                    double areaSelected = 0.0;
            
                    //loop through stand, defoliating patches of size target area
                    while (sitesToConsider.Count > 0 && areaSelected < targetArea) 
                    {

                        currentSite = sitesToConsider.Dequeue();
                    
                        // Because this is the first year, neighborhood defoliaiton is given a value.
                        // The value is used in Defoliate.DefoliateCohort()
                        SiteVars.NeighborhoodDefoliation[currentSite] = SiteVars.InitialOutbreakProb[currentSite];
                        areaSelected += PlugIn.ModelCore.CellArea;

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
                                //PlugIn.ModelCore.Log.WriteLine("That darn Queue!  randomnum={0}, prob={1}.", randomNum, SiteVars.InitialOutbreakProb[neighbor]);
                                
                                //check if it's a valid neighbor:
                                if (SiteVars.InitialOutbreakProb[neighbor] > randomNum)
                                {
                                    sitesToConsider.Enqueue((ActiveSite) neighbor);
                                }
                            }
                        }
                    } //endwhile
                    
                    //PlugIn.ModelCore.Log.WriteLine("   Initial Patch Area Selected={0:0.0}.", areaSelected);
                } //endif

            } //endfor
        
        
        } //endfunc
        
    }
    
}

