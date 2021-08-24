using System;

namespace ICA.AutoCAD
{
    [Flags]
    public enum ObjectSnap
    {
        Endpoint = 1,
        Midpoint = 2,
        Center = 4,
        Node = 8,
        Quadrant = 16,
        Intersection = 32,
        Insertion = 64,
        Perpendicular = 128,
        Tangent = 256,
        Nearest = 512,
        GeometricCenter = 1024,
        ApparentIntersection = 2048,
        Extension = 4096,
        Parallel = 8192,
        Disable = 16384
    }
}
