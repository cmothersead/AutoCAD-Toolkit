using System;

namespace ICA.AutoCAD
{
    [Flags]
    public enum GridDisplay
    {
        /// <summary>
        /// Displays grid beyond the document's specified LIMITS.
        /// </summary>
        BeyondLimits = 1,
        /// <summary>
        /// Adjusts the density of the grid lines when zoomed out.
        /// </summary>
        Adaptive = 2,
        /// <summary>
        /// Adjusts the density of the grid lines when zoomed in or out.
        /// </summary>
        AdaptiveZoomIn = 6,
        /// <summary>
        /// Changes the grid plane to follow the XY plane of the dynamic UCS.
        /// </summary>
        UCS = 8
    }
}
