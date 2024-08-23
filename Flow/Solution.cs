using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    internal class Solution
    {
        private class SolutionEdge
        {
            public enum SolutionEdgeState
            {
                Bad,
                Maybe,
                Good
            }

            SolutionEdgeState _solutionEdgeType;
            Flow.Direction _direction;
            Color _color;


        }
        private class SolutionNode
        {
            private readonly Node _node;
            private bool _isSolved;
            private SolutionEdge[] _solutionEdges;
            SolutionNode(Node node)
            {
                _node = node;
                _isSolved = false;
                _solutionEdges = new SolutionEdge[4];
            }

            public void Draw()
            {

            }
        }

        private SolutionNode[,] _solutionNodes;

        bool _isGuessing;
        public Solution(Graph graph)
        {
            _solutionNodes = new SolutionNode[Flow.GraphDim, Flow.GraphDim];
            for (int i = 0; i < Flow.GraphDim; i++)
            {
                for (int j = 0; j < Flow.GraphDim; j++)
                {
                    
                }
            }
            _isGuessing = false;
        }


        public void Draw()
        {
            for (int i = 0; i < Flow.GraphDim; i++)
            {
                for (int j = 0; j < Flow.GraphDim; j++)
                {
                    if (_solutionNodes[i, j] != null) _solutionNodes[i, j].Draw();
                }
            }
        }

        public void Move()
        {
            
        }


    }
}
