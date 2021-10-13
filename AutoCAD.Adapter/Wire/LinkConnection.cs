using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Text.RegularExpressions;

namespace ICA.AutoCAD.Adapter
{
    public class LinkConnection : ConnectionPoint
    {
        #region Properties

        #region Public Properies

        public AttributeReference Reference { get; set; }

        #endregion

        #endregion

        #region Constructors

        public LinkConnection(AttributeReference reference)
        {
            Regex format = new Regex(@"X([1,2,4,8])LINK");
            Match match = format.Match(reference.Tag);
            if (!match.Success)
                throw new ArgumentException("Attribute reference is not formatted as a valid link connection.");
            WireDirection = (Orientation)int.Parse(match.Groups[1].ToString());
            Location = reference.GetPosition();
            Reference = reference;
        }

        #endregion
    }
}
