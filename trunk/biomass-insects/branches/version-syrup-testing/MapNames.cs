//  Copyright 2007 University of Wisconsin-Madison
//  Authors:  
//      Robert M. Scheller
//      Jane R. Foster
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Species;
using System.Collections.Generic;

namespace Landis.Insects
{
    /// <summary>
    /// Methods for working with the template for map filenames.
    /// </summary>
    public static class MapNames
    {
        public const string InsectNameVar = "insectName";
        public const string TimestepVar = "timestep";

        private static IDictionary<string, bool> knownVars;
        private static IDictionary<string, string> varValues;

        //---------------------------------------------------------------------
        static MapNames()
        {
            knownVars = new Dictionary<string, bool>();
            knownVars[InsectNameVar] = true;
            knownVars[TimestepVar] = true;

            varValues = new Dictionary<string, string>();
        }

        //---------------------------------------------------------------------
        public static void CheckTemplateVars(string template)
        {
            OutputPath.CheckTemplateVars(template, knownVars);
        }

        //---------------------------------------------------------------------
        public static string ReplaceTemplateVars(string template,
                                                 string insectName,
                                                 int    timestep)
        {
            varValues[InsectNameVar] = insectName;
            varValues[TimestepVar] = timestep.ToString();
            return OutputPath.ReplaceTemplateVars(template, varValues);
        }
        //---------------------------------------------------------------------
        public static string ReplaceTemplateVars(string template,
                                                 string insectName)
        {
            varValues[InsectNameVar] = insectName;
            return OutputPath.ReplaceTemplateVars(template, varValues);
        }
    }
}
