//  Copyright 2008-2010 University of Wisconsin, Portland State University
//  Authors:
//      Jane Foster
//      Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Insects
{
    //public enum DistributionType {Gamma};

    public interface ISusceptible
    {

        byte Number{get;set;}
        IDistribution Distribution_80 {get;set;}
        IDistribution Distribution_60 {get;set;}
        IDistribution Distribution_40 {get;set;}
        IDistribution Distribution_20 {get;set;}
        IDistribution Distribution_0 {get;set;}
    }

    /// <summary>
    /// Definition of a wind severity.
    /// </summary>
    public class Susceptible
        : ISusceptible
    {
        private byte number;
        private IDistribution distribution_80;
        private IDistribution distribution_60;
        private IDistribution distribution_40;
        private IDistribution distribution_20;
        private IDistribution distribution_0;


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
        public IDistribution Distribution_80
        {
            get {
                return distribution_80;
            }
            set {
                distribution_80 = value;
            }
        }
        //---------------------------------------------------------------------
        public IDistribution Distribution_60
        {
            get {
                return distribution_60;
            }
            set {
                distribution_60 = value;
            }
        }
        //---------------------------------------------------------------------
        public IDistribution Distribution_40
        {
            get {
                return distribution_40;
            }
            set {
                distribution_40 = value;
            }
        }
        //---------------------------------------------------------------------
        public IDistribution Distribution_20
        {
            get {
                return distribution_20;
            }
            set {
                distribution_20 = value;
            }
        }
        //---------------------------------------------------------------------
        public IDistribution Distribution_0
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

/*        public Susceptible(byte   number,
                        IDistribution distribution_80,
                        //double value1_80,
                        //double value2_80,
                        IDistribution distribution_60,
                        IDistribution distribution_40,
                        IDistribution distribution_20,
                        IDistribution distribution_0
                        )
        {
            this.number = number;
            this.distribution_80 = distribution_80;
            //this.value1 = value1;
            //this.value2 = value2;
            this.distribution_60 = distribution_60;
            this.distribution_40 = distribution_40;
            this.distribution_20 = distribution_20;
            this.distribution_0 = distribution_0;
        }*/
    }
}
