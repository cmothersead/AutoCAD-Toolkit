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
            get => Get(Type.Endpoint);
            set => Set(Type.Endpoint, value);
        }
        public static bool Midpoint
        {
            get => Get(Type.Midpoint);
            set => Set(Type.Midpoint, value);
        }
        public static bool Center
        {
            get => Get(Type.Center);
            set => Set(Type.Center, value);
        }
        public static bool Node
        {
            get => Get(Type.Node);
            set => Set(Type.Node, value);
        }
        public static bool Quadrant
        {
            get => Get(Type.Quadrant);
            set => Set(Type.Quadrant, value);
        }
        public static bool Intersection
        {
            get => Get(Type.Intersection);
            set => Set(Type.Intersection, value);
        }
        public static bool Insertion
        {
            get => Get(Type.Insertion);
            set => Set(Type.Insertion, value);
        }
        public static bool Perpendicular
        {
            get => Get(Type.Perpendicular);
            set => Set(Type.Perpendicular, value);
        }
        public static bool Tangent
        {
            get => Get(Type.Tangent);
            set => Set(Type.Tangent, value);
        }
        public static bool Nearest
        {
            get => Get(Type.Nearest);
            set => Set(Type.Nearest, value);
        }
        public static bool GeometricCenter
        {
            get => Get(Type.GeometricCenter);
            set => Set(Type.GeometricCenter, value);
        }
        public static bool ApparentIntersection
        {
            get => Get(Type.ApparentIntersection);
            set => Set(Type.ApparentIntersection, value);
        }
        public static bool Extension
        {
            get => Get(Type.Extension);
            set => Set(Type.Extension, value);
        }
        public static bool Parallel
        {
            get => Get(Type.Parallel);
            set => Set(Type.Parallel, value);
        }
        public static bool IsEnabled
        {
            get => !Get(Type.Disable);
            set => Set(Type.Disable, !value);
        }

        public static short Value
        {
            get => Get();
            set => Set(value);
        }

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
                    Set((short)(Get() + Math.Pow(2,(int)type)));
                else
                {
                    Set((short)(Get() - Math.Pow(2, (int)type)));
                }
        }

        public static void None()
        {
            Set(0);
        }
    }
}
