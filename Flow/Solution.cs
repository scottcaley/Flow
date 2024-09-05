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

namespace Flow
{
    internal class Solution : Puzzle
    {
        private static HashSet<HashSet<Node>> GenerateGraphs(HashSet<Node> nodes, bool includeGood = true, bool includeMaybe = true, bool includeBad = true)
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
        static private bool ContainedInASet<T>(T element, HashSet<HashSet<T>> sets)
        {
            foreach (HashSet<T> set in sets)
            {
                if (set.Contains(element)) return true;
            }

            return false;
        }
        static private HashSet<T> FindSet<T>(T element, HashSet<HashSet<T>> sets)
        {
            foreach (HashSet<T> set in sets)
            {
                if (set.Contains(element)) return set;
            }

            return null;
        } 

        public class Move
        {
            public enum Reason
            {
                Starter,

                NumPaths,
                AdjacentColor,
                BorderScan,
                Tautology,

                SelfContact,
                MismatchedColor,
                EndpointPartition,

                Guess,
                UndoingGuess
            }

            public Path path;
            public Path.PathState pathState;
            public bool isCertain;
            public Reason reason;

            public Move(Path path, Path.PathState newState, bool isCertain, Reason reason)
            {
                this.path = path;
                this.pathState = newState;
                this.isCertain = isCertain;
                this.reason = reason;
            }

            public void Execute()
            {
                Path.PathState tempState = path.pathState;
                path.pathState = pathState;
                pathState = tempState;

                bool temp = path.isCertain;
                path.isCertain = isCertain;
                isCertain = temp;
            }

        }

        public class LogicCaseSet<T> : HashSet<Dictionary<T, bool>>
        {

            public LogicCaseSet() : base() { }


            private HashSet<T> VariableSet()
            {
                HashSet<T> variables = new HashSet<T>();

                foreach (Dictionary<T, bool> map in this)
                {
                    foreach (T variable in map.Keys)
                    {
                        variables.Add(variable);
                    }
                }

                return variables;
            }

            private static Dictionary<T, bool> GenerateCase(List<T> variableList, int caseNumber)
            {
                Dictionary<T, bool> thisCase = new Dictionary<T, bool>();

                for (int index = 0; index < (1 << variableList.Count); index++)
                {
                    int digit = (caseNumber >> (variableList.Count - 1 - index));
                    bool variableValue = !(digit % 2 == 0);
                    thisCase.Add(variableList[index], variableValue);
                }

                return thisCase;
            }

            private static bool IsSubset(Dictionary<T, bool> subset, Dictionary<T, bool> superset)
            {
                foreach (T variable in subset.Keys)
                {
                    if (!superset.ContainsKey(variable)) return false;
                    if (subset[variable] != superset[variable]) return false;
                }

                return true;
            }

            public void NonRedundantAdd(Dictionary<T, bool> set)
            {
                if (this.Contains(set)) return;

                foreach(Dictionary<T, bool> possibleSuperset in this)
                {
                    if (IsSubset(set, possibleSuperset)) return;
                }

                HashSet<Dictionary<T, bool>> subsets = new HashSet<Dictionary<T, bool>>();
                foreach(Dictionary<T, bool> possibleSubset in this)
                {
                    if (IsSubset(possibleSubset, set)) subsets.Add(possibleSubset);
                }
                this.ExceptWith(subsets);
            }

            public bool OrAllCases()
            {

                List<T> variableList = VariableSet().ToList();
                for (int caseNumber = 0; caseNumber < (1 << variableList.Count); caseNumber++)
                {
                    Dictionary<T, bool> variableCase = GenerateCase(variableList, caseNumber);
                    bool solutionFound = false;
                    foreach (Dictionary<T, bool> possibleSolution in this)
                    {
                        if (IsSubset(possibleSolution, variableCase)) solutionFound = true;
                    }
                    if (!solutionFound) return false;
                }

                return true;
            }
        }

        private int NumGuesses
        {
            get { return _guesses.Count; }
        }
        private bool IsGuessing
        {
            get { return (_guesses.Count > 0); }
        }
        private Path LastPath
        {
            get { return _completedMoves.Peek().path; }
        }
        private bool NextMoveReady
        {
            get { return _incomingMoves.Count > 0; }
        }
        private bool IsBacked
        {
            get { return _numBack > 0; }
        }

        


        private Stack<Move> _incomingMoves;
        private Stack<Move> _completedMoves;
        private int _numBack;

        private Stack<(List<Path>, int)> _guesses;
        private Dictionary<Path, Dictionary<Path, Path.PathState>> _goodConditionals;
        private Dictionary<Path, Dictionary<Path, Path.PathState>> _badConditionals;
        private Move.Reason _cause;
        private int _maxGuesses;

        private HashSet<HashSet<Node>> _lines;
        private HashSet<HashSet<Node>> _partitions;

        private HashSet<Node> _markedNumPaths;
        private HashSet<Path> _markedAdjacentColor;
        private HashSet<HashSet<Node>> _markedBorderScan;



        public Solution(Graph graph) : base(graph)
        {

            _incomingMoves = new Stack<Move>();
            _completedMoves = new Stack<Move>();
            _numBack = 0;

            _guesses = new Stack<(List<Path>, int)>();
            _goodConditionals = new Dictionary<Path, Dictionary<Path, Path.PathState>>();
            _badConditionals = new Dictionary<Path, Dictionary<Path, Path.PathState>>();
            _maxGuesses = 0;

            _lines = GenerateGraphs(_allNodes, true, false, false);
            _partitions = GenerateGraphs(_allNodes, true, true, false);

            _markedNumPaths = new HashSet<Node>(_allNodes);
            _markedAdjacentColor = new HashSet<Path>(_allPaths);
            _markedBorderScan = new HashSet<HashSet<Node>>(_partitions);
            
            FindFirstMoves();
        }



        private void CreateMove(Path path, Path.PathState pathState, Move.Reason reason)
        {
            _incomingMoves.Push(new Move(path, pathState, !IsGuessing, reason));
        }
        private void CreateGuess(Path path, Path.PathState pathState, Move.Reason reason)
        {
            Move guess = new Move(path, pathState, false, reason);
            _incomingMoves.Push(guess);
        }
        private void CreateUndo(Path path)
        {
            _incomingMoves.Push(new Move(path, Path.PathState.Maybe, false, _cause));
        }
        private void CreateUndo(Path path, Move.Reason reason)
        {
            _incomingMoves.Push(new Move(path, Path.PathState.Maybe, false, reason));
        }


        private void FindFirstMoves()
        {
            foreach (Path path in _allPaths)
            {
                if (path is BridgePath)
                {
                    CreateMove(path, Path.PathState.Good, Move.Reason.Starter);
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

                HashSet<Node> line1 = node1.FindGraph(true, false, false);
                _lines.Add(line1);
                if (!line1.Contains(node2)) _lines.Add(node2.FindGraph(true, false, false));
            }

            if (previousState == Path.PathState.Bad ^ path.pathState == Path.PathState.Bad)
            {
                HashSet<Node> oldSet = FindSet(node1, _partitions);
                _partitions.Remove(oldSet);
                _markedBorderScan.Remove(oldSet);
                oldSet = FindSet(node2, _partitions);
                _partitions.Remove(oldSet);
                _markedBorderScan.Remove(oldSet);

                HashSet<Node> partition1 = node1.FindGraph(true, true, false);
                _partitions.Add(partition1);
                foreach (Node node in partition1)
                {
                    if (!node.IsSolved)
                    {
                        _markedBorderScan.Add(partition1);
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
                            _markedBorderScan.Add(partition2);
                            break;
                        }
                    }
                }
            }

            if (!node1.IsSolved)
            {
                _markedNumPaths.Add(node1);
                _markedAdjacentColor.Add(Node.Between(node1, node2));

                foreach (Path partitionPath in node1.PathSet(true, true, false))
                {
                    if (partitionPath.pathState == Path.PathState.Maybe) _markedAdjacentColor.Add(partitionPath);
                }
            }

            if (!node2.IsSolved)
            {
                _markedNumPaths.Add(node2);
                _markedAdjacentColor.Add(Node.Between(node2, node1));

                foreach (Path partitionPath in node2.PathSet(true, true, false))
                {
                    if (partitionPath.pathState == Path.PathState.Maybe) _markedAdjacentColor.Add(partitionPath);
                }
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
            if (IsBacked)
            {
                //nothing, next move is already there
            }
            else if (IsGuessing && !Check()) // if an error found
            {
                CancelGuess();
            }
            else if (!NextMoveReady && !Derive()) // if no move found
            {
                MakeNextGuess();
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

            if (_numBack > 0) _numBack--;

            _actionLine = GenerateActionLine(move);

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

            _numBack++;

            _actionLine = "Undoing";

            return true;
        }

        private String GenerateActionLine(Move move)
        {
            return move.path + " changed from " + move.pathState + " to " + move.path.pathState + " via " + move.reason;
        }


        /**
         * DERIVATIONS
         */

        private bool Derive()
        {
            return (NumPathsDerivation(_markedNumPaths)
                || AdjacentColorDerivation(_markedAdjacentColor)
                || BorderScanDerivation(_markedBorderScan)
                || TautologyDerivation());
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
                        CreateMove(maybePath, Path.PathState.Good, Move.Reason.NumPaths);
                    }
                    return true;
                }
                else if (node.numPaths == node.PathCount(true, false, false))
                {
                    foreach (Path maybePath in node.PathSet(false, true, false))
                    {
                        CreateMove(maybePath, Path.PathState.Bad, Move.Reason.NumPaths);
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
                if (path.pathState != Path.PathState.Maybe) continue;

                Node node1 = path.node1;
                Node node2 = path.node2;

                if (node1.colorIndex >= 0 && node2.colorIndex >= 0)
                {
                    if (node1.colorIndex == node2.colorIndex)
                    {
                        CreateMove(path, Path.PathState.Good, Move.Reason.AdjacentColor);
                    }
                    else
                    {
                        CreateMove(path, Path.PathState.Bad, Move.Reason.AdjacentColor);
                    }
                    return true;
                }
            }

            return false;
        }

        private bool BorderScanDerivation(HashSet<HashSet<Node>> markedGraphs)
        {
            return false;
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
                            CreateMove(line[i], Path.PathState.Good, Move.Reason.BorderScan);
                        }
                        moveFound = true;
                    }
                }

                if (moveFound) return true;
            }

            return false;
        }

        private bool TautologyDerivation()
        {
            return false;
        }



        /**
         * CHECKS
         */

        private bool Check()
        {
            return (SelfContactCheck(LastPath)
                && MismatchedColorCheck(LastPath)
                && EndpointPartitionCheck(LastPath));
        }

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
                        if (line.Contains(otherNode))
                        {
                            _cause = Move.Reason.SelfContact;
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
            if (path.pathState == Path.PathState.Good)
            {
                HashSet<Node> line = FindSet(path.node1, _lines);
                HashSet<int> colorIndices = new HashSet<int>();

                foreach (Node node in line)
                {
                    colorIndices.Add(node.colorIndex);
                }

                if (colorIndices.Count > 1)
                {
                    _cause = Move.Reason.MismatchedColor;
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
                    if (!allEven || !atLeastOneEndpoint)
                    {
                        _cause = Move.Reason.EndpointPartition;
                        return false;
                    }
                }
            }

            return true;
        }




        /**
         * GUESSES
         */

        private List<Path> GenerateGuessList()
        {
            List<Path> guessList = new List<Path>();

            foreach (Path path in _allPaths)
            {
                if (path.pathState == Path.PathState.Maybe) guessList.Add(path);
            }

            return guessList;
        }

        private void PrepareNextGuess()
        {
            if (NumGuesses == 0)
            {
                _maxGuesses++;
                List<Path> guessList = GenerateGuessList();
                _guesses.Push((guessList, 0));
            }
            else if (NumGuesses < _maxGuesses)
            {
                List<Path> guessList = GenerateGuessList();
                _guesses.Push((guessList, 0));
            }
            else //NumGuesses == _maxGuesses, most common case
            {
                (List<Path>, int) guessData = _guesses.Pop();
                List<Path> guessList = guessData.Item1;
                int newGuessNumber = guessData.Item2 + 1;
                if (newGuessNumber < 2 * guessList.Count)
                {
                    _guesses.Push((guessList, newGuessNumber));
                }
                else //newGuessNumber == 2 * guessList.Count, need to fall back
                {
                    if (NumGuesses > 0) //remember this value has changed
                    {
                        CancelGuess();
                    }
                    PrepareNextGuess();
                }
            }
        }

        private void CancelAllGuesses()
        {
            if (_guesses.Count == 0) return;

            (List<Path>, int) firstGuessData = _guesses.Pop();
            while (_guesses.Count > 0) firstGuessData = _guesses.Pop();

            Path firstGuessPath = firstGuessData.Item1[firstGuessData.Item2 / 2];
            foreach (Move move in _completedMoves) //from the top
            {
                if (!move.isCertain) CreateUndo(move.path);

                if (move.path == firstGuessPath)
                {
                    break;
                }
            }

            _maxGuesses = 0;
        }

        private void CancelGuess()
        {
            (List<Path>, int) guessData = _guesses.Peek();
            List<Path> guessList = guessData.Item1;
            int guessNumber = guessData.Item2;
            Path guessPath = guessList[guessNumber / 2];

            bool correctMoveFound = false;
            foreach (Move move in _completedMoves) //from the top
            {
                if (!move.isCertain) CreateUndo(move.path);
                else correctMoveFound = true;

                if (move.path == guessPath)
                {
                    break;
                }
            }

            if (correctMoveFound)
            {
                CancelAllGuesses();
            }


        }

        private void MakeNextGuess()
        {
            PrepareNextGuess();

            (List<Path>, int) guessData = _guesses.Peek();
            List<Path> guessList = guessData.Item1;
            int guessNumber = guessData.Item2;
            CreateGuess(guessList[guessNumber / 2], (guessNumber % 2 == 0) ? Path.PathState.Good : Path.PathState.Bad, Move.Reason.Guess);
        }

    }
}
