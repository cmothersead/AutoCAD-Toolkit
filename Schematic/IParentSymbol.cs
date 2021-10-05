
namespace ICA.Schematic
{
    public interface IParentSymbol : ISymbol, IHideableAttributes
    {
        void UpdateTag(string format);
    }
}