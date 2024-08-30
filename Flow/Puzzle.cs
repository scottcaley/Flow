using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    abstract class Puzzle
    {
        protected class Path
        {
            public enum PathState
            {
                Bad,
                Maybe,
                Good
            }

            public PathState pathState;
            public bool isGuess;

            public readonly Node node1;
            public readonly Node node2;
            public Path(Node node1, Node node2)
                : this(node1, node2, true) { }

            protected Path(Node node1, Node node2, bool isStandardPath)
            {
                pathState = PathState.Maybe;
                this.node1 = node1;
                this.node2 = node2;
                isGuess = false;

                if (isStandardPath) AssignNodes();
            }

            protected Color GetColor()
            {
                if (pathState == PathState.Maybe) return Flow.MaybeColor;

                if (node1.color == node2.color)
                {
                    return node1.color;
                }
                else
                {
                    return Flow.GoodColor;
                }
            }

            protected virtual void AssignNodes()
            {
                assignNodeOffOne(node1, node2.x, node2.y);
                assignNodeOffOne(node2, node1.x, node1.y);
            }

            protected void assignNodeOffOne(Node node, int destX, int destY)
            {
                //check if they are actually supposed to be doing this
                if (Math.Abs(node.x - destX) == 1 ^ Math.Abs(node.y - destY) == 1)
                {
                    if (destX - node.x == 1) node.paths[0] = this;
                    else if (destY - node.y == 1) node.paths[1] = this;
                    else if (destX - node.x == -1) node.paths[2] = this;
                    else if (destY - node.y == -1) node.paths[3] = this;
                }
            }

            public virtual void Draw()
            {
                if (pathState == PathState.Bad) return;
                Flow.Sd.DrawPath(node1.x, node1.y, node2.x, node2.y, GetColor(), isGuess);
            }

        }

        protected class BridgePath : Path
        {
            public readonly int centerX;
            public readonly int centerY;
            public BridgePath(Node node1, Node node2)
                : base(node1, node2, false)
            {
                this.centerX = (node1.x + node2.x) / 2;
                this.centerY = (node1.y + node2.y) / 2;

                AssignNodes();
            }

            protected override void AssignNodes()
            {
                assignNodeOffOne(node1, centerX, centerY);
                assignNodeOffOne(node2, centerX, centerY);
            }

            public override void Draw()
            {
                Flow.Sd.DrawBridgePath(node1.x, node1.y, node2.x, node2.y, GetColor(), isGuess);
            }
        }

        protected class PortalPath : Path
        {
            public readonly int dest1X;
            public readonly int dest1Y;
            public readonly int dest2X;
            public readonly int dest2Y;
            public PortalPath(Node node1, Node node2, int dest1X, int dest1Y, int dest2X, int dest2Y)
                : base(node1, node2, false)
            {
                this.dest1X = dest1X;
                this.dest1Y = dest1Y;
                this.dest2X = dest2X;
                this.dest2Y = dest2Y;
                int x = 0;
                AssignNodes();
            }

            protected override void AssignNodes()
            {
                assignNodeOffOne(node1, dest1X, dest1Y);
                assignNodeOffOne(node2, dest2X, dest2Y);
            }

            public override void Draw()
            {
                Flow.Sd.DrawPortalPath(node1.x, node1.y, node2.x, node2.y, dest1X, dest1Y, dest2X, dest2Y, GetColor(), isGuess);
            }
        }

        protected class Node
        {
            public readonly int x;
            public readonly int y;
            public readonly int numPaths;

            public Path[] paths;
            public Color color;
            
            public Node(int x, int y, int numPaths)
            {
                this.x = x;
                this.y = y;
                this.numPaths = numPaths;
                paths = new Path[4];
            }

            public void OrganizedEdgeDraw()
            {
                if (paths[0] != null) paths[0].Draw();
                if (paths[1] != null) paths[1].Draw();
                if ((x == 0 && paths[2] != null) || paths[2] is PortalPath) paths[2].Draw();
                if ((y == 0 && paths[3] != null) || paths[3] is PortalPath) paths[3].Draw();
            }
        }

        

        protected Node[,] _allNodes;
        public Puzzle(Graph graph)
        {

            _allNodes = new Node[Flow.GraphDim, Flow.GraphDim];
            for (int i = 0; i < Flow.GraphDim; i++)
            {
                for (int j = 0; j < Flow.GraphDim; j++)
                {
                    Vertex vertex = graph.getVertex(i, j);
                    if (vertex != null)
                    {
                        int numPaths = 0;
                        if (vertex.Type == Vertex.VertexType.Standard)
                        {
                            numPaths = 2;
                        }
                        else if (vertex.Type == Vertex.VertexType.Endpoint)
                        {
                            numPaths = 1;
                        }
                        _allNodes[i, j] = new Node(i, j, numPaths);
                        if (vertex.Type == Vertex.VertexType.Endpoint)
                        {
                            _allNodes[i, j].color = Flow.Colors[vertex.ColorIndex];
                        }
                    }
                }
            }

            for (int i = -1; i < Flow.GraphDim; i++)
            {
                for (int j = -1; j < Flow.GraphDim; j++)
                {
                    Edge edge = graph.getEdge(i, j, i + 1, j);
                    if (edge == null) continue;
                    
                    if (edge.Type == Edge.EdgeType.Standard)
                    {
                        Vertex leftVertex = graph.getVertex(i, j);
                        Vertex rightVertex = graph.getVertex(i + 1, j);

                        if (leftVertex.Type != Vertex.VertexType.Bridge && rightVertex.Type != Vertex.VertexType.Bridge)
                        {
                            Path newPath = new Path(_allNodes[i, j], _allNodes[i + 1, j]);
                        }
                        else if (leftVertex.Type != Vertex.VertexType.Bridge && rightVertex.Type == Vertex.VertexType.Bridge)
                        {
                            BridgePath newPath = new BridgePath(_allNodes[i, j], _allNodes[i + 2, j]);
                        }
                        // other bridge cases don't bother (redundant)
                    }
                    else if (edge.Type == Edge.EdgeType.Portal)
                    {
                        int portalIndex;
                        for (portalIndex = 0; portalIndex < graph.NumPortalEdges; portalIndex++)
                        {
                            if (graph.PortalEdges[portalIndex] == edge) break;
                        }
                        int otherPortalindex = (portalIndex / 2) * 2 + (portalIndex % 2 + 1) % 2;
                        Edge otherEdge = graph.PortalEdges[otherPortalindex];

                        
                        int x1 = edge.PointFirst ? edge.X1 : edge.X2;
                        int y1 = edge.PointFirst ? edge.Y1 : edge.Y2;
                        int x2 = otherEdge.PointFirst ? otherEdge.X1 : otherEdge.X2;
                        int y2 = otherEdge.PointFirst ? otherEdge.Y1 : otherEdge.Y2;
                        if (graph.getVertex(x1, y1) != null)
                        {
                            PortalPath newPath = new PortalPath(_allNodes[x1, y1], _allNodes[x2, y2],
                                edge.PointFirst ? edge.X2 : edge.X1, edge.PointFirst ? edge.Y2 : edge.Y1,
                                otherEdge.PointFirst ? otherEdge.X2 : otherEdge.X1, otherEdge.PointFirst ? otherEdge.Y2 : otherEdge.Y1);
                        }

                        // by way of chatGPT editing
                        x1 = edge.PointFirst ? edge.X2 : edge.X1;
                        y1 = edge.PointFirst ? edge.Y2 : edge.Y1;
                        x2 = otherEdge.PointFirst ? otherEdge.X2 : otherEdge.X1;
                        y2 = otherEdge.PointFirst ? otherEdge.Y2 : otherEdge.Y1;

                        if (graph.getVertex(x1, y1) != null)
                        {
                            PortalPath newPath = new PortalPath(_allNodes[x1, y1], _allNodes[x2, y2],
                                edge.PointFirst ? edge.X1 : edge.X2, edge.PointFirst ? edge.Y1 : edge.Y2,
                                otherEdge.PointFirst ? otherEdge.X1 : otherEdge.X2, otherEdge.PointFirst ? otherEdge.Y1 : otherEdge.Y2);
                        }
                        
                    }
                    //else wall, don't bother
                }
            }


            for (int i = -1; i < Flow.GraphDim; i++)
            {
                for (int j = -1; j < Flow.GraphDim; j++)
                {
                    Edge edge = graph.getEdge(i, j, i, j + 1);
                    if (edge == null) continue;

                    if (edge.Type == Edge.EdgeType.Standard)
                    {
                        Vertex topVertex = graph.getVertex(i, j);
                        Vertex bottomVertex = graph.getVertex(i, j + 1);

                        if (topVertex.Type != Vertex.VertexType.Bridge && bottomVertex.Type != Vertex.VertexType.Bridge)
                        {
                            Path newPath = new Path(_allNodes[i, j], _allNodes[i, j + 1]);
                        }
                        else if (topVertex.Type != Vertex.VertexType.Bridge && bottomVertex.Type == Vertex.VertexType.Bridge)
                        {
                            BridgePath newPath = new BridgePath(_allNodes[i, j], _allNodes[i, j + 2]);
                        }
                        // other bridge cases don't bother (redundant)
                    }
                    else if (edge.Type == Edge.EdgeType.Portal)
                    {
                        int portalIndex;
                        for (portalIndex = 0; portalIndex < graph.NumPortalEdges; portalIndex++)
                        {
                            if (graph.PortalEdges[portalIndex] == edge) break;
                        }
                        int otherPortalindex = (portalIndex / 2) * 2 + (portalIndex % 2 + 1) % 2;
                        Edge otherEdge = graph.PortalEdges[otherPortalindex];


                        int x1 = edge.PointFirst ? edge.X1 : edge.X2;
                        int y1 = edge.PointFirst ? edge.Y1 : edge.Y2;
                        int x2 = otherEdge.PointFirst ? otherEdge.X1 : otherEdge.X2;
                        int y2 = otherEdge.PointFirst ? otherEdge.Y1 : otherEdge.Y2;
                        if (graph.getVertex(x1, y1) != null)
                        {
                            PortalPath newPath = new PortalPath(_allNodes[x1, y1], _allNodes[x2, y2],
                                edge.PointFirst ? edge.X2 : edge.X1, edge.PointFirst ? edge.Y2 : edge.Y1,
                                otherEdge.PointFirst ? otherEdge.X2 : otherEdge.X1, otherEdge.PointFirst ? otherEdge.Y2 : otherEdge.Y1);
                        }

                        // by way of chatGPT editing
                        x1 = edge.PointFirst ? edge.X2 : edge.X1;
                        y1 = edge.PointFirst ? edge.Y2 : edge.Y1;
                        x2 = otherEdge.PointFirst ? otherEdge.X2 : otherEdge.X1;
                        y2 = otherEdge.PointFirst ? otherEdge.Y2 : otherEdge.Y1;

                        if (graph.getVertex(x1, y1) != null)
                        {
                            PortalPath newPath = new PortalPath(_allNodes[x1, y1], _allNodes[x2, y2],
                                edge.PointFirst ? edge.X1 : edge.X2, edge.PointFirst ? edge.Y1 : edge.Y2,
                                otherEdge.PointFirst ? otherEdge.X1 : otherEdge.X2, otherEdge.PointFirst ? otherEdge.Y1 : otherEdge.Y2);
                        }

                    }
                    //else wall, don't bother
                }
            }


        }


        public void Draw()
        {
            for (int i = 0; i < Flow.GraphDim; i++)
            {
                for (int j = 0; j < Flow.GraphDim; j++)
                {
                    if (_allNodes[i, j] != null) _allNodes[i, j].OrganizedEdgeDraw();
                }
            }
        }

        public abstract void PerformMove();



    }
}
