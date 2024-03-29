﻿using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public class Graph<TNode, TData> where TNode : IGraphNode<TData>
    {
        public List<TNode> Nodes { get; set; } = new List<TNode>();

        public Graph() { }

        public Graph(TNode first) => DiscoverNodes(first);

        public TNode AddNode(TNode value)
        {
            if (!Contains(value))
            {
                Nodes.Add(value);
                value.Neighbors.ForEach(neighbor => AddEdge(value, neighbor));
            }
            return Nodes[Nodes.IndexOf(value)];
        }

        public void RemoveNode(TNode value)
        {
            if (!Contains(value))
                return;

            value.Neighbors.ForEach(neighbor => RemoveEdge(value, neighbor));

            Nodes.Remove(value);
        }

        public bool AddEdge(IGraphNode<TData> node1, IGraphNode<TData> node2) => node1.AddNeighbor(node2) && node2.AddNeighbor(node1);

        public bool RemoveEdge(IGraphNode<TData> node1, IGraphNode<TData> node2) => node1.RemoveNeighbor(node2) && node2.RemoveNeighbor(node1);

        public bool Contains(TNode value) => Nodes.Contains(value);

        private void DiscoverNodes(TNode value)
        {
            if (!Contains(value))
                Nodes.Add(value);

            var neighbors = value.Neighbors.ToList();
            foreach (TNode neighbor in neighbors)
            {
                if (!Contains(neighbor))
                    DiscoverNodes(neighbor);
            }
        }
    }
}
