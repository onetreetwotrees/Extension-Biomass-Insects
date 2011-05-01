//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

using Edu.Wisc.Forest.Flel.Util;
using Landis.SpatialModeling;
using Landis.Core;
using System.Collections.Generic;

namespace Landis.Extension.Insects
{
    /// <summary>
    /// Parameters for the extension.
    /// </summary>
    public interface IInsect
    {
        string Name {get;set;}
        double MeanDuration {get;set;}
        int StdDevDuration {get;set;}
        int MeanTimeBetweenOutbreaks {get;set;}
        int StdDevTimeBetweenOutbreaks {get;set;}
        int NeighborhoodDistance {get;set;}

        double InitialAreaCalibrator {get;set;}
        DistributionType InitialPatchDistr {get;set;}
        double InitialPatchValue1 {get;set;}
        double InitialPatchValue2 {get;set;}

        int OutbreakStartYear {get;set;}
        int OutbreakStopYear {get;set;}
        int MortalityYear {get; set;}

        List<ISppParameters> SppTable{get;set;}
        List<ISusceptible> SusceptibleTable{get;set;}
        IEnumerable<RelativeLocation> Neighbors{get;set;}

        //ISiteVar<byte> Severity{get;set;}
        ISiteVar<bool> Disturbed{get;set;}
        ISiteVar<Dictionary<int,double[]>> HostDefoliationByYear{get;set;}
        ISiteVar<double> LastYearDefoliation{get;set;}
        ISiteVar<double> ThisYearDefoliation{get;set;}

        bool ActiveOutbreak{get;set;}


    }
}

namespace Landis.Extension.Insects
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class Insect
        : IInsect
    {
        private string name;
        private double meanDuration;
        private int stdDevDuration;
        private int meanTimeBetweenOutbreaks;
        private int stdDevTimeBetweenOutbreaks;
        private int neighborhoodDistance;

        private double initialAreaCalibrator;
        private DistributionType initialPatchDistr;
        private double initialPatchValue1;
        private double initialPatchValue2;

        private int outbreakStartYear;
        private int outbreakStopYear;
        private int mortalityYear;

        private bool activeOutbreak;

        private List<ISppParameters> sppTable;
        private List<ISusceptible> susceptibleTable;
        IEnumerable<RelativeLocation> neighbors;

        //private ISiteVar<byte> severity;
        private ISiteVar<Dictionary<int, double[]>> hostDefoliationByYear;
        private ISiteVar<bool> disturbed;
        private ISiteVar<double> lastYearDefoliation;
        private ISiteVar<double> thisYearDefoliation;

        //---------------------------------------------------------------------

        public string Name
        {
            get {
                return name;
            }
            set {
                name = value;
            }
        }

        //---------------------------------------------------------------------
        public double MeanDuration
        {
            get {
                return meanDuration;
            }
            set {
                if (value <= 0)
                    throw new InputValueException(value.ToString(), "Value must be  > 0.");
                meanDuration = value;
            }
        }
        //---------------------------------------------------------------------
        public int StdDevDuration
        {
            get {
                return stdDevDuration;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be  > 0.");
                 stdDevDuration = value;
            }
        }
        //---------------------------------------------------------------------
        public int MeanTimeBetweenOutbreaks
        {
            get {
                return meanTimeBetweenOutbreaks;
            }
            set {
                if (value <= 0)
                        throw new InputValueException(value.ToString(), "Value must be  > 0.");
                 meanTimeBetweenOutbreaks = value;
            }
        }
        //---------------------------------------------------------------------
        public int StdDevTimeBetweenOutbreaks
        {
            get {
                return stdDevTimeBetweenOutbreaks;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(), "Value must be  > 0.");
                 stdDevTimeBetweenOutbreaks = value;
            }
        }

        //---------------------------------------------------------------------
        public int NeighborhoodDistance
        {
            get {
                return neighborhoodDistance;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(), "Value must be  > or = 0.");
                 neighborhoodDistance = value;
            }
        }
        //---------------------------------------------------------------------
        public double InitialAreaCalibrator
        {
            get {
                return initialAreaCalibrator;
            }
            set {
                 if (value <= 0)
                        throw new InputValueException(value.ToString(), "Value must be  > 0.");
                 initialAreaCalibrator = value;
            }
        }
        //---------------------------------------------------------------------
        public DistributionType InitialPatchDistr
        {
            get {
                return initialPatchDistr;
            }
            set {
                 initialPatchDistr = value;
            }
        }
        //---------------------------------------------------------------------
        public double InitialPatchValue1
        {
            get {
                return initialPatchValue1;
            }
            set {
                 if (value <= 0)
                        throw new InputValueException(value.ToString(), "Value must be  > 0.");
                 initialPatchValue1 = value;
            }
        }
        //---------------------------------------------------------------------
        public double InitialPatchValue2
        {
            get {
                return initialPatchValue2;
            }
            set {
                 if (value <= 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be  > 0.");
                 initialPatchValue2 = value;
            }
        }
        //---------------------------------------------------------------------
        public int OutbreakStartYear
        {
            get {
                return outbreakStartYear;
            }
            set {
                outbreakStartYear = value;
            }
        }

        //---------------------------------------------------------------------
        public int OutbreakStopYear
        {
            get {
                return outbreakStopYear;
            }
            set {
                outbreakStopYear = value;
            }
        }
        //---------------------------------------------------------------------
        public int MortalityYear
        {
            get {
                return mortalityYear;
            }
            set {
                mortalityYear = value;
            }
        }
        //---------------------------------------------------------------------
        public bool ActiveOutbreak
        {
            get {
                return activeOutbreak;
            }
            set {
                activeOutbreak = value;
            }
        }
        //---------------------------------------------------------------------
        public List<ISppParameters> SppTable
        {
            get {
                return sppTable;
            }
            set {
                sppTable = value;
            }
        }
        //---------------------------------------------------------------------
        public List<ISusceptible> SusceptibleTable
        {
            get {
                return susceptibleTable;
            }
            set {
                susceptibleTable = value;
            }
        }

        //---------------------------------------------------------------------
        public IEnumerable<RelativeLocation> Neighbors
        {
            get {
                return neighbors;
            }
            set {
                neighbors = value;
            }
        }

        //---------------------------------------------------------------------
        public ISiteVar<Dictionary<int, double[]>> HostDefoliationByYear
        {
            get {
                return hostDefoliationByYear;
            }
            set {
                hostDefoliationByYear = value;
            }
        }
        //---------------------------------------------------------------------
        public ISiteVar<bool> Disturbed
        {
            get {
                return disturbed;
            }
            set {
                disturbed = value;
            }
        }
        //---------------------------------------------------------------------
        public ISiteVar<double> LastYearDefoliation
        {
            get {
                return lastYearDefoliation;
            }
            set {
                lastYearDefoliation = value;
            }
        }
        //---------------------------------------------------------------------
        public ISiteVar<double> ThisYearDefoliation
        {
            get {
                return thisYearDefoliation;
            }
            set {
                thisYearDefoliation = value;
            }
        }

        //---------------------------------------------------------------------
        public Insect(int sppCount)
        {
            sppTable = new List<ISppParameters>(sppCount);
            susceptibleTable = new List<ISusceptible>();
            neighbors = new List<RelativeLocation>();

            hostDefoliationByYear = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, double[]>>();
            disturbed = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();
            lastYearDefoliation = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            thisYearDefoliation = PlugIn.ModelCore.Landscape.NewSiteVar<double>();

            outbreakStopYear = 0;  //default = beginning of simulation
            outbreakStartYear = 0;  //default = beginning of simulation
            mortalityYear = 0;  //default = beginning of simulation
            activeOutbreak = false;
            
            //Initialize outbreaks:
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                hostDefoliationByYear[site] = new Dictionary<int, double[]>();
            }
        }
        //---------------------------------------------------------------------
/*        public Insect(
                            string name,
                            double meanDuration,
                            int stdDevDuration,
                            int meanTimeBetweenOutbreaks,
                            int stdDevTimeBetweenOutbreaks,
                            int neighborhoodDistance,
                            double initialAreaCalibrator,
                            DistributionType initialPatchDistr,
                            double initialPatchValue1,
                            double initialPatchValue2,
                          ISppParameters[] sppTable,
                          ISusceptible[] susceptible
                          )
        {
            this.name = name;
            this.meanDuration = meanDuration;
            this.stdDevDuration = stdDevDuration;
            this.meanTimeBetweenOutbreaks = meanTimeBetweenOutbreaks;
            this.stdDevTimeBetweenOutbreaks = stdDevTimeBetweenOutbreaks;
            this.neighborhoodDistance = neighborhoodDistance;
            this.initialAreaCalibrator = initialAreaCalibrator;
            this.initialPatchDistr  = initialPatchDistr;
            this.initialPatchValue1 = initialPatchValue1;
            this.initialPatchValue2 = initialPatchValue2;
            this.sppTable = sppTable;
            this.susceptible = susceptible;

            this.hostDefoliationByYear = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, double[]>>();
            //this.severity = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();
            this.disturbed = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();
            this.lastYearDefoliation = PlugIn.ModelCore.Landscape.NewSiteVar<double>();
            this.thisYearDefoliation = PlugIn.ModelCore.Landscape.NewSiteVar<double>();

            this.outbreakStopYear = 0;  //default = beginning of simulation
            this.outbreakStartYear = 0;  //default = beginning of simulation
            this.mortalityYear = 0;  //default = beginning of simulation
            this.activeOutbreak = false;

            //Initialize outbreaks:
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                this.hostDefoliationByYear[site] = new Dictionary<int, double[]>();
            }

        }*/
    }
}
