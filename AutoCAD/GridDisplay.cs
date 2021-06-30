namespace ICA.AutoCAD
{
    public static class GridDisplay
    {
        /// <summary>
        /// Restricts the viewport grid to the specified limits when set.
        /// </summary>
        public static bool Limits
        {
            get => !SystemVariable.Get(0);
            set => SystemVariable.Set(0, !value);
        }

        private static Bitcode SystemVariable => new Bitcode("GRIDDISPLAY");
    }
}
