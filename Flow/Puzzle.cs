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
                Unknown,
                Good
            }

            public static PathState Opposite(PathState pathState)
            {
                if (pathState == PathState.Bad) return PathState.Good;
                else if (pathState == PathState.Unknown) return PathState.Unknown;
                else return PathState.Bad;
            }

            public PathState pathState;
            public bool isCertain;

            public readonly Node node1;
            public readonly Node node2;
            public Path(Node node1, Node node2)
                : this(node1, node2, true) { }

            protected Path(Node node1, Node node2, bool isStandardPath)
            {
                pathState = PathState.Unknown;
                this.node1 = node1;
                this.node2 = node2;
                isCertain = false;

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
                Color color;
                if (pathState == PathState.Bad)
                {
                    color = isCertain ? Flow.BadColor : Flow.UncertainBadColor;
                }
                else if (pathState == PathState.Unknown)
                {
                    color = Flow.MaybeColor;
                }
                else if (node1.colorIndex == node2.colorIndex && node1.colorIndex >= 0)
                {
                    int colorIndex = node1.colorIndex;
                    color = isCertain ? Flow.Colors[colorIndex] : Flow.UncertainColors[colorIndex];
                }
                else
                {
                    color = isCertain ? Flow.GoodColor : Flow.UncertainGoodColor;
                }

                return color;
            }

            protected virtual void AssignNodes()
            {
                AssignNodeOffOne(node1, node2.x, node2.y);
                AssignNodeOffOne(node2, node1.x, node1.y);
            }

            public override String ToString()
            {
                return "path from (" + node1.x + ", " + node1.y + ") to (" + node2.x + ", " + node2.y + ")";
            }

            protected void AssignNodeOffOne(Node node, int destX, int destY)
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
                if (pathState != PathState.Bad || !isCertain)
                {
                    Flow.Sd.DrawPath(node1.x, node1.y, node2.x, node2.y, GetColor());
                }
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
                AssignNodeOffOne(node1, dest1X, dest1Y);
                AssignNodeOffOne(node2, dest2X, dest2Y);
            }
            public override String ToString()
            {
                return "bridge-path from (" + node1.x + ", " + node1.y + ") to (" + node2.x + ", " + node2.y + ")";
            }

            /*public override void Draw()
            {
               not necessary 
            }*/
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
                AssignNodeOffOne(node1, dest1X, dest1Y);
                AssignNodeOffOne(node2, dest2X, dest2Y);
            }

            public override String ToString()
            {
                return "portal-path from (" + node1.x + ", " + node1.y + ") with dest (" + dest1X + ", " + dest1Y + ")" +
                    " to (" + node2.x + ", " + node2.y + ") with dest (" + dest2X + ", " + dest2Y + ")";
            }

            public override void Draw()
            {
                if (pathState != PathState.Bad)
                {
                    Flow.Sd.DrawHalfPath(node1.x, node1.y, dest1X, dest1Y, GetColor());
                    Flow.Sd.DrawHalfPath(node2.x, node2.y, dest2X, dest2Y, GetColor());
                }
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
                        if (path != null && path.pathState == Path.PathState.Unknown) return false;
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
                        (path.pathState == Path.PathState.Unknown && includeMaybe) ||
                        (path.pathState == Path.PathState.Bad && includeBad))
                    {
                        neighbors.Add(path.OtherNode(this));
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

                int colorIndex = (colorIndices.Count == 1) ? colorIndices.First() : -1;
                foreach (Node node in graph)
                {
                    if (!node.isEndpoint) node.colorIndex = colorIndex;
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
                        (path.pathState == Path.PathState.Unknown && includeMaybe) ||
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
                        (path.pathState == Path.PathState.Unknown && includeMaybe) ||
                        (path.pathState == Path.PathState.Bad && includeBad))
                    {
                        pathSet.Add(path);
                    }
                }

                return pathSet;
            }

            /*
             * Should be only called on an endpoint
             */
            public Node EndOfLine()
            {
                Node end = this;
                Node previousEnd = null;
                if (PathCount(true, false, false) == 1)
                {
                    end = Neighbors(true, false, false).First();
                    previousEnd = this;
                }

                while (end.PathCount(true, false, false) == 2)
                {
                    foreach (Node node in end.Neighbors(true, false, false))
                    {
                        if (node != previousEnd)
                        {
                            previousEnd = end;
                            end = node;
                            break;
                        }
                    }
                }

                return end;
            }

            private (Path, int) LeftMost(int previousDirection)
            {
                int direction = previousDirection - 1; //optimal direction, traverse rightward from here
                if (direction == -1) direction = 3; //stupid clock arithmetic

                for (int i = 0; i < 4; i++)
                {
                    Path leftMostPath = paths[direction];
                    if (leftMostPath == null || leftMostPath.pathState == Path.PathState.Bad) //if not valid
                    {
                        direction = (direction + 1) % 4;
                        continue;
                    }

                    Node leftMostNode = leftMostPath.OtherNode(this);
                    return (leftMostPath, direction);
                }

                return (null, -1); //shouldn't happen
            }

            public bool VerifyNoNumPathViolations(HashSet<Path> borderPath)
            {
                Dictionary<Node, HashSet<Path>> nodeToPaths = new Dictionary<Node, HashSet<Path>>();
                
                foreach (Path path in borderPath)
                {
                    Node[] nodes = { path.node1, path.node2 };
                    foreach (Node node in nodes)
                    {
                        if (!nodeToPaths.ContainsKey(node)) nodeToPaths.Add(node, new HashSet<Path>());
                        nodeToPaths[node].Add(path);
                    }
                }

                foreach (Node node in nodeToPaths.Keys)
                {
                    nodeToPaths[node].UnionWith(node.PathSet(true, false, false));
                    if (nodeToPaths[node].Count > node.numPaths) return false;
                }

                return true;
            }

            public HashSet<Path> TraverseBorderClockwise(int wallDirection, HashSet<Node> sourceLine)
            {
                HashSet<Path> borderPaths = new HashSet<Path>();

                Node node = this;
                int direction = (wallDirection + 1) % 4;

                do
                {
                    (Path, int) borderData = node.LeftMost(direction);
                    Path path = borderData.Item1;
                    direction = borderData.Item2;
                    node = path.OtherNode(node);

                    if (borderPaths.Contains(path)
                        || sourceLine.Contains(node)
                        || (node.colorIndex >= 0 && node.colorIndex != colorIndex)
                        || path is PortalPath) 
                    {
                        return null;
                    }

                    borderPaths.Add(path);

                } while (node.colorIndex == -1);

                if (!VerifyNoNumPathViolations(borderPaths)) return null;

                return borderPaths;
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
        protected bool _solutionFound;
        protected String _actionLine;
        public Puzzle(Grid graph)
        {
            _solutionFound = false;
            _allNodes = new HashSet<Node>();
            Node[,] allNodes = new Node[Flow.GraphDimX, Flow.GraphDimY];
            for (int i = 0; i < Flow.GraphDimX; i++)
            {
                for (int j = 0; j < Flow.GraphDimY; j++)
                {
                    Square vertex = graph.getVertex(i, j);
                    if (vertex == null || vertex.Type == Square.SquareType.Bridge) continue;

                    int numPaths = (vertex.Type == Square.SquareType.Standard) ? 2 : 1;
                    Node newNode = new Node(i, j, numPaths);
                    allNodes[i, j] = newNode;
                    _allNodes.Add(newNode);

                    if (vertex.Type == Square.SquareType.Endpoint)
                    {
                        newNode.colorIndex = vertex.ColorIndex;
                        newNode.isEndpoint = true;
                    }
                }
            }

            for (int i = -1; i < Flow.GraphDimX; i++)
            {
                for (int j = -1; j < Flow.GraphDimY; j++)
                {
                    Border edge = graph.getEdge(i, j, i + 1, j);
                    if (edge == null) continue;
                    
                    if (edge.Type == Border.BorderType.Standard)
                    {
                        Square leftVertex = graph.getVertex(i, j);
                        Square rightVertex = graph.getVertex(i + 1, j);

                        if (leftVertex.Type != Square.SquareType.Bridge && rightVertex.Type != Square.SquareType.Bridge)
                        {
                            Path newPath = new Path(allNodes[i, j], allNodes[i + 1, j]);
                        }
                        else if (leftVertex.Type != Square.SquareType.Bridge && rightVertex.Type == Square.SquareType.Bridge)
                        {
                            int nextNonBridgeX = i + 2;
                            while (graph.getVertex(nextNonBridgeX, j).Type == Square.SquareType.Bridge) nextNonBridgeX++;

                            BridgePath newPath = new BridgePath(allNodes[i, j], allNodes[nextNonBridgeX, j]);
                        }
                        // other bridge cases don't bother (redundant)
                    }
                    else if (edge.Type == Border.BorderType.Portal)
                    {
                        int portalIndex;
                        for (portalIndex = 0; portalIndex < graph.NumPortalBarriers; portalIndex++)
                        {
                            if (graph.PortalBarriers[portalIndex] == edge) break;
                        }
                        int otherPortalindex = (portalIndex / 2) * 2 + (portalIndex % 2 + 1) % 2;
                        Border otherEdge = graph.PortalBarriers[otherPortalindex];

                        
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


            for (int i = -1; i < Flow.GraphDimX; i++)
            {
                for (int j = -1; j < Flow.GraphDimY; j++)
                {
                    Border edge = graph.getEdge(i, j, i, j + 1);
                    if (edge == null) continue;

                    if (edge.Type == Border.BorderType.Standard)
                    {
                        Square topVertex = graph.getVertex(i, j);
                        Square bottomVertex = graph.getVertex(i, j + 1);

                        if (topVertex.Type != Square.SquareType.Bridge && bottomVertex.Type != Square.SquareType.Bridge)
                        {
                            Path newPath = new Path(allNodes[i, j], allNodes[i, j + 1]);
                        }
                        else if (topVertex.Type != Square.SquareType.Bridge && bottomVertex.Type == Square.SquareType.Bridge)
                        {
                            int nextNonBridgeY = j + 2;
                            while (graph.getVertex(i, nextNonBridgeY).Type == Square.SquareType.Bridge) nextNonBridgeY++;

                            BridgePath newPath = new BridgePath(allNodes[i, j], allNodes[i, nextNonBridgeY]);
                        }
                        // other bridge cases don't bother (redundant)
                    }
                    else if (edge.Type == Border.BorderType.Portal)
                    {
                        int portalIndex;
                        for (portalIndex = 0; portalIndex < graph.NumPortalBarriers; portalIndex++)
                        {
                            if (graph.PortalBarriers[portalIndex] == edge) break;
                        }
                        int otherPortalindex = (portalIndex / 2) * 2 + (portalIndex % 2 + 1) % 2;
                        Border otherEdge = graph.PortalBarriers[otherPortalindex];


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

            _actionLine = "Begin";
        }


        public void Draw()
        {
            foreach (Node node in _allNodes)
            {
                node.OrganizedEdgeDraw();
            }

            Flow.Sd.DisplayLine(_actionLine);
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
                    if (path.pathState == Path.PathState.Unknown) return false;
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

        protected bool VerifySolution()
        {
            _solutionFound = checkPathCounts() && checkColors();
            return _solutionFound;
        }

        public abstract bool IsFinished();
        public abstract void Forward();
        public abstract void Back();


    }
}
