using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Salmon
{
    public class SalmonInfo : GH_AssemblyInfo
    {
        public override string Name => "Salmon";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("392bf32e-413a-4f1a-9839-11b5252fb8ab");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}