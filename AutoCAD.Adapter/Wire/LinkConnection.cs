using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Text.RegularExpressions;

namespace ICA.AutoCAD.Adapter
{
    public class LinkConnection : ConnectionPoint
    {
        #region Constructors

        public LinkConnection(BlockReference blockReference, AttributeDefinition definition) : base(blockReference)
        {
            Regex format = new Regex(@"X([1,2,4,8])LINK");
            Match match = format.Match(definition.Tag);
            if (!match.Success)
                throw new ArgumentException("Attribute definition is not formatted as a valid link connection.");
            WireDirection = (Orientation)int.Parse(match.Groups[1].ToString());
            Location = blockReference.Position.TransformBy(Matrix3d.Displacement(definition.Position.GetAsVector())).ToPoint2D();
        }

        #endregion
    }
}
