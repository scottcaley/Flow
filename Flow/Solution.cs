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
        public Solution(Graph graph) : base(graph) { }

        public override void PerformMove()
        {
            
        }
    }
}
