//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

using Edu.Wisc.Forest.Flel.Util;
//using Troschuetz.Random;

namespace Landis.Extension.Insects
{
    public enum DistributionType {Gamma, Beta, Weibull, Normal, Exponential};

    public interface IDistribution
    {

        DistributionType Name {get;set;}
        double Value1 {get;set;}
        double Value2 {get;set;}
    }

    /// <summary>
    /// Definition of a wind severity.
    /// </summary>
    public class Distribution
        : IDistribution
    {
        private DistributionType name;
        private double value1;
        private double value2;


        //---------------------------------------------------------------------
        public DistributionType Name
        {
            get {
                return name;
            }

            set {
                name = value;
            }
        }
        //---------------------------------------------------------------------
        public double Value1
        {
            get {
                return value1;
            }
            set {
                if (value < 0 || value > 100)
                        throw new InputValueException(value.ToString(), "Value must be between 0 and 100");
                value1 = value;
            }
        }
        //---------------------------------------------------------------------
        public double Value2
        {
            get {
                return value2;
            }
            set {
                if (value < 0.0 || value > 10.0)
                        throw new InputValueException(value.ToString(), "Value must be between 0.0 and 10.0");
                value2 = value;
            }
        }

        //---------------------------------------------------------------------

        public Distribution()
        {
        }

        //---------------------------------------------------------------------

        public static double GenerateRandomNum(DistributionType dist, double parameter1, double parameter2)
        {
            double randomNum = 0.0;
            if(dist == DistributionType.Normal)
            {
                PlugIn.ModelCore.NormalDistribution.Mu = parameter1; //mean
                PlugIn.ModelCore.NormalDistribution.Sigma = parameter2; // std dev
                if (parameter2 == 0)
                    randomNum = parameter1;
                else
                {
                    randomNum = PlugIn.ModelCore.NormalDistribution.NextDouble();
                    randomNum = PlugIn.ModelCore.NormalDistribution.NextDouble();
                }
            }
            if (dist == DistributionType.Exponential)
            {
                PlugIn.ModelCore.ExponentialDistribution.Lambda = parameter1;
                randomNum = PlugIn.ModelCore.ExponentialDistribution.NextDouble();
                randomNum = PlugIn.ModelCore.ExponentialDistribution.NextDouble();
            }
            if(dist == DistributionType.Weibull)
            {
                PlugIn.ModelCore.WeibullDistribution.Alpha = parameter1;// mean
                PlugIn.ModelCore.WeibullDistribution.Lambda = parameter2;// std dev
                randomNum = PlugIn.ModelCore.WeibullDistribution.NextDouble();
                randomNum = PlugIn.ModelCore.WeibullDistribution.NextDouble();
            }

            if(dist == DistributionType.Gamma)
            {
                PlugIn.ModelCore.GammaDistribution.Alpha = parameter1;// mean
                PlugIn.ModelCore.GammaDistribution.Theta = parameter2;// std dev
                randomNum = PlugIn.ModelCore.GammaDistribution.NextDouble();
                randomNum = PlugIn.ModelCore.GammaDistribution.NextDouble();
            }
            if(dist == DistributionType.Beta)
            {
                PlugIn.ModelCore.BetaDistribution.Alpha = parameter1;// mean
                PlugIn.ModelCore.BetaDistribution.Beta = parameter2;// std dev
                if (parameter1 == 0)
                    randomNum = 0;
                else if (parameter2 == 0)
                    randomNum = 1;
                else
                {
                    randomNum = PlugIn.ModelCore.BetaDistribution.NextDouble();
                    randomNum = PlugIn.ModelCore.BetaDistribution.NextDouble();
                }
            }
            return randomNum;
        }
    }
}
