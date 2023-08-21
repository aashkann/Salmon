using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino;
using Salmon.Properties;
namespace Salmon.Components.Export
{
    public class ExportIllustrator : GH_Component
    {
        public ExportIllustrator()
          : base("Export to Illustrator", "ExportIll",
            "Export curves to individual Illustrator files",
            TemplateConfig.SalmonTab, TemplateConfig.Tabs.Export)
        {
        }
        protected override Bitmap Icon => Resources.ic_exportIllustrator;
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "C", "Curves to export", GH_ParamAccess.list);
            pManager.AddTextParameter("Path", "P", "Folder to save files", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Export", "E", "Press to export curves", GH_ParamAccess.item);


        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)

        {
            

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> curves = new List<Curve>();
            string folderPath = "";
            bool export = false;

            // Retrieve input data
            if (!DA.GetDataList(0, curves)) return;
            if (!DA.GetData(1, ref folderPath)) return;
            if (!DA.GetData(2, ref export)) return;

            if (export)
            {
                
                // Check if the folder path exists
                if (!Directory.Exists(folderPath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Folder does not exist.");
                    return;
                }

                // Loop through the curves and export them
                for (int i = 0; i < curves.Count; i++)
                {
                    Curve curve = curves[i];

                    // Generate a unique filename
                    string fileName = Path.Combine(folderPath, "Curve_" + i + ".ai");

                    // Create a copy of the curve for export
                    Curve exportCurve = curve.DuplicateCurve();

                    // Add the exportCurve to the Rhino document
                    Guid curveId = RhinoDoc.ActiveDoc.Objects.AddCurve(exportCurve);

                    // Select the curve by its ID
                    RhinoDoc.ActiveDoc.Objects.Select(curveId, true);

                    // Use RhinoScript methods to export the selected object as an Illustrator file
                    string exportScript = string.Format("_-Export \"{0}\" _Enter", fileName);
                    Rhino.RhinoApp.RunScript(exportScript, false);

                    // Clear the selection
                    RhinoDoc.ActiveDoc.Objects.UnselectAll();

                    // Delete the temporary exportCurve from the document
                    RhinoDoc.ActiveDoc.Objects.Delete(curveId, true);

                    // Optionally, you might want to add some error checking for the export process
                    
                }
            }
        }

      
            

        public override GH_Exposure Exposure => GH_Exposure.primary;


        public override Guid ComponentGuid
        {
            get { return new Guid("0447AA78-A808-4EC9-A2B4-3B00B6D73C7D"); }
        }
    }
}