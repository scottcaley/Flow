using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    internal class Node
    {
        public enum NodeType
        {
            Standard,
            Endpoint,
            Bridge,
            Gone
        }
        public NodeType Type;

        public readonly int X;
        public readonly int Y;
        public int ColorIndex;
        public Node(int x, int y)
        {
            X = x;
            Y = y; 
            Type = NodeType.Standard;
        }

        public void Update()
        {
            
        }

        public void Draw()
        {
            if (Type != NodeType.Gone) FillSquare();

            if (Type == NodeType.Endpoint)
            {
                Flow.Sd.DrawInnerRectangle(X, Y, Flow.Colors[ColorIndex]);
            }
        }

        private void FillSquare()
        {
            Flow.Sd.DrawOuterRectangle(X, Y, Color.Black);
        }

        
    }
}
