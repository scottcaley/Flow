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
        public bool PointFirst;
        public Edge(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Type = EdgeType.Standard;
            ColorIndex = -1;
            PointFirst = true;
        }

        public void Draw()
        {
            if (Type == EdgeType.Standard)
            {
                Flow.Sd.DrawEdge(X1, Y1, X2, Y2, Color.White, false);
            }            
            else if (Type == EdgeType.Wall)
            {
                Flow.Sd.DrawEdge(X1, Y1, X2, Y2, Color.White, true);
            }
            else if (Type == EdgeType.Portal)
            {
                Flow.Sd.DrawEdge(X1, Y1, X2, Y2, Flow.Colors[ColorIndex], true);
                if (PointFirst) Flow.Sd.DrawPortalDirection(X1, Y1, X2, Y2, Flow.Colors[ColorIndex]);
                else Flow.Sd.DrawPortalDirection(X2, Y2, X1, Y1, Flow.Colors[ColorIndex]);
            }
        }
        
    }
}
