using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Flow
{
    internal class Graph
    {
        public Node[,] Nodes;
        public Edge[,] HorizontalEdges;
        public Edge[,] VerticalEdges;

        public Node[] EndpointNodes;
        public int NumEndpointNodes;

        public Edge[] PortalEdges;
        public int NumPortalEdges;
        public Graph()
        {
            Nodes = new Node[Flow.GraphDim, Flow.GraphDim];
            HorizontalEdges = new Edge[Flow.GraphDim, Flow.GraphDim + 1];
            VerticalEdges = new Edge[Flow.GraphDim + 1, Flow.GraphDim];
            for (int i = 0; i <  Flow.GraphDim; i++)
            {
                for (int j = 0; j < Flow.GraphDim; j++)
                {
                    Nodes[i, j] = new Node(i, j);
                    HorizontalEdges[i, j] = new Edge(i, j, i + 1, j);
                    VerticalEdges[i, j] = new Edge(i, j, i, j + 1);
                }
            }
            for (int i = 0; i < Flow.GraphDim; i++) //bottom edges
            {
                HorizontalEdges[i, Flow.GraphDim] = new Edge(i, Flow.GraphDim, i + 1, Flow.GraphDim);
            }
            for (int j = 0; j < Flow.GraphDim; j++) //right edges
            {
                VerticalEdges[Flow.GraphDim, j] = new Edge(Flow.GraphDim, j, Flow.GraphDim, j + 1);
            }

            EndpointNodes = new Node[Flow.Colors.Length * 2];
            NumEndpointNodes = 0;
            PortalEdges = new Edge[Flow.Colors.Length * 2];
            NumPortalEdges = 0;
        }

        public void Update()
        {
            Input.KeyboardInputType keyInput = Input.GetKeyboardInputType();
            if (keyInput == Input.KeyboardInputType.Endpoint || keyInput == Input.KeyboardInputType.Bridge || keyInput == Input.KeyboardInputType.Gone) //nodes
            {
                if (!Input.IsClickingOnNode()) return;

                (int, int) nodeCoordinates = Input.NodeCoordinates();
                Node node = Nodes[nodeCoordinates.Item1, nodeCoordinates.Item2];

                if (node == null || node.Type != Node.NodeType.Standard) return;

                if (keyInput == Input.KeyboardInputType.Endpoint)
                {
                    node.Type = Node.NodeType.Endpoint;
                    node.ColorIndex = NumEndpointNodes / 2;
                    EndpointNodes[NumEndpointNodes] = node;
                    NumEndpointNodes++;
                }
                else if (keyInput == Input.KeyboardInputType.Bridge)
                {
                    node.Type = Node.NodeType.Bridge;
                }
                else // Gone
                {
                    int x = node.X;
                    int y = node.Y;
                    Nodes[nodeCoordinates.Item1, nodeCoordinates.Item2] = null;
                    EdgeRemovals(x, y);
                }
            }
            else if (keyInput == Input.KeyboardInputType.Wall || keyInput == Input.KeyboardInputType.Portal) //edges
            {
                if (!Input.IsClickingOnEdge()) return;

                ((int, int), (int, int)) edgePair = Input.EdgeCoordinates();
                Edge edge;
                if (edgePair.Item1.Item1 + 1 == edgePair.Item2.Item1) //horizontal
                {
                    edge = HorizontalEdges[edgePair.Item1.Item1, edgePair.Item1.Item2];
                }
                else
                {
                    edge = VerticalEdges[edgePair.Item1.Item1, edgePair.Item1.Item2];
                }

                if (edge == null || edge.Type != Edge.EdgeType.Standard) return;

                if (keyInput == Input.KeyboardInputType.Wall)
                {
                    edge.Type = Edge.EdgeType.Wall;
                }
                else // Portal
                {
                    edge.Type = Edge.EdgeType.Portal;
                    edge.ColorIndex = NumPortalEdges / 2;
                    PortalEdges[NumPortalEdges] = edge;
                    NumPortalEdges++;
                }
            }
        }

        private void EdgeRemovals(int x, int y)
        {
            if (1 <= x)
            {
                if (Nodes[x - 1, y] == null) VerticalEdges[x, y] = null;
            }
            if (x < Flow.GraphDim - 1)
            {
                if (Nodes[x + 1, y] == null) VerticalEdges[x + 1, y] = null;
            }

            if (1 <= y)
            {
                if (Nodes[x, y - 1] == null) HorizontalEdges[x, y] = null;
            }
            if (y < Flow.GraphDim - 1)
            {
                if (Nodes[x, y + 1] == null) HorizontalEdges[x, y + 1] = null;
            }
        }

        public void Draw()
        {
            foreach (Node node in Nodes)
            {
                if (node != null) node.Draw();
            }
            foreach(Edge edge in HorizontalEdges)
            {
                if (edge != null) edge.Draw();
            }
            foreach(Edge edge in VerticalEdges)
            {
                if (edge != null) edge.Draw();
            }
        }
    }
}
