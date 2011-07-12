//  Copyright 2008-2010 University of Wisconsin, Portland State University
//  Authors:
//      Jane Foster
//      Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Ecoregions;
using Landis.Util;
using System.Collections.Generic;
using System.Text;

namespace Landis.Insects
{
    /// <summary>
    /// A parser that reads the extension parameters from text input.
    /// </summary>
    public class InputParameterParser
        : Landis.TextParser<IInputParameters>
    {
        public static IDataset EcoregionsDataset = null;

        //---------------------------------------------------------------------
        public override string LandisDataValue
        {
            get {
                return "Biomass Insects";
            }
        }

        //---------------------------------------------------------------------
        public InputParameterParser()
        {
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            ReadLandisDataVar();

            InputParameters parameters = new InputParameters();

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            //----------------------------------------------------------
            // Read in Map and Log file names.

            InputVar<string> mapNames = new InputVar<string>("MapNames");
            ReadVar(mapNames);
            parameters.MapNamesTemplate = mapNames.Value;

            InputVar<string> logFile = new InputVar<string>("LogFile");
            ReadVar(logFile);
            parameters.LogFileName = logFile.Value;

            //----------------------------------------------------------
            // Last, read in Insect File names,
            // then parse the data in those files into insect parameters.

            InputVar<string> insectFileName = new InputVar<string>("InsectInputFiles");
            ReadVar(insectFileName);

            List<IInsect> insectParameterList = new List<IInsect>();
            InsectParser insectParser = new InsectParser();

            IInsect insectParameters = Data.Load<IInsect>(insectFileName.Value,insectParser);
            insectParameterList.Add(insectParameters);

            while (!AtEndOfInput) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(insectFileName, currentLine);

                insectParameters = Data.Load<IInsect>(insectFileName.Value,insectParser);

                insectParameterList.Add(insectParameters);

                GetNextLine();

            }

            foreach(IInsect activeInsect in insectParameterList)
            {
                if(insectParameters == null)
                    UI.WriteLine("PARSE:  Insect Parameters NOT loading correctly.");
                else
                    UI.WriteLine("Name of Insect = {0}", insectParameters.Name);

            }
            parameters.ManyInsect = insectParameterList;

            return parameters; //.GetComplete();

        }
    }
}
