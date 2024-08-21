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
        private Edge[,] _horizontalEdges;
        private Edge[,] _verticalEdges;

        Node[] _endpointNodes;
        int _numEndpointNodes;

        Edge[] _portalEdges;
        int _numPortalEdges;
        public Graph()
        {
            _nodes = new Node[Flow.GraphDim, Flow.GraphDim];
            _horizontalEdges = new Edge[Flow.GraphDim, Flow.GraphDim + 1];
            _verticalEdges = new Edge[Flow.GraphDim + 1, Flow.GraphDim];
            for (int i = 0; i <  Flow.GraphDim; i++)
            {
                for (int j = 0; j < Flow.GraphDim; j++)
                {
                    _nodes[i, j] = new Node(i, j);
                    _horizontalEdges[i, j] = new Edge(i, j, i + 1, j);
                    _verticalEdges[i, j] = new Edge(i, j, i, j + 1);
                }
            }
            for (int i = 0; i < Flow.GraphDim; i++) //bottom edges
            {
                _horizontalEdges[i, Flow.GraphDim] = new Edge(i, Flow.GraphDim, i + 1, Flow.GraphDim);
            }
            for (int j = 0; j < Flow.GraphDim; j++) //right edges
            {
                _verticalEdges[Flow.GraphDim, j] = new Edge(Flow.GraphDim, j, Flow.GraphDim, j + 1);
            }

            _endpointNodes = new Node[Flow.Colors.Length * 2];
            _numEndpointNodes = 0;
            _portalEdges = new Edge[Flow.Colors.Length * 2];
            _numPortalEdges = 0;
        }

        public void Update()
        {
            Input.KeyboardInputType keyInput = Input.GetKeyboardInputType();
            if (keyInput == Input.KeyboardInputType.Endpoint || keyInput == Input.KeyboardInputType.Bridge || keyInput == Input.KeyboardInputType.Gone) //nodes
            {
                if (!Input.IsClickingOnNode()) return;

                (int, int) nodeCoordinates = Input.NodeCoordinates();
                Node node = _nodes[nodeCoordinates.Item1, nodeCoordinates.Item2];

                if (node.Type != Node.NodeType.Standard) return;

                if (keyInput == Input.KeyboardInputType.Endpoint)
                {
                    node.Type = Node.NodeType.Endpoint;
                    node.ColorIndex = _numEndpointNodes / 2;
                    _endpointNodes[_numEndpointNodes] = node;
                    _numEndpointNodes++;
                }
                else if (keyInput == Input.KeyboardInputType.Bridge)
                {
                    node.Type = Node.NodeType.Bridge;
                }
                else // Gone
                {
                    node.Type = Node.NodeType.Gone;
                    EdgeRemovals(node.X, node.Y);
                }
            }
            else if (keyInput == Input.KeyboardInputType.Wall || keyInput == Input.KeyboardInputType.Portal) //edges
            {
                if (!Input.IsClickingOnEdge()) return;

                ((int, int), (int, int)) edgePair = Input.EdgeCoordinates();
                Edge edge;
                if (edgePair.Item1.Item1 + 1 == edgePair.Item2.Item1) //horizontal
                {
                    edge = _horizontalEdges[edgePair.Item1.Item1, edgePair.Item1.Item2];
                }
                else
                {
                    edge = _verticalEdges[edgePair.Item1.Item1, edgePair.Item1.Item2];
                }

                if (edge.Type != Edge.EdgeType.Standard) return;

                if (keyInput == Input.KeyboardInputType.Wall)
                {
                    edge.Type = Edge.EdgeType.Wall;
                }
                else // Portal
                {
                    edge.Type = Edge.EdgeType.Portal;
                    edge.ColorIndex = _numPortalEdges / 2;
                    _portalEdges[_numPortalEdges] = edge;
                    _numPortalEdges++;
                }
            }
        }

        private void EdgeRemovals(int x, int y)
        {
            if (1 <= x)
            {
                if (_nodes[x - 1, y].Type == Node.NodeType.Gone) _verticalEdges[x, y] = null;
            }
            if (x < Flow.GraphDim - 1)
            {
                if (_nodes[x + 1, y].Type == Node.NodeType.Gone) _verticalEdges[x + 1, y] = null;
            }

            if (1 <= y)
            {
                if (_nodes[x, y - 1].Type == Node.NodeType.Gone) _horizontalEdges[x, y] = null;
            }
            if (y < Flow.GraphDim - 1)
            {
                if (_nodes[x, y + 1].Type == Node.NodeType.Gone) _horizontalEdges[x, y + 1] = null;
            }
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
