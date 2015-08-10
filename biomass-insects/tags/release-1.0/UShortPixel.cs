//  Copyright 2006 University of Wisconsin
//  Authors:  
//      Jane Foster
//      Robert M. Scheller
//  Version 1.0
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.RasterIO;

namespace Landis.Insects
{
    public class UShortPixel
        : SingleBandPixel<ushort>
    {
        public UShortPixel()
            : base()
        {
        }

        //---------------------------------------------------------------------

        public UShortPixel(ushort band0)
            : base(band0)
        {
        }
    }
}
