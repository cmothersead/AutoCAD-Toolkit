using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public static class LineExtensions
    {
        public static List<Line> GetConnected(this Line line, List<Line> input)
        {
            List<Line> output = new List<Line>()
            {
                line
            };
            foreach(Line potential in input)
                if (line.IntersectsWith(potential))
                    output.Add(potential);
            List<Line> toAdd = new List<Line>();
            foreach (Line confirmed in output)
                if (confirmed != line)
                    toAdd = toAdd.Union(confirmed.GetConnected(input.Except(output).ToList())).ToList(); ;
            return output.Union(toAdd).ToList();
        }
    }
}
