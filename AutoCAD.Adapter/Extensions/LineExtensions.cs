using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public static class LineExtensions
    {
        #region Public Extension Methods

        public static List<Line> GetConnected(this Line line, IEnumerable<Line> input)
        {
            IEnumerable<Line> output = new List<Line> { line };

            output = output.Union(input.Where(potential => line.IntersectsWith(potential)));
            
            return output.Union(output.Where(confirmed => confirmed != line)
                         .SelectMany(confirmed => confirmed.GetConnected(input.Except(output))))
                         .ToList();
        }

        #endregion
    }
}
