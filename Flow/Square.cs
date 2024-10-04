using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    internal class Square
    {
        public enum SquareType
        {
            Standard,
            Endpoint,
            Bridge
        }
        public SquareType Type;

        public readonly int X;
        public readonly int Y;
        public int ColorIndex;
        public Square(int x, int y)
        {
            X = x;
            Y = y; 
            Type = SquareType.Standard;
        }

        public void Draw()
        {
            FillSquare();

            if (Type == SquareType.Endpoint)
            {
                Flow.Sd.DrawInnerRectangle(X, Y, Flow.Colors[ColorIndex]);
            }
            else if (Type == SquareType.Bridge)
            {
                Flow.Sd.DrawCross(X, Y, Color.White);
            }
        }

        private void FillSquare()
        {
            Flow.Sd.DrawOuterRectangle(X, Y, Color.Black);
        }

        
    }
}
