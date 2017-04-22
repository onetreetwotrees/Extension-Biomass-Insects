using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;
using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using System.IO;
using Flel = Edu.Wisc.Forest.Flel;

namespace Landis.Extension.Insects
{
    public static class MetadataHandler
    {
        public static ExtensionMetadata Extension { get; set; }

        public static void InitializeMetadata(int Timestep, string eventLogName)
        {

            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata()
            {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
            };

            Extension = new ExtensionMetadata(PlugIn.ModelCore)
            //Extension = new ExtensionMetadata()
            {
                Name = PlugIn.ExtensionName,
                TimeInterval = PlugIn.ModelCore.CurrentTime, //change this to PlugIn.TimeStep for other extensions
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

            CreateDirectory(eventLogName);
            PlugIn.eventLog = new MetadataTable<EventsLog>(eventLogName);

            PlugIn.ModelCore.UI.WriteLine("   Generating event table...");
            OutputMetadata tblOut_event = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "InsectEventsLog",
                FilePath = PlugIn.eventLog.FilePath,
                Visualize = true,
            };
            tblOut_event.RetriveFields(typeof(EventsLog));
            Extension.OutputMetadatas.Add(tblOut_event);

            //2 kinds of maps: species and pool maps, maybe multiples of each?
            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            //OutputMetadata mapOut_BiomassRemoved = new OutputMetadata()
            //{
            //    Type = OutputType.Map,
            //    Name = "biomass removed",
            //    FilePath = @HarvestMapName,
            //    Map_DataType = MapDataType.Continuous,
            //    Map_Unit = FieldUnits.Mg_ha,
            //    Visualize = true,
            //};
            //Extension.OutputMetadatas.Add(mapOut_BiomassRemoved);


            foreach (IInsect insect in PlugIn.insects)
            {
                OutputMetadata mapOut_Growth = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = insect.Name,
                    FilePath = MapNames.ReplaceTemplateVars(PlugIn.mapNames,
                                                       insect.Name, Timestep),
                    Map_DataType = MapDataType.Continuous,
                    Visualize = true,
                    //Map_Unit = "categorical",
                };
                Extension.OutputMetadatas.Add(mapOut_Growth);

                OutputMetadata mapOut_Patch = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = "InitialPatchMap-" + insect.Name,
                    FilePath = MapNames.ReplaceTemplateVars(PlugIn.mapNames,
                                       "InitialPatchMap-" + insect.Name, Timestep),
                    Map_DataType = MapDataType.Continuous,
                    Visualize = true,
                    //Map_Unit = "categorical",
                };
                Extension.OutputMetadatas.Add(mapOut_Patch);

                OutputMetadata mapOut_Biomass = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = "BiomassRemoved-" + insect.Name,
                    FilePath = MapNames.ReplaceTemplateVars(PlugIn.mapNames,
                       "BiomassRemoved-" + insect.Name, Timestep),
                    Map_DataType = MapDataType.Continuous,
                    Visualize = true,
                    //Map_Unit = "categorical",
                };
                Extension.OutputMetadatas.Add(mapOut_Biomass);
            }

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);
        }
        public static void CreateDirectory(string path)
        {
            //Require.ArgumentNotNull(path);
            path = path.Trim(null);
            if (path.Length == 0)
                throw new ArgumentException("path is empty or just whitespace");

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Flel.Util.Directory.EnsureExists(dir);
            }

            //return new StreamWriter(path);
            return;
        }
    }
}