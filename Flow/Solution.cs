using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    internal class Solution : Puzzle
    {
        private class Move
        {
            Path path;
            Path.PathState pathState;
            bool isGuess;

            public Move(Path path, Path.PathState pathState, bool isGuess)
            {
                this.path = path;
                this.pathState = pathState;
                this.isGuess = isGuess;
            }

            public void Perform()
            {
                path.pathState = pathState;
                path.isGuess = isGuess;
            }

            public void Undo()
            {
                path.pathState = Path.PathState.Maybe;
                path.isGuess = false;
            }
        }

        Queue<Move> _moves;
        Stack<Move> _guesses;
        bool isGuessing;
        public Solution(Graph graph) : base(graph)
        {
            _moves = new Queue<Move>();
            _guesses = new Stack<Move>();
            isGuessing = false;
        }

        public override void PerformMove()
        {
            if (_moves.Count > 0)
            {
                _moves.Dequeue().Perform();
                return;
            }

            if (true)
            {

            }

            _moves.Dequeue().Perform();
        }
    }
}
