using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    abstract class Puzzle
    {
        public class Path
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

            public Node OtherNode(Node node)
            {
                if (node == node1) return node2;
                else if (node == node2) return node1;
                else return null;
            }

            protected Color GetColor()
            {
                if (pathState == PathState.Maybe) return Flow.MaybeColor;

                if (node1.colorIndex == node2.colorIndex && node1.colorIndex >= 0)
                {
                    return Flow.Colors[node1.colorIndex];
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

        public class BridgePath : Path
        {
            public readonly int dest1X;
            public readonly int dest1Y;
            public readonly int dest2X;
            public readonly int dest2Y;
            public BridgePath(Node node1, Node node2)
                : base(node1, node2, false)
            {
                dest1X = node1.x;
                dest1Y = node1.y;
                dest2X = node2.x;
                dest2Y = node2.y;
                if (node1.x != node2.x)
                {
                    dest1X += node1.x < node2.x ? 1 : -1;
                    dest2X += node2.x < node1.x ? 1 : -1;
                }
                else //node1.y != node2.y
                {
                    dest1Y += node1.y < node2.y ? 1 : -1;
                    dest2Y += node2.y < node1.y ? 1 : -1;
                }

                AssignNodes();
            }

            protected override void AssignNodes()
            {
                assignNodeOffOne(node1, dest1X, dest1Y);
                assignNodeOffOne(node2, dest2X, dest2Y);
            }

            public override void Draw()
            {
                Flow.Sd.DrawBridgePath(node1.x, node1.y, node2.x, node2.y, GetColor(), isGuess);
            }
        }

        public class PortalPath : Path
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

        public class Node
        {
            public readonly int x;
            public readonly int y;
            public readonly int numPaths;

            public Path[] paths;
            public int colorIndex;
            public bool isEndpoint;
            
            public Node(int x, int y, int numPaths)
            {
                this.x = x;
                this.y = y;
                this.numPaths = numPaths;
                paths = new Path[4];
                colorIndex = -1;
                isEndpoint = false;
            }

            public bool IsSolved
            {
                get
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Path path = paths[i];
                        if (path != null && path.pathState == Path.PathState.Maybe) return false;
                    }

                    return true;
                }
            }

            public static Path Between(Node node1, Node node2)
            {
                for (int i = 0; i < 4; i++)
                {
                    Path path1 = node1.paths[i];
                    if (path1 == null) continue;

                    for (int j = 0; j < 4; j++)
                    {
                        Path path2 = node2.paths[j];
                        if (path2 == null) continue;

                        if (path1 == path2) return path1;
                    }
                }

                return null;
            }

            public HashSet<Node> Neighbors(bool includeGood = true, bool includeMaybe = true, bool includeBad = true)
            {
                HashSet<Node> neighbors = new HashSet<Node>();

                for (int i = 0; i < 4; i++)
                {
                    Path path = paths[i];
                    if (path == null) continue;

                    if ((path.pathState == Path.PathState.Good && includeGood) ||
                        (path.pathState == Path.PathState.Maybe && includeMaybe) ||
                        (path.pathState == Path.PathState.Bad && includeBad))
                    {
                        if (path.node1 == this) neighbors.Add(path.node2);
                        else neighbors.Add(path.node1);
                    }
                }

                return neighbors;
            }

            public HashSet<Node> FindGraph(bool includeGood = true, bool includeMaybe = true, bool includeBad = true)
            {
                HashSet<Node> unprocessedNodes = new HashSet<Node>();
                unprocessedNodes.Add(this);

                HashSet<Node> processedNodes = new HashSet<Node>();
                while (unprocessedNodes.Count > 0)
                {
                    Node node = unprocessedNodes.First();
                    unprocessedNodes.Remove(node);
                    processedNodes.Add(node);

                    HashSet<Node> newNodes = node.Neighbors(includeGood, includeMaybe, includeBad);
                    newNodes.ExceptWith(processedNodes);
                    unprocessedNodes.UnionWith(newNodes);
                }

                return processedNodes;
            }

            public static void UpdateGraphColor(HashSet<Node> graph)
            {
                HashSet<int> colorIndices = new HashSet<int>();

                foreach (Node node in graph)
                {
                    if (node.isEndpoint) colorIndices.Add(node.colorIndex);
                }

                if (colorIndices.Count == 1)
                {
                    int colorIndex = colorIndices.First();
                    foreach (Node node in graph)
                    {
                        node.colorIndex = colorIndex;
                    }
                }
            }

            public int PathCount(bool includeGood = true, bool includeMaybe = true, bool includeBad = true)
            {
                int count = 0;

                for (int pathIndex = 0; pathIndex < 4; pathIndex++)
                {
                    Path path = paths[pathIndex];
                    if (path == null) continue;

                    if ((path.pathState == Path.PathState.Good && includeGood) ||
                        (path.pathState == Path.PathState.Maybe && includeMaybe) ||
                        (path.pathState == Path.PathState.Bad && includeBad))
                    {
                        count++;
                    }
                }

                return count;
            }

            public HashSet<Path> PathSet(bool includeGood = true, bool includeMaybe = true, bool includeBad = true)
            {
                HashSet<Path> pathSet = new HashSet<Path>();

                for (int pathIndex = 0; pathIndex < 4; pathIndex++)
                {
                    Path path = paths[pathIndex];
                    if (path == null) continue;

                    if ((path.pathState == Path.PathState.Good && includeGood) ||
                        (path.pathState == Path.PathState.Maybe && includeMaybe) ||
                        (path.pathState == Path.PathState.Bad && includeBad))
                    {
                        pathSet.Add(path);
                    }
                }

                return pathSet;
            }


            private Node Up(HashSet<Node> graph)
            {
                Path upPath = paths[3];
                if (upPath == null) return null;

                Node upNode = upPath.OtherNode(this);

                if (graph.Contains(upNode))
                {
                    return upNode;
                }
                else
                {
                    return null;
                }
            }

            private (Node, int) NextLeft(HashSet<Node> graph, int previousDirection)
            {
                int direction = previousDirection - 1; //optimal direction, traverse rightwardward from here
                if (direction == -1) direction = 3; //stupid clock arithmetic

                for (int i = 0; i < 4; i++)
                {
                    Path path = paths[direction];
                    if (path == null) //if not valid
                    {
                        direction = (direction + 1) % 4;
                        continue;
                    }

                    Node otherNode = path.OtherNode(this);
                    if (graph.Contains(otherNode))
                    {
                        return (otherNode, direction);
                    }

                    direction = (direction + 1) % 4;
                }

                return (null, -1); //shouldn't happen
            }

            public static List<Node> ClockwiseBorder(HashSet<Node> graph)
            {
                Node topNode = graph.First();
                while (topNode.Up(graph) != null) topNode = topNode.Up(graph); //find the border

                List<Node> borderNodes = new List<Node>();

                (Node, int) borderData = topNode.NextLeft(graph, 0);
                Node firstNode = borderData.Item1;
                int firstDirection = borderData.Item2;
                borderNodes.Add(firstNode);

                Node thisNode = firstNode;
                int thisDirection = firstDirection;
                while (true)
                {
                    borderData = thisNode.NextLeft(graph, thisDirection);
                    thisNode = borderData.Item1;
                    thisDirection = borderData.Item2;

                    if (thisNode == firstNode && thisDirection == firstDirection)
                    {
                        break; //we've come full circle, we done
                    }
                    else
                    {
                        borderNodes.Add(thisNode);
                    }
                }

                return borderNodes;
            }

            public void OrganizedEdgeDraw()
            {
                if (paths[0] != null) paths[0].Draw();
                if (paths[1] != null) paths[1].Draw();
                if ((x == 0 && paths[2] != null) || paths[2] is PortalPath) paths[2].Draw();
                if ((y == 0 && paths[3] != null) || paths[3] is PortalPath) paths[3].Draw();
            }
        }

        

        protected HashSet<Node> _allNodes;
        protected HashSet<Path> _allPaths;
        public Puzzle(Graph graph)
        {
            _allNodes = new HashSet<Node>();
            Node[,] allNodes = new Node[Flow.GraphDim, Flow.GraphDim];
            for (int i = 0; i < Flow.GraphDim; i++)
            {
                for (int j = 0; j < Flow.GraphDim; j++)
                {
                    Vertex vertex = graph.getVertex(i, j);
                    if (vertex == null || vertex.Type == Vertex.VertexType.Bridge) continue;

                    int numPaths = (vertex.Type == Vertex.VertexType.Standard) ? 2 : 1;
                    Node newNode = new Node(i, j, numPaths);
                    allNodes[i, j] = newNode;
                    _allNodes.Add(newNode);

                    if (vertex.Type == Vertex.VertexType.Endpoint)
                    {
                        newNode.colorIndex = vertex.ColorIndex;
                        newNode.isEndpoint = true;
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
                            Path newPath = new Path(allNodes[i, j], allNodes[i + 1, j]);
                        }
                        else if (leftVertex.Type != Vertex.VertexType.Bridge && rightVertex.Type == Vertex.VertexType.Bridge)
                        {
                            int nextNonBridgeX = i + 2;
                            while (graph.getVertex(nextNonBridgeX, j).Type == Vertex.VertexType.Bridge) nextNonBridgeX++;

                            BridgePath newPath = new BridgePath(allNodes[i, j], allNodes[nextNonBridgeX, j]);
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
                            PortalPath newPath = new PortalPath(allNodes[x1, y1], allNodes[x2, y2],
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
                            PortalPath newPath = new PortalPath(allNodes[x1, y1], allNodes[x2, y2],
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
                            Path newPath = new Path(allNodes[i, j], allNodes[i, j + 1]);
                        }
                        else if (topVertex.Type != Vertex.VertexType.Bridge && bottomVertex.Type == Vertex.VertexType.Bridge)
                        {
                            int nextNonBridgeY = j + 2;
                            while (graph.getVertex(i, nextNonBridgeY).Type == Vertex.VertexType.Bridge) nextNonBridgeY++;

                            BridgePath newPath = new BridgePath(allNodes[i, j], allNodes[i, nextNonBridgeY]);
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
                            PortalPath newPath = new PortalPath(allNodes[x1, y1], allNodes[x2, y2],
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
                            PortalPath newPath = new PortalPath(allNodes[x1, y1], allNodes[x2, y2],
                                edge.PointFirst ? edge.X1 : edge.X2, edge.PointFirst ? edge.Y1 : edge.Y2,
                                otherEdge.PointFirst ? otherEdge.X1 : otherEdge.X2, otherEdge.PointFirst ? otherEdge.Y1 : otherEdge.Y2);
                        }

                    }
                    //else wall, don't bother
                }
            }


            _allPaths = new HashSet<Path>();
            foreach (Node node in _allNodes)
            {
                for (int pathIndex = 0; pathIndex < 4; pathIndex++)
                {
                    Path path = node.paths[pathIndex];
                    if (path == null) continue;
                    _allPaths.Add(path);
                }
            }
        }


        public void Draw()
        {
            foreach (Node node in _allNodes)
            {
                node.OrganizedEdgeDraw();
            }
        }

        private bool checkPathCounts()
        {
            foreach (Node node in _allNodes)
            { 
                int goodPathCount = 0;
                for (int pathIndex = 0; pathIndex < 4; pathIndex++)
                {
                    Path path = node.paths[pathIndex];
                    if (path == null) continue;
                    if (path.pathState == Path.PathState.Maybe) return false;
                    if (path.pathState == Path.PathState.Good) goodPathCount++;
                }
                if (goodPathCount != node.numPaths) return false;
                
            }
            return true;
        }

        private bool checkColors()
        {
            HashSet<Node>[] nodesByColor = new HashSet<Node>[16];
            for (int i = 0; i < 16; i++)
            {
                nodesByColor[i] = new HashSet<Node>();
            }

            foreach (Node node in _allNodes)
            {
                    if (node.colorIndex == -1) return false; //uncolored node is bad
                    nodesByColor[node.colorIndex].Add(node);
            }

            for (int i = 0; i < 16; i++)
            {
                HashSet<Node> coloredNodes = nodesByColor[i];
                if (coloredNodes.Count == 0) continue;

                Node firstNode = coloredNodes.First();
                HashSet<Node> connectedNodes = firstNode.FindGraph(true, false, false);

                if (!coloredNodes.SetEquals(connectedNodes)) return false; //if not all connected, then false
            }

            return true;
        }

        public bool IsSolved
        {
            get
            {
                return checkPathCounts() && checkColors();
            }
        }

        public abstract void Forward();
        public abstract void Back();


    }
}
