using System;

namespace ICA.AutoCAD.Adapter
{
    public class WDPProperty
    {
        public char Type { get; set; }
        public int Number { get; set; }
        public string Value { get; set; }

        public WDPProperty(string line)
        {
            switch (line[0])
            {
                case '+':
                    Type = '+';
                    break;
                case '?':
                    Type = '?';
                    break;
                default:
                    throw new ArgumentException("Line is not a valid AutoCAD Electrical Project property");
            }
            int startIndex = line.IndexOf('[') + 1;
            int endIndex = line.IndexOf(']');
            int length = endIndex - startIndex;
            Number = int.Parse(line.Substring(startIndex, length));
            Value = line.Substring(endIndex + 1);
        }
    }
}
