﻿namespace ICA.Schematic
{
    public interface IChildSymbol : ISymbol, IHideableAttributes
    {
        IParentSymbol SelectParent();
    }
}
