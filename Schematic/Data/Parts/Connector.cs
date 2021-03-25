﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    public class Connector : IConnector
    {
        public string Type { get; set; }
        public int Pins { get; set; }
        Gender IConnector.Gender { get; set; }
        Angle IConnector.Angle { get; set; }

        public enum Gender
        {
            Male,
            Female
        }

        public enum Angle
        {
            Straight,
            RightAngle
        }
    }
}
