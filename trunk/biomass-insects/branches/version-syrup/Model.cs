using Landis.Biomass;
using System;

namespace Landis.Insects
{
    internal static class Model
    {
        private static PlugIns.ICore core;
        private static ILandscapeCohorts cohorts;

        //---------------------------------------------------------------------

        internal static PlugIns.ICore Core
        {
            get {
                return core;
            }

            set {
                core = value;
                if (value != null) {
                    cohorts = core.SuccessionCohorts as ILandscapeCohorts;
                    if (cohorts == null)
                        throw new ApplicationException("Cohorts don't support biomass-cohort interface");
                }
            }
        }

        //---------------------------------------------------------------------

        internal static ILandscapeCohorts LandscapeCohorts
        {
            get {
                return cohorts;
            }
        }
    }
}
