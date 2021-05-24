using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD
{
    public class ObjectSnap
    {
        public enum Type
        {
            Endpoint,
            Midpoint,
            Center,
            Node,
            Quadrant,
            Intersection,
            Insertion,
            Perpendicular,
            Tangent,
            Nearest,
            GeometricCenter,
            ApparentIntersection,
            Extension,
            Parallel,
            Disable
        }

        public static bool Endpoint
        {
            get
            {
                return Get(Type.Endpoint);
            }
            set
            {
                Set(Type.Endpoint, value);
            }
        }
        public static bool Midpoint;
        public static bool Center;
        public static bool Node;
        public static bool Quadrant;
        public static bool Intersection;
        public static bool Insertion;
        public static bool Perpendicular;
        public static bool Tangent;
        public static bool Nearest;
        public static bool GeometricCenter;
        public static bool ApparentIntersection;
        public static bool Extension;
        public static bool Parallel;
        public static bool IsEnabled;

        private static short Get()
        {
            return (short)Application.GetSystemVariable("OSMODE");
        }

        private static bool Get(Type type)
        {
            return new BitArray(new int[1]
            {
                Get()
            })[(int)type];
        }

        private static void Set(short value)
        {
            Application.SetSystemVariable("OSMODE", value);
        }

        private static void Set(Type type, bool value)
        {
            if (Get(type) != value)
                if (value)
                    Set((short)(Get() + 2 ^ ((int)type)));
                else
                {
                    var test = Get();
                    var test2 = 2 ^ (int)type;
                    test = (short)(test - (short)2 ^ (short)type);
                    Set((short)(test - 2 ^ ((int)type)));
                }
                    
        }

        public static void None()
        {
            Set(0);
        }
    }
}
