using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Flow
{
    internal class Solution : Puzzle
    {
        public class Move
        {
            public Path path;
            public Path.PathState pathState;
            public bool isGuess;

            public Move(Path path, Path.PathState newState, bool isGuess)
            {
                this.path = path;
                this.pathState = newState;
                this.isGuess = isGuess;
            }

            public void Execute()
            {
                Path.PathState tempState = path.pathState;
                path.pathState = pathState;
                pathState = tempState;

                bool temp = path.isGuess;
                path.isGuess = isGuess;
                isGuess = temp;
            }

        }
        private int NumGuesses
        {
            get
            {
                return _guesses.Count;
            }
        }
        private bool IsGuessing
        {
            get
            {
                return (_guesses.Count > 0);
            }
        }
        private Path LastPath
        {
            get
            {
                return _completedMoves.Peek().path;
            }
        }
        private bool NextMoveReady
        {
            get
            {
                return _incomingMoves.Count > 0;
            }
        }


        private void CreateMove(Path path, Path.PathState pathState)
        {
            _incomingMoves.Push(new Move(path, pathState, false));
        }
        private void CreateGuess(Path path, Path.PathState pathState)
        {
            _incomingMoves.Push(new Move(path, pathState, true));
        }


        private Stack<Move> _incomingMoves;
        private Stack<Move> _completedMoves;

        private Stack<List<Move>> _guesses; //?
        private int _maxGuesses;

        private HashSet<HashSet<Node>> _lines;
        private HashSet<HashSet<Node>> _partitions;

        private HashSet<Node> _markedNumPaths;
        private HashSet<Path> _markedMatchedColor;
        private HashSet<HashSet<Node>> _markedBorderScan;

        private HashSet<HashSet<Node>> GenerateGraphs(HashSet<Node> nodes, bool includeGood = true, bool includeMaybe = true, bool includeBad = true)
        {
            HashSet<HashSet<Node>> graphs = new HashSet<HashSet<Node>>();

            foreach (Node node in nodes)
            {
                if (!ContainedInASet(node, graphs))
                {
                    graphs.Add(node.FindGraph(includeGood, includeMaybe, includeBad));
                }
            }

            return graphs;
        }
        private bool ContainedInASet<T>(T element, HashSet<HashSet<T>> sets)
        {
            foreach (HashSet<T> set in sets)
            {
                if (set.Contains(element)) return true;
            }

            return false;
        }
        private HashSet<T> FindSet<T>(T element, HashSet<HashSet<T>> sets)
        {
            foreach (HashSet<T> set in sets)
            {
                if (set.Contains(element)) return set;
            }

            return null;
        } 

        public Solution(Graph graph) : base(graph)
        {

            _incomingMoves = new Stack<Move>();
            _completedMoves = new Stack<Move>();

            _guesses = new Stack<List<Move>>();
            _maxGuesses = 0;

            _lines = GenerateGraphs(_allNodes, true, false, false);
            _partitions = GenerateGraphs(_allNodes, true, true, false);

            _markedNumPaths = new HashSet<Node>(_allNodes);
            _markedMatchedColor = new HashSet<Path>(_allPaths);
            _markedBorderScan = new HashSet<HashSet<Node>>(_partitions);
            
            FindFirstMoves();
        }

        private void FindFirstMoves()
        {
            foreach (Path path in _allPaths)
            {
                if (path is BridgePath)
                {
                    CreateMove(path, Path.PathState.Good);
                }
            }
        }
        


        private void UpdateDataStructures(Path path, Path.PathState previousState)
        {
            Node node1 = path.node1;
            Node node2 = path.node2;

            if (previousState == Path.PathState.Good ^ path.pathState == Path.PathState.Good)
            {
                HashSet<Node> oldSet = FindSet(node1, _lines);
                _lines.Remove(oldSet);
                oldSet = FindSet(node2, _lines);
                _lines.Remove(oldSet);

                HashSet<Node> set1 = node1.FindGraph(true, false, false);
                _lines.Add(set1);
                if (!set1.Contains(node2)) _lines.Add(node2.FindGraph(true, false, false));
            }

            if (previousState == Path.PathState.Bad ^ path.pathState == Path.PathState.Bad)
            {
                HashSet<Node> oldSet = FindSet(node1, _partitions);
                _partitions.Remove(oldSet);
                _markedBorderScan.Remove(oldSet);
                oldSet = FindSet(node2, _partitions);
                _partitions.Remove(oldSet);
                _markedBorderScan.Remove(oldSet);

                HashSet<Node> set1 = node1.FindGraph(true, true, false);
                _partitions.Add(set1);
                _markedBorderScan.Add(set1);

                if (!set1.Contains(node2))
                {
                    HashSet<Node> set2 = node2.FindGraph(true, true, false);
                    _partitions.Add(set2);
                    _markedBorderScan.Add(set2);
                }
            }

            if (!node1.IsSolved)
            {
                _markedNumPaths.Add(node1);
                _markedMatchedColor.Add(Node.Between(node1, node2));
            }

            if (!node2.IsSolved)
            {
                _markedNumPaths.Add(node2);
                _markedMatchedColor.Add(Node.Between(node2, node1));
            }
        }

        private void UpdateColors(Path path, Path.PathState previousState)
        {
            if (previousState == Path.PathState.Good ^ path.pathState == Path.PathState.Good)
            {
                Node.UpdateGraphColor(FindSet(path.node1, _lines));
                Node.UpdateGraphColor(FindSet(path.node2, _lines));
            }
        }



        
        public override void Forward()
        {
            if (IsGuessing)
            {
                if (SelfContactCheck(LastPath) &&
                    MismatchedColorCheck(LastPath) &&
                    PartitionedEndpointCheck(LastPath))
                {

                }
                else
                {

                }
            }
            if (!NextMoveReady &&
                !NumPathsDerivation(_markedNumPaths) &&
                !MatchedColorDerivation(_markedMatchedColor) &&
                !BorderScanDerivation(_markedBorderScan))
            {
                //generate a guess
            }

            PerformNextMove();
        }

        private bool PerformNextMove()
        {
            if (_incomingMoves.Count == 0) return false;

            Move move = _incomingMoves.Pop();
            move.Execute();
            _completedMoves.Push(move);

            UpdateDataStructures(move.path, move.pathState);
            UpdateColors(move.path, move.pathState);

            return true;
        }


        public override void Back()
        {
            UndoPreviousMove();
        }


        private bool UndoPreviousMove()
        {
            if (_completedMoves.Count == 0) return false;

            Move move = _completedMoves.Pop();
            move.Execute();
            _incomingMoves.Push(move);

            UpdateDataStructures(move.path, move.pathState);
            UpdateColors(move.path, move.pathState);

            return true;
        }




        

        


        /**
         * CHECKS
         */


        /**
         * checks to make sure lines are not adjacent to themself by a maybe/bad edge
         */
        private bool SelfContactCheck(Path path)
        {
            if (path.pathState == Path.PathState.Good)
            {
                HashSet<Node> line = FindSet(path.node1, _lines);

                foreach (Node node in line)
                {
                    HashSet<Node> otherNeighbors = node.Neighbors(false, true, true);
                    foreach (Node otherNode in otherNeighbors)
                    {
                        if (line.Contains(otherNode)) return false;
                    }
                }
            }

            return true;
        }


        /**
         * checks to make sure each line has at most one color
         * lines are partitioned by maybe paths and bad paths
         */
        private bool MismatchedColorCheck(Path path)
        {
            if (path.pathState == Path.PathState.Good)
            {
                HashSet<Node> line = FindSet(path.node1, _lines);
                HashSet<int> colorIndices = new HashSet<int>();

                foreach (Node node in line)
                {
                    colorIndices.Add(node.colorIndex);
                }

                if (colorIndices.Count > 1) return false;
            }

            return true;
        }

        /**
         * checks all partitions (partitioned by bad paths),
         * every partition must have at least one endpoint
         * every endpoint must be found in the same partition as its counterpart
         */
        private bool PartitionedEndpointCheck(Path path)
        {
            if (path.pathState == Path.PathState.Bad)
            {
                Node[] pathNodes = { path.node1, path.node2 };
                for (int nodeIndex = 0; nodeIndex < 2; nodeIndex++)
                {
                    HashSet<Node> partition = pathNodes[nodeIndex].Neighbors(true, true, false);
                    int[] endpointCounts = new int[Flow.Colors.Length];

                    foreach (Node node in partition)
                    {
                        if (node.isEndpoint) endpointCounts[node.colorIndex]++;
                    }

                    bool allEven = true;
                    bool atLeastOneEndpoint = false;
                    for (int colorIndex = 0; colorIndex < Flow.Colors.Length; colorIndex++)
                    {
                        int count = endpointCounts[colorIndex];
                        if (count % 2 != 0) allEven = false;
                        if (count > 0) atLeastOneEndpoint = true;
                    }
                    if (!allEven || !atLeastOneEndpoint) return false;
                }
            }

            return true;
        }
        
        
        /**
         * DERIVATIONS
         */

        private bool NumPathsDerivation(HashSet<Node> markedNodes)
        {
            while (markedNodes.Count > 0)
            {
                Node node = markedNodes.First();
                markedNodes.Remove(node);

                if (node.numPaths == node.PathCount(true, true, false))
                {
                    foreach (Path maybePath in node.PathSet(false, true, false))
                    {
                        CreateMove(maybePath, Path.PathState.Good);
                    }
                    return true;
                }
                else if (node.numPaths == node.PathCount(true, false, false))
                {
                    foreach (Path maybePath in node.PathSet(false, true, false))
                    {
                        CreateMove(maybePath, Path.PathState.Bad);
                    }
                    return true;
                }
            }

            return false;
        }

        private bool MatchedColorDerivation(HashSet<Path> markedPaths)
        {
            while (markedPaths.Count > 0)
            {
                Path path = markedPaths.First();
                markedPaths.Remove(path);
                if (path.pathState != Path.PathState.Maybe) continue;

                Node node1 = path.node1;
                Node node2 = path.node2;

                if (node1.colorIndex >= 0 && node2.colorIndex >= 0)
                {
                    if (node1.colorIndex == node2.colorIndex)
                    {
                        CreateMove(path, Path.PathState.Good);
                    }
                    else
                    {
                        CreateMove(path, Path.PathState.Bad);
                    }
                }
            }

            return false;
        }

        private bool BorderScanDerivation(HashSet<HashSet<Node>> markedGraphs)
        {
            while (markedGraphs.Count > 0)
            {
                HashSet<Node> graph = markedGraphs.First();
                markedGraphs.Remove(graph);
                if (graph.Count == 0) continue;

                List<Node> border = Node.ClockwiseBorder(graph);
                int len = border.Count;

                List<List<Path>>[] linesByColor = new List<List<Path>>[Flow.Colors.Length];
                for (int colorIndex = 0; colorIndex < Flow.Colors.Length; colorIndex++)
                {
                    linesByColor[colorIndex] = new List<List<Path>>();
                }

                int lastColorPosition = -1;
                for (int thisPosition = 0; lastColorPosition < len /* <-- to go a little longer than one full rotation */; thisPosition++)
                {
                    int thisColorIndex = border[thisPosition % len].colorIndex;
                    if (thisColorIndex < 0) continue; //no color

                    if (lastColorPosition >= 0 && thisColorIndex == border[lastColorPosition % len].colorIndex && thisPosition >= lastColorPosition + 2) //if a candidate coloring
                    {
                        List<Path> line = new List<Path>();
                        for (int i = lastColorPosition; i < thisPosition; i++)
                        {
                            line.Add(Node.Between(border[i % len], border[(i + 1) % len]));
                        }

                        linesByColor[thisColorIndex].Add(line);
                    }

                    lastColorPosition = thisPosition;
                }

                //check if all lines found in one color are the same line
                //if good, reduce to one. if bad, reduce to zero
                for (int colorIndex = 0; colorIndex < Flow.Colors.Count(); colorIndex++)
                {
                    List<List<Path>> lines = linesByColor[colorIndex];

                    for (int i = lines.Count - 2; i >= 0; i--)
                    {
                        HashSet<Path> line1Paths = new HashSet<Path>(lines[i]);
                        HashSet<Path> line2Paths = new HashSet<Path>(lines[i + 1]);

                        if (line1Paths.SetEquals(line2Paths))
                        {
                            lines.RemoveAt(i + 1); //remove the last path
                        }
                        else
                        {
                            lines.Clear();
                            continue;
                        }

                    }
                }

                bool moveFound = false;
                for (int colorIndex = 0; colorIndex < Flow.Colors.Count(); colorIndex++)
                {
                    if (linesByColor[colorIndex].Count == 1)
                    {
                        List<Path> line = linesByColor[colorIndex][0];
                        for (int i = 0; i < line.Count; i++)
                        {
                            CreateMove(line[i], Path.PathState.Good);
                        }
                        moveFound = true;
                    }
                }

                if (moveFound) return true;
            }

            return false;
        }


    }
}
