using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class EntityNode : IGraphNode<Entity>
    {
        public Entity Value { get; set; }

        public ICollection<IGraphNode<Entity>> Neighbors => Value.GetXData()
                                                                 .Where(data => data.TypeCode == DxfCode.ExtendedDataHandle)
                                                                 .Select(data => new EntityNode(Value.Database.OpenHandleString((string)data.Value) as Entity))
                                                                 .Cast<IGraphNode<Entity>>()
                                                                 .ToList();

        public EntityNode(Entity entity)
        {
            Value = entity;
        }

        public bool AddNeighbor(IGraphNode<Entity> neighbor) => AddNeighbor(neighbor.Value);

        public bool AddNeighbor(Entity neighbor)
        {
            if (!Value.HasXData(DxfCode.ExtendedDataHandle, neighbor.Handle))
                Value.AddXData(DxfCode.ExtendedDataHandle, neighbor.Handle);
            return Value.HasXData(DxfCode.ExtendedDataHandle, neighbor.Handle);
        }

        public bool RemoveNeighbor(IGraphNode<Entity> neighbor) => RemoveNeighbor(neighbor.Value);

        public bool RemoveNeighbor(Entity neighbor)
        {
            Value.RemoveXData(DxfCode.ExtendedDataHandle, neighbor.Handle);
            return !Value.HasXData(DxfCode.ExtendedDataHandle, neighbor.Handle.Value);
        }

        public static bool AddEdge(IGraphNode<Entity> node1, IGraphNode<Entity> node2) => node1.AddNeighbor(node2) && node2.AddNeighbor(node1);

        public static bool RemoveEdge(IGraphNode<Entity> node1, IGraphNode<Entity> node2) => node1.RemoveNeighbor(node2) && node2.RemoveNeighbor(node1);

        public override int GetHashCode() => Value.ObjectId.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is EntityNode entityNode)
                return Value.ObjectId == entityNode.Value.ObjectId;

            return false;
        }
    }
}
