using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    internal class Edge
    {
        public enum EdgeType
        {
            Standard,
            Wall,
            Portal
        }

        public EdgeType Type;

        public readonly int X1;
        public readonly int Y1;
        public readonly int X2;
        public readonly int Y2;

        public int ColorIndex;


        public Edge(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Type = EdgeType.Standard;
            ColorIndex = -1;
        }

        public void Draw()
        {
            if (Type == EdgeType.Standard)
            {
                Flow.Sd.DrawThinLine(X1, Y1, X2, Y2, Color.White);
            }            
            else if (Type == EdgeType.Wall)
            {
                Flow.Sd.DrawThickLine(X1, Y1, X2, Y2, Color.White);
            }
            else if (Type == EdgeType.Portal)
            {
                Flow.Sd.DrawThickLine(X1, Y1, X2, Y2, Flow.Colors[ColorIndex]);
            }
        }
        
    }
}
