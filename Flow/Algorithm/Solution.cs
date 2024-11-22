using Flow.UI;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Flow.Algorithm.Puzzle.Path;
using static Flow.Algorithm.Solution;

namespace Flow.Algorithm
{
    internal class Solution : Puzzle
    {

        public class Move
        {
            public enum Reason
            {
                Starter,

                NumPathsDerivation,
                AdjacentColorDerivation,
                BorderScanDerivation,

                SelfContactContradiction,
                MismatchedColorContradiction,
                EndpointPartitionContradiction,

                Guess,
                MaxGuesses,
                SolutionFound
            }

            public readonly Path path;
            public readonly PathState pathState;
            public readonly bool isCertain;
            public readonly Reason reason;

            public PathState previousPathState;
            public bool previousIsCertain;

            public Move(Path path, PathState newState, bool isCertain, Reason reason)
            {
                this.path = path;
                pathState = newState;
                this.isCertain = isCertain;
                this.reason = reason;
            }

            public void Execute()
            {
                previousPathState = path.pathState;
                path.pathState = pathState;
                previousIsCertain = path.isCertain;
                path.isCertain = isCertain;
            }

            public void ReverseExecute()
            {
                path.pathState = previousPathState;
                path.isCertain = previousIsCertain;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (!(obj is Move)) return false;
                Move other = (Move)obj;

                return path.Equals(other.path)
                    && pathState.Equals(other.pathState)
                    && isCertain.Equals(other.isCertain)
                    && reason.Equals(other.reason);
            }

            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 31 + (path != null ? path.GetHashCode() : 0);
                hash = hash * 31 + pathState.GetHashCode();
                hash = hash * 31 + isCertain.GetHashCode();
                hash = hash * 31 + reason.GetHashCode();
                return hash;
            }

        }



        private int NumGuesses
        {
            get { return _guessQueues.Count; }
        }
        private bool IsGuessing
        {
            get { return _guessQueues.Count > 0 || _completedMoves.Count > 0 && LastPath.pathState == PathState.Unknown; }
        }
        private Path LastPath
        {
            get { return _completedMoves.Peek().path; }
        }
        private bool NextMoveReady
        {
            get { return _incomingMoves.Count > 0 || _backedMoves.Count > 0; }
        }
        private bool IsBacked
        {
            get { return _backedMoves.Count > 0; }
        }




        private Queue<Move> _incomingMoves;
        private Stack<Move> _backedMoves;
        private Stack<Move> _completedMoves;
        private Stack<Move> _uncertainMoves;
        private int _numBack;

        private Stack<Queue<Move>> _guessQueues;
        private Stack<Dictionary<Move, int>> _markedGuess;
        private Move.Reason _cause;
        private int _maxGuesses;

        private SetSet<Node> _lines;
        private SetSet<Node> _partitions;

        private HashSet<Node> _markedNumPaths;
        private HashSet<Path> _markedAdjacentColor;
        private HashSet<Node> _markedBorderScan;



        public Solution(Grid graph) : base(graph)
        {

            _incomingMoves = new Queue<Move>();
            _backedMoves = new Stack<Move>();
            _completedMoves = new Stack<Move>();
            _uncertainMoves = new Stack<Move>();
            _numBack = 0;

            _guessQueues = new Stack<Queue<Move>>();
            _markedGuess = new Stack<Dictionary<Move, int>>();
            _markedGuess.Push(new Dictionary<Move, int>());
            _maxGuesses = 0;

            _lines = GenerateGraphs(_allNodes, true, false, false);
            _partitions = GenerateGraphs(_allNodes, true, true, false);

            _markedNumPaths = new HashSet<Node>(_allNodes);
            _markedAdjacentColor = new HashSet<Path>(_allPaths);
            _markedBorderScan = new HashSet<Node>(_allNodes.Where(node => node.isEndpoint));

            FindFirstMoves();
        }

        private static SetSet<Node> GenerateGraphs(HashSet<Node> nodes, bool includeGood = true, bool includeMaybe = true, bool includeBad = true)
        {
            SetSet<Node> graphs = new SetSet<Node>();
            foreach (Node node in nodes)
            {
                if (!graphs.ContainsInSet(node))
                {
                    graphs.Add(node.FindGraph(includeGood, includeMaybe, includeBad));
                }
            }
            return graphs;
        }

        private void FindFirstMoves()
        {
            foreach (Path path in _allPaths)
            {
                if (path is BridgePath)
                {
                    CreateMove(path, PathState.Good, Move.Reason.Starter);
                }
            }
        }


        private void CreateMove(Path path, PathState pathState, Move.Reason reason)
        {
            _incomingMoves.Enqueue(new Move(path, pathState, !IsGuessing, reason));
        }
        private void CreateUndo(Path path)
        {
            _incomingMoves.Enqueue(new Move(path, PathState.Unknown, !IsGuessing, _cause));
        }
        private void CreateOpposite(Path path)
        {
            PathState currentState = path.pathState;
            PathState oppositeState;
            switch (currentState)
            {
                case PathState.Good:
                    oppositeState = PathState.Bad;
                    break;
                case PathState.Bad:
                    oppositeState = PathState.Good;
                    break;
                default:
                    oppositeState = PathState.Unknown;
                    break;
            }
            _incomingMoves.Enqueue(new Move(path, oppositeState, !IsGuessing, _cause));
        }
        private void CreateConfirmation(Path path)
        {
            PathState currentState = path.pathState;
            _incomingMoves.Enqueue(new Move(path, currentState, true, Move.Reason.SolutionFound));
        }



        private void UpdateDataStructures(Move move)
        {
            Path path = move.path;
            PathState previousState = move.previousPathState;

            Node node1 = path.node1;
            Node node2 = path.node2;
            Node[] nodes = { node1, node2 };

            if (previousState == PathState.Good ^ path.pathState == PathState.Good)
            {
                HashSet<Node> oldSet = _lines.FindSet(node1);
                _lines.Remove(oldSet);
                oldSet = _lines.FindSet(node2);
                _lines.Remove(oldSet);

                HashSet<Node> line1 = node1.FindGraph(true, false, false);
                _lines.Add(line1);
                if (!line1.Contains(node2)) _lines.Add(node2.FindGraph(true, false, false));
            }

            if (previousState == PathState.Bad ^ path.pathState == PathState.Bad)
            {
                HashSet<Node> oldSet = _partitions.FindSet(node1);
                _partitions.Remove(oldSet);
                _markedBorderScan.ExceptWith(oldSet);
                oldSet = _partitions.FindSet(node2);
                _partitions.Remove(oldSet);
                _markedBorderScan.ExceptWith(oldSet);

                HashSet<Node> partition1 = node1.FindGraph(true, true, false);
                _partitions.Add(partition1);

                foreach (Node node in partition1)
                {
                    if (!node.IsSolved)
                    {
                        _markedBorderScan.UnionWith(partition1.Where(node => node.isEndpoint));
                        break;
                    }
                }

                if (!partition1.Contains(node2))
                {
                    HashSet<Node> partition2 = node2.FindGraph(true, true, false);
                    _partitions.Add(partition2);
                    foreach (Node node in partition2)
                    {
                        if (!node.IsSolved)
                        {
                            _markedBorderScan.UnionWith(partition2.Where(node => node.isEndpoint));
                            break;
                        }
                    }
                }
            }

            foreach (Node node in nodes)
            {
                if (!node.IsSolved)
                {
                    _markedNumPaths.Add(node);
                    _markedAdjacentColor.Add(Node.Between(node1, node2));

                    foreach (Path partitionPath in node.PathSet(true, true, false))
                    {
                        if (partitionPath.pathState == PathState.Unknown) _markedAdjacentColor.Add(partitionPath);
                    }

                    if (path.pathState != PathState.Unknown)
                    {
                        foreach (Path maybePath in node.PathSet(false, true, false))
                        {
                            _markedGuess.Peek()[new Move(maybePath, PathState.Good, false, Move.Reason.Guess)] = 0;
                            _markedGuess.Peek()[new Move(maybePath, PathState.Bad, false, Move.Reason.Guess)] = 0;
                        }
                    }
                }
            }
        }

        private void UpdateColors(Move move)
        {
            Path path = move.path;
            PathState previousState = move.previousPathState;
            if (previousState == PathState.Good ^ path.pathState == PathState.Good)
            {
                Node.UpdateGraphColor(_lines.FindSet(path.node1));
                Node.UpdateGraphColor(_lines.FindSet(path.node2));
            }
        }





        public override void Forward()
        {
            //prepare the move
            if (!NextMoveReady && IsGuessing && !Check()) // if an error found
            {
                HandleBadGuess();
            }
            else if (!NextMoveReady && VerifySolution())
            {
                ValidatePaths();
            }
            else if (!NextMoveReady && !Derive()) // if no move found
            {
                IterativeDFS();
            }

            PerformNextMove();
        }

        private void PerformNextMove()
        {
            Move move;
            bool wasBacked = IsBacked;
            if (wasBacked) move = _backedMoves.Pop();
            else if (NextMoveReady) move = _incomingMoves.Dequeue();
            else return;

            move.Execute();
            _completedMoves.Push(move);
            if (!move.isCertain && move.pathState != PathState.Unknown && !wasBacked) _uncertainMoves.Push(move);

            UpdateDataStructures(move);
            UpdateColors(move);

            if (_numBack > 0) _numBack--;

            _actionLine = GenerateActionLine(move);
        }

        public override void Back()
        {
            UndoPreviousMove();
        }

        private void UndoPreviousMove()
        {
            if (_completedMoves.Count == 0) return;

            Move move = _completedMoves.Pop();
            move.ReverseExecute();
            _backedMoves.Push(move);

            UpdateDataStructures(move);
            UpdateColors(move);

            _numBack++;

            _actionLine = "Undoing";
        }

        private void ValidatePaths()
        {
            foreach (Path path in _allPaths)
            {
                if (!path.isCertain) CreateConfirmation(path);
            }
        }

        private int GuessQueuesTotalCount()
        {
            int count = 0;
            foreach (Queue<Move> queue in _guessQueues)
            {
                count += queue.Count;
            }
            return count;
        }

        private int MarkedGuessTotalCount()
        {
            int count = 0;
            foreach (Dictionary<Move, int> dictionary in _markedGuess)
            {
                count += dictionary.Count;
            }
            return count;
        }

        private string GenerateActionLine(Move move)
        {
            return "Move " + _completedMoves.Count + ": " + move.path + " changed from " + (move.previousIsCertain ? "" : "un") + "certainly " + move.previousPathState
                + " to " + (move.isCertain ? "" : "un") + "certainly " + move.pathState + " via " + move.reason /*
                + "\n" + _guessQueues.Count + ",  " + _backedMoves.Count + ",  " + _completedMoves.Count + ",  " + _uncertainMoves.Count
                + "\n" + _guessQueues.Count + ",  " + GuessQueuesTotalCount() + ",  " + _markedGuess.Count + ",  " + MarkedGuessTotalCount()
                + "\n" + _lines.SetsTotalCount() + ",  " + _partitions.SetsTotalCount()
                + "\n" + _markedNumPaths.Count + ",  " + _markedAdjacentColor.Count + ",  " + _markedBorderScan.Count*/;
        }

        public override bool IsFinished()
        {
            return _solutionFound && !NextMoveReady && !IsBacked;
        }


        /**
         * DERIVATIONS
         */

        private bool Derive()
        {
            return NumPathsDerivation(_markedNumPaths)
                || AdjacentColorDerivation(_markedAdjacentColor)
                || BorderScanDerivation(_markedBorderScan);
        }

        private bool NumPathsDerivation(HashSet<Node> markedNodes)
        {
            while (markedNodes.Count > 0)
            {
                Node node = markedNodes.First();
                markedNodes.Remove(node);
                if (node.IsSolved) continue;

                if (node.numPaths == node.PathCount(true, true, false))
                {
                    foreach (Path maybePath in node.PathSet(false, true, false))
                    {
                        CreateMove(maybePath, PathState.Good, Move.Reason.NumPathsDerivation);
                    }
                    return true;
                }
                else if (node.numPaths == node.PathCount(true, false, false))
                {
                    foreach (Path maybePath in node.PathSet(false, true, false))
                    {
                        CreateMove(maybePath, PathState.Bad, Move.Reason.NumPathsDerivation);
                    }
                    return true;
                }
            }

            return false;
        }

        private bool AdjacentColorDerivation(HashSet<Path> markedPaths)
        {
            while (markedPaths.Count > 0)
            {
                Path path = markedPaths.First();
                markedPaths.Remove(path);
                if (path.pathState != PathState.Unknown) continue;

                Node node1 = path.node1;
                Node node2 = path.node2;

                if (node1.colorIndex >= 0 && node2.colorIndex >= 0)
                {
                    if (node1.colorIndex == node2.colorIndex)
                    {
                        CreateMove(path, PathState.Good, Move.Reason.AdjacentColorDerivation);
                    }
                    else
                    {
                        CreateMove(path, PathState.Bad, Move.Reason.AdjacentColorDerivation);
                    }
                    return true;
                }
            }

            return false;
        }

        private bool BorderScanDerivation(HashSet<Node> markedEndpoints)
        {
            while (markedEndpoints.Count > 0)
            {
                Node endpoint = markedEndpoints.First();
                markedEndpoints.Remove(endpoint);

                Node otherEndpoint = null;
                foreach (Node otherNode in markedEndpoints)
                {
                    if (otherNode.isEndpoint && otherNode != endpoint && otherNode.colorIndex == endpoint.colorIndex) otherEndpoint = otherNode;
                }
                markedEndpoints.Remove(otherEndpoint);

                if (_lines.FindSet(endpoint) == _lines.FindSet(otherEndpoint)) continue;

                Node end = endpoint.EndOfLine();
                Node otherEnd = otherEndpoint.EndOfLine();

                HashSet<Path> theBorderPath = null;
                bool uniqueBorder = false;

                Node[] ends = { end, otherEnd };
                for (int i = 0; i < 2; i++) //for the 2 starting points
                {
                    Node startingPoint = ends[i];
                    for (int pathIndex = 0; pathIndex < 4; pathIndex++) //for the 4 paths
                    {
                        Path path = startingPoint.paths[pathIndex]; //move up 3 lines
                        if (!(path == null || path.pathState == PathState.Bad)) continue;

                        bool[] directions = { true, false };
                        foreach (bool isClockwise in directions)
                        {
                            HashSet<Path> borderPath = startingPoint.TraverseBorder(pathIndex, _lines.FindSet(startingPoint), isClockwise);
                            if (borderPath != null) //if successful
                            {
                                if (theBorderPath != null && !theBorderPath.SetEquals(borderPath))
                                {
                                    uniqueBorder = false;
                                }
                                else if (theBorderPath == null)
                                {
                                    theBorderPath = borderPath;
                                    uniqueBorder = true;
                                }
                            }
                        }
                    }
                }

                if (uniqueBorder)
                {
                    foreach (Path path in theBorderPath)
                    {
                        if (path.pathState == PathState.Unknown) CreateMove(path, PathState.Good, Move.Reason.BorderScanDerivation);
                    }
                    return true;
                }

            }


            return false;
        }




        /**
         * CHECKS
         */

        private bool Check()
        {
            return SelfContactCheck(LastPath)
                && MismatchedColorCheck(LastPath)
                && EndpointPartitionCheck(LastPath)
                && RemovedAdjacencyCheck(LastPath);
        }

        /**
         * checks to make sure lines are not adjacent to themself by a maybe/bad edge
         */
        private bool SelfContactCheck(Path path)
        {
            if (path.pathState == PathState.Good)
            {
                HashSet<Node> line = _lines.FindSet(path.node1);

                foreach (Node node in line)
                {
                    HashSet<Node> otherNeighbors = node.Neighbors(false, true, true);
                    foreach (Node otherNode in otherNeighbors)
                    {
                        if (line.Contains(otherNode))
                        {
                            _cause = Move.Reason.SelfContactContradiction;
                            return false;
                        }
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
            if (path.pathState == PathState.Good)
            {
                HashSet<Node> line = _lines.FindSet(path.node1);
                HashSet<int> colorIndices = new HashSet<int>();

                foreach (Node node in line)
                {
                    colorIndices.Add(node.colorIndex);
                }

                if (colorIndices.Count > 1)
                {
                    _cause = Move.Reason.MismatchedColorContradiction;
                    return false;
                }
            }

            return true;
        }

        /**
         * checks all partitions (partitioned by bad paths),
         * every partition must have at least one endpoint
         * every endpoint must be found in the same partition as its counterpart
         */
        private bool EndpointPartitionCheck(Path path)
        {
            if (path.pathState == PathState.Bad)
            {
                Node[] pathNodes = { path.node1, path.node2 };
                for (int nodeIndex = 0; nodeIndex < 2; nodeIndex++)
                {
                    HashSet<Node> partition = _partitions.FindSet(pathNodes[nodeIndex]);
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
                    if (!allEven || !atLeastOneEndpoint)
                    {
                        _cause = Move.Reason.EndpointPartitionContradiction;
                        return false;
                    }
                }
            }
            else if (path.pathState == PathState.Good)
            {

            }

            return true;
        }


        private bool RemovedAdjacencyCheck(Path path)
        {
            return true;
        }



        /**
         * GUESSES
         */

        private void HandleBadGuess()
        {
            _incomingMoves.Clear();
            Move guessMove = _guessQueues.Peek().Peek();
            _guessQueues.Pop();
            _markedGuess.Pop();
            if (NumGuesses == 0)
            {
                _maxGuesses = 0;
            }

            Move lastUncertainMove = _uncertainMoves.Pop();
            while (lastUncertainMove != guessMove)
            {
                CreateUndo(lastUncertainMove.path);
                lastUncertainMove = _uncertainMoves.Pop();
            }
            CreateOpposite(lastUncertainMove.path);
        }

        private void IterativeDFS()
        {
            AdvanceGuessHierarchy();

            Move guessMove = _guessQueues.Peek().Peek(); //need to make sure this goes at the end of the incoming moves
            _incomingMoves.Enqueue(guessMove);
        }
        private void AdvanceGuessHierarchy()
        {
            if (NumGuesses == 0 || NumGuesses < _maxGuesses)
            {
                if (NumGuesses == 0) //either we just started guessing or a DFS iteration has finished at the iteration of length _maxGuesses
                {
                    _maxGuesses++;
                }
                _guessQueues.Push(GenerateGuessQueue());
                _markedGuess.Push(new Dictionary<Move, int>());
            }
            else //NumGuesses == _maxGuesses, NumGuesses > 0 most common case
            {
                AdvanceGuessQueue();
            }
        }

        private Queue<Move> GenerateGuessQueue()
        {
            Queue<Move> guessQueue = new Queue<Move>();

            HashSet<Move> completed = new HashSet<Move>();
            foreach (Move move in _markedGuess.Peek().Keys)
            {
                if (move.path.pathState == PathState.Unknown && _markedGuess.Peek()[move] + NumGuesses < _maxGuesses)
                {
                    guessQueue.Enqueue(move);
                }
                else if (move.path.isCertain)
                {
                    completed.Add(move);
                }
            }

            foreach (Move move in completed) _markedGuess.Peek().Remove(move);

            return guessQueue;
        }

        private void AdvanceGuessQueue()
        {
            Move previousMove = _guessQueues.Peek().Dequeue();

            Dictionary<Move, int> top = _markedGuess.Pop();
            _markedGuess.Peek()[previousMove]++;
            _markedGuess.Push(top);

            CancelGuess(previousMove);

            if (_guessQueues.Peek().Count == 0) //out of guesses here, need to fall back
            {
                _guessQueues.Pop();
                _markedGuess.Pop();
                if (_guessQueues.Count > 0)
                {
                    AdvanceGuessQueue();
                }
                else
                {
                    AdvanceGuessHierarchy(); //gotta start fresh
                }
            }
        }

        /**
         * Called when an error is found or out of guesses
         * Does not modify guess data
         * Only adds cancellation moves to _incomingMoves
         * Returns true if a correct move was found
         */
        private void CancelGuess(Move guessMove)
        {
            _cause = Move.Reason.MaxGuesses;
            Move lastUncertainMove = _uncertainMoves.Pop();
            while (lastUncertainMove != guessMove)
            {
                CreateUndo(lastUncertainMove.path);
                lastUncertainMove = _uncertainMoves.Pop();
            }
            CreateUndo(lastUncertainMove.path);
        }


    }
}
