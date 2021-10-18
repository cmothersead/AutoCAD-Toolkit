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

        public ICollection<IGraphNode<Entity>> Neighbors => Value.XData?.Cast<TypedValue>()
                                                                       .Where(value => value.TypeCode == (int)DxfCode.ExtendedDataHandle)
                                                                       .Select(value => new EntityNode(Value.Database.OpenHandleString(value.Value as string) as Entity))
                                                                       .Cast<IGraphNode<Entity>>()
                                                                       .ToList();

        public EntityNode(Entity entity)
        {
            Value = entity;
        }

        public bool AddNeighbor(IGraphNode<Entity> neighbor) => AddNeighbor(neighbor.Value);

        public bool AddNeighbor(Entity neighbor)
        {
            ResultBuffer buffer = new ResultBuffer
            {
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, "ICA"),
                new TypedValue((int)DxfCode.ExtendedDataHandle, neighbor.Handle)
            };
            using (Transaction transaction = neighbor.Database.TransactionManager.StartTransaction())
            {
                neighbor.SetXData(transaction, buffer);
                transaction.Commit();
            }
            return neighbor.XData == buffer;
        }

        public bool RemoveNeighbor(IGraphNode<Entity> neighbor) => throw new NotImplementedException();

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
