//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;

namespace Landis.Extension.Insects
{
    public enum DistributionType {Gamma, Beta, Weibull};

    public interface ISusceptible
    {

        byte Number{get;set;}
        DistributionType Distribution_80 {get;set;}
        DistributionType Distribution_60 {get;set;}
        DistributionType Distribution_40 {get;set;}
        DistributionType Distribution_20 {get;set;}
        DistributionType Distribution_0 {get;set;}
    }

    /// <summary>
    /// Definition of a wind severity.
    /// </summary>
    public class Susceptible
        : ISusceptible
    {
        private byte number;
        private DistributionType distribution_80;
        private DistributionType distribution_60;
        private DistributionType distribution_40;
        private DistributionType distribution_20;
        private DistributionType distribution_0;


        //---------------------------------------------------------------------

        /// <summary>
        /// The severity's number (between 1 and 254).
        /// </summary>
        public byte Number
        {
            get {
                return number;
            }
            set {
                if (value > 5)
                        throw new InputValueException(value.ToString(), "Value must be between 1 and 5.");
                number = value;
            }
        }

        //---------------------------------------------------------------------
        public DistributionType Distribution_80
        {
            get {
                return distribution_80;
            }
            set {
                distribution_80 = value;
            }
        }
        //---------------------------------------------------------------------
        public DistributionType Distribution_60
        {
            get {
                return distribution_60;
            }
            set {
                distribution_60 = value;
            }
        }
        //---------------------------------------------------------------------
        public DistributionType Distribution_40
        {
            get {
                return distribution_40;
            }
            set {
                distribution_40 = value;
            }
        }
        //---------------------------------------------------------------------
        public DistributionType Distribution_20
        {
            get {
                return distribution_20;
            }
            set {
                distribution_20 = value;
            }
        }
        //---------------------------------------------------------------------
        public DistributionType Distribution_0
        {
            get {
                return distribution_0;
            }
            set {
                distribution_0 = value;
            }
        }
        //---------------------------------------------------------------------

        public Susceptible()
        {
        }

    }
}
