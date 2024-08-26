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
        private Node[,] _nodes;
        public Node getNode(int x, int y) { return _nodes[x, y]; }
        private Edge[,] _horizontalEdges;
        private Edge[,] _verticalEdges;
        public Edge getEdge(int x1, int y1, int x2, int y2)
        {
            if (x1 + 1 == x2)
            {
                return _verticalEdges[x2, y2];
            }
            else if (y1 + 1 == y2)
            {
                return _horizontalEdges[x2, y2];
            }
            else return null;
        }

        public Node[] EndpointNodes;
        public int NumEndpointNodes;

        public Edge[] PortalEdges;
        public int NumPortalEdges;
        public Graph()
        {
            _nodes = new Node[Flow.GraphDim, Flow.GraphDim];
            _horizontalEdges = new Edge[Flow.GraphDim, Flow.GraphDim + 1];
            _verticalEdges = new Edge[Flow.GraphDim + 1, Flow.GraphDim];

            for (int i = 0; i < Flow.GraphDim; i++)
            {
                for (int j = 0; j < Flow.GraphDim; j++)
                {
                    _nodes[i, j] = new Node(i, j);
                }
            }

            for (int i = 0; i < Flow.GraphDim; i++)
            {
                for (int j = 0; j < Flow.GraphDim + 1; j++)
                {
                    _horizontalEdges[i, j] = new Edge(i, j - 1, i, j);
                    _verticalEdges[j, i] = new Edge(j - 1, i, j, i);
                }
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
                Node node = _nodes[nodeCoordinates.Item1, nodeCoordinates.Item2];

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
                    _nodes[nodeCoordinates.Item1, nodeCoordinates.Item2] = null;
                    EdgeRemovals(x, y);
                }
            }
            else if (keyInput == Input.KeyboardInputType.Wall || keyInput == Input.KeyboardInputType.Portal) //edges
            {
                if (!Input.IsClickingOnEdge()) return;

                (int, int, int, int) edgePair = Input.EdgeCoordinates();
                Edge edge;
                if (edgePair.Item1 + 1 == edgePair.Item3) //vertical edge
                {
                    edge = _verticalEdges[edgePair.Item3, edgePair.Item4];
                }
                else //horizontal edge
                {
                    edge = _horizontalEdges[edgePair.Item3, edgePair.Item4];
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
            //left
            if (x == 0 || _nodes[x - 1, y] == null) _verticalEdges[x, y] = null;
            //right
            if (x == Flow.GraphDim - 1 || _nodes[x + 1, y] == null) _verticalEdges[x + 1, y] = null;
            //top
            if (y == 0 || _nodes[x, y - 1] == null) _horizontalEdges[x, y] = null;
            //bottom
            if (y == Flow.GraphDim - 1 || _nodes[x, y + 1] == null) _horizontalEdges[x, y + 1] = null;
        }

        public void Draw()
        {
            foreach (Node node in _nodes)
            {
                if (node != null) node.Draw();
            }
            foreach(Edge edge in _horizontalEdges)
            {
                if (edge != null) edge.Draw();
            }
            foreach(Edge edge in _verticalEdges)
            {
                if (edge != null) edge.Draw();
            }
        }
    }
}
