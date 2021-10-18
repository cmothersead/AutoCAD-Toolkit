using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
