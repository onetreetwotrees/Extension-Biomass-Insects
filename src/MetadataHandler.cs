using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;

namespace Landis.Extension.Insects
{
    public static class MetadataHandler
    {
        
        public static ExtensionMetadata Extension {get; set;}

        public static void InitializeMetadata(int Timestep, 
            string MapFileName, 
            string logFileName, 
            IEnumerable<IInsect> insects,
            ICore mCore)
        {
            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata() {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
            };

            Extension = new ExtensionMetadata(mCore){
                Name = PlugIn.ExtensionName,
                TimeInterval = 1, //Timestep, //change this to PlugIn.TimeStep for other extensions
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

             PlugIn.eventLog = new MetadataTable<EventsLog>("insects-log.csv");

            OutputMetadata tblOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "EventsLog",
                FilePath = PlugIn.eventLog.FilePath,
                Visualize = false,
            };
            tblOut_events.RetriveFields(typeof(EventsLog));
            Extension.OutputMetadatas.Add(tblOut_events);


            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            foreach (IInsect insect in insects)
            {

                string mapPath = MapNames.ReplaceTemplateVarsMetadata(MapFileName, insect.Name);
                OutputMetadata mapOut_GrowthReduction = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = "Growth Reduction",
                    FilePath = @mapPath,
                    Map_DataType = MapDataType.Continuous,
                    Map_Unit = FieldUnits.Percentage,
                    Visualize = false,
                };
                Extension.OutputMetadatas.Add(mapOut_GrowthReduction);

                mapPath = MapNames.ReplaceTemplateVarsMetadata(MapFileName, ("InitialPatchMap-" + insect.Name));
                OutputMetadata mapOut_InitialPatchProb = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = "Initial Outbreak Probabilities",
                    FilePath = @mapPath,
                    Map_DataType = MapDataType.Continuous,
                    Map_Unit = FieldUnits.Percentage,
                    Visualize = false,
                };
                Extension.OutputMetadatas.Add(mapOut_InitialPatchProb);

                mapPath = MapNames.ReplaceTemplateVarsMetadata(MapFileName, ("BiomassRemoved-" + insect.Name));
                OutputMetadata mapOut_BiomassRemoved = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = System.String.Format(insect.Name + " Biomass Mortality"),
                    FilePath = @mapPath,
                    Map_DataType = MapDataType.Continuous,
                    Map_Unit = FieldUnits.Mg_ha,
                    Visualize = true,
                };
                Extension.OutputMetadatas.Add(mapOut_BiomassRemoved);
            }
            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);




        }
    }
}
