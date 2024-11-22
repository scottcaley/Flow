using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.UI
{
    internal class Border
    {
        public enum BorderType
        {
            Standard,
            Wall,
            Portal
        }

        public BorderType Type;

        public readonly int X1;
        public readonly int Y1;
        public readonly int X2;
        public readonly int Y2;

        public int ColorIndex;
        public bool PointFirst;
        public Border(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Type = BorderType.Standard;
            ColorIndex = -1;
            PointFirst = true;
        }

        public void Draw()
        {
            if (Type == BorderType.Standard)
            {
                Flow.Sd.DrawBorder(X1, Y1, X2, Y2, Color.White, false);
            }
            else if (Type == BorderType.Wall)
            {
                Flow.Sd.DrawBorder(X1, Y1, X2, Y2, Color.White, true);
            }
            else if (Type == BorderType.Portal)
            {
                Flow.Sd.DrawBorder(X1, Y1, X2, Y2, Flow.Colors[ColorIndex], true);
                if (PointFirst) Flow.Sd.DrawPortalDirection(X1, Y1, X2, Y2, Flow.Colors[ColorIndex]);
                else Flow.Sd.DrawPortalDirection(X2, Y2, X1, Y1, Flow.Colors[ColorIndex]);
            }
        }

    }
}
