using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public interface IGraphNode<T>
    {
        T Value { get; set; }
        ICollection<IGraphNode<T>> Neighbors { get; }

        bool AddNeighbor(IGraphNode<T> neighbor);
        bool RemoveNeighbor(IGraphNode<T> neighbor);
    }
}
