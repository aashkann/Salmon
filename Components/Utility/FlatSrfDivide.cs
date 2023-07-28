using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Salmon.Components.Utility
{
    public class FlatSrfDivide : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public FlatSrfDivide()
          : base("FlatSrfDivide", "FlatSrfDivide",
            "A simple tessellation for a planar surface",
            TemplateConfig.SalmonTab, TemplateConfig.Tabs.Utility)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Conncet your planar surface", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Specify the width of each panel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width Spacing", "WS", "Specify the gap between horizontal panels", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Specify the height of each panel", GH_ParamAccess.item, 2000);
            pManager.AddNumberParameter("Height Spacing", "HS", "Specify the gap between vertical panels", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Panels", "Panels", "Panels", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Borders", "Borders", "Borders", GH_ParamAccess.list);
            pManager.AddIntervalParameter("UDomain", "U", "Intervals in the U direction", GH_ParamAccess.list);
            pManager.AddIntervalParameter("VDomain", "V", "Intervals in the V direction", GH_ParamAccess.list);
            pManager.AddInterval2DParameter("UVDoamin", "UV", "Intervals in 2D", GH_ParamAccess.list);
            pManager.AddIntegerParameter("UNumbers", "UNumbers", "UNumbers", GH_ParamAccess.item);
            pManager.AddIntegerParameter("VNumbers", "VNumbers", "VNumbers", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Setting up the Parameters

            Surface srf = null;
            double ULength = double.NaN;
            double VLength = double.NaN;
            double UGap = double.NaN;
            double VGap = double.NaN;
            DA.GetData(0, ref srf);
            DA.GetData("Width", ref ULength);
            DA.GetData("Height", ref VLength);
            DA.GetData("Width Spacing", ref UGap);
            DA.GetData("Height Spacing", ref VGap);

            Interval srfULength = srf.Domain(0);
            Interval srfVLength = srf.Domain(1);



            //Defining All the Arrays and Main Variables

            List<Interval> Udomain = new List<Interval>();
            List<Interval> Vdomain = new List<Interval>();
            List<UVInterval> UV = new List<UVInterval>();
            List<Surface> IsoSrf = new List<Surface>();
            List<Surface> Panels = new List<Surface>();
            List<Surface> Borders = new List<Surface>();
            List<double> countt = new List<double>();

            int nMax;
            int mMax;



            // Calculating Surface
            double remainderU = srfULength[1] % (ULength + UGap);
            int equalU = Convert.ToInt32(Math.Floor(srfULength[1] / (ULength + UGap)));
            double remainderV = srfVLength[1] % (VLength + VGap);
            int equalV = Convert.ToInt32(Math.Floor(srfVLength[1] / (VLength + VGap)));




            //Writing U values
            for (int j = 0; j < equalU; j++)
            {
                // Creating Gaps
                double Udown = j * (ULength + UGap);
                double Uup = j * (ULength + UGap) + UGap;
                Interval tempU = new Interval(Udown, Uup);
                Udomain.Add(tempU);
                // Creating Panels
                Udown = j * (ULength + UGap) + UGap;
                Uup = (j + 1) * (ULength + UGap);
                tempU = new Interval(Udown, Uup);
                Udomain.Add(tempU);
                countt.Add(j);
            }



            //Writing V values
            for (int k = 0; k < equalV; k++)
            {
                // Creating Gaps
                double Vdown = k * (VLength + VGap);
                double Vup = k * (VLength + VGap) + VGap;
                Interval tempV = new Interval(Vdown, Vup);
                Vdomain.Add(tempV);
                //Creating Panels
                Vdown = k * (VLength + VGap) + VGap;
                Vup = (k + 1) * (VLength + VGap);
                tempV = new Interval(Vdown, Vup);
                Vdomain.Add(tempV);
            }


            //Calculating the Numbers of Panels
            double netU = equalU * (ULength + UGap);
            double remainderUU = equalU * (ULength + UGap) + remainderU;
            double netV = equalV * (VLength + VGap);
            double remainderVV = equalV * (VLength + VGap) + remainderV;

            //Evaluating Whether Remainders are Not Zero
            if (remainderU != 0)
            {
                Interval leftU = new Interval(netU, remainderUU);
                Udomain.Add(leftU);
            }
            if (remainderV != 0)
            {
                Interval leftV = new Interval(netV, remainderVV);
                Vdomain.Add(leftV);
            }



            //Calculating Numbers of Panels in Each Directions
            if (remainderU != 0)
            {
                nMax = 2 * equalU + 1;
            }
            else
            {
                nMax = 2 * equalU;

            }
            if (remainderV != 0)
            {
                mMax = 2 * equalV + 1;
            }
            else
            {
                mMax = 2 * equalV;
            }

            //Trimming the Surface According to Generated Domains
            for (int n = 0; n < nMax; n++)
            {
                for (int m = 0; m < mMax; m++)
                {
                    UVInterval tempUV = new UVInterval(Udomain[n], Vdomain[m]);
                    Surface tempSrf = srf.Trim(Udomain[n], Vdomain[m]);
                    UV.Add(tempUV);
                    IsoSrf.Add(tempSrf);


                }
            }

            //Writing Panels and Borders Into Different Arrays
            foreach (Surface i in IsoSrf)
            {
                var tolerance = 0.01;
                AreaMassProperties measArea = AreaMassProperties.Compute(i);
                if (measArea.Area <= ULength * VLength + tolerance && measArea.Area >= ULength * VLength - tolerance)
                {
                    Panels.Add(i);
                }
                else
                {
                    Borders.Add(i);
                }
            }


            //Writing Results to The Outputs
            DA.SetDataList("UDomain", Udomain);
            DA.SetDataList("VDomain", Vdomain);
            DA.SetDataList("UVDoamin", UV);
            DA.SetDataList("Panels", Panels);
            DA.SetDataList("Borders", Borders);
            DA.SetDataList("UNumbers", nMax.ToString());
            DA.SetDataList("VNumbers", mMax.ToString());
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E8A03466-4485-4E99-99A2-720516106E56"); }
        }
    }
}