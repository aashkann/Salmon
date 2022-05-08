using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;


using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;
using Grasshopper.GUI;
using Rhino.DocObjects.Tables;
using System.ComponentModel;

namespace Salmon
{

   
    public class FlatOffset : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public FlatOffset()
          : base("Flat Offset", "FlatOff",
              "Offset surface edges",
              "Salmon", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surfaces", "Srfs", "Surfaces", GH_ParamAccess.list);
            pManager.AddNumberParameter("Frame Offset", "FOff", "Frame Offset", GH_ParamAccess.item, 200);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Offset Panel", "OffP", "Offset Panel", GH_ParamAccess.list);
            pManager.AddPointParameter("Centeroid", "C", "Centeroid", GH_ParamAccess.list);
            pManager.AddVectorParameter("Normals", "N", "Normals", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curves", "Cu", "Curves", GH_ParamAccess.tree);
            pManager.AddPointParameter("Corner Points", "CoPt", "Corner Points", GH_ParamAccess.tree);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Surface> srf = new List<Surface>();
            double frameOff = double.NaN;
            DA.GetDataList("Surfaces", srf);
            DA.GetData("Frame Offset", ref frameOff);

            List<Point3d> centerSrf = new List<Point3d>();
            List<Vector3d> normals = new List<Vector3d>();
            List<Point3d> output = new List<Point3d>();
            List<Surface> middlePanel = new List<Surface>();
            



            DataTree<Curve> allCurve = new DataTree<Curve>();
            List<CurveIntersections> intersectShift = new List<CurveIntersections>();

            //Rhino.RhinoDoc doc;
            int firstPath = 0;
            int surpath = 0;

            List<Surface> shrankSrf = new List<Surface>();
            
            
            //Surface surface;
            DataTree<Point3d> interPtTemp = new DataTree<Point3d>();


            foreach (Surface i in srf)
            {

                List<Point3d> interPt = new List<Point3d>();

                //Calculating Center and Normals
                AreaMassProperties srfProp = AreaMassProperties.Compute(i);
                Point3d tempCenter = srfProp.Centroid;
                i.SetDomain(0, new Interval(0, 1));
                i.SetDomain(0, new Interval(0, 1));
                Vector3d tempNormal = i.NormalAt(0.5, 0.5);
                centerSrf.Add(tempCenter);
                normals.Add(tempNormal);
                //middlePanel.Add(tempSrf);
                Brep srfNew = i.ToBrep();
                List<Curve> allCrvTemp = new List<Curve>(srfNew.DuplicateEdgeCurves());
                for (int k = 0; k < allCrvTemp.Count; k++)
                {
                    //Generating Path for Moved Curves
                    GH_Path curvePath = new GH_Path(firstPath, 0);

                    allCrvTemp[k].Domain = new Interval(0, 1);
                    Point3d curveCenter = allCrvTemp[k].PointAt(0.5);
                    Vector3d moveLines = tempCenter - curveCenter;
                    moveLines.Unitize();
                    allCrvTemp[k].Transform(Transform.Translation(moveLines * frameOff));

                    //Writing Curves In A Tree 
                    allCurve.Add(allCrvTemp[k], curvePath);
                }
                firstPath++;
                
                List<CurveIntersections> events = new List<CurveIntersections>();
                for (int intCount = 0; intCount < allCrvTemp.Count; intCount++)
                {
                    //GH_Path curvePathCount = new GH_Path(surpath);
                    if (intCount + 1 < allCrvTemp.Count) 
                    { 
                    events.Add(Rhino.Geometry.Intersect.Intersection.CurveCurve(allCrvTemp[intCount], allCrvTemp[intCount + 1], 0.0001, 0.0));
                    } else
                    {
                        events.Add(Rhino.Geometry.Intersect.Intersection.CurveCurve(allCrvTemp[allCrvTemp.Count - 1], allCrvTemp[0], 0.0001, 0.0));
                        
                        break;
                    }
                }

                
                    for (int n = 0; n < events.Count; n++)
                    {

                        GH_Path enumPath = new GH_Path(n % 4);
                        var cc_events = events[n][0];
                        Point3d npt = new Point3d(cc_events.PointA);
                        interPt.Add(npt);
                        interPtTemp.Add(npt, enumPath);

                    }

                
                shrankSrf.Add(
                    NurbsSurface.CreateFromCorners(
                        interPt[0], interPt[1], interPt[2], interPt[3]));
                surpath++;

            }



            DA.SetDataList(0, shrankSrf);
            DA.SetDataList(1, centerSrf);
            DA.SetDataList(2, normals);
            DA.SetDataTree(3 , allCurve);
            DA.SetDataTree(4, interPtTemp);

    

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
                return Properties.Resources.flatOffset; ;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a4c1d373-45ee-463c-ad5e-402e6e98bfdf"); }
        }
    }
}