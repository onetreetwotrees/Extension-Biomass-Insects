//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

using Landis.SpatialModeling;

namespace Landis.Extension.Insects
{
    public class UShortPixel : Pixel
    {
        public Band<ushort> MapCode = "The numeric code for each raster cell";

        public UShortPixel()
        {
            SetBands(MapCode);
        }
    }
}
