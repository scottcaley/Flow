﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    internal class Vertex
    {
        public enum VertexType
        {
            Standard,
            Endpoint,
            Bridge
        }
        public VertexType Type;

        public readonly int X;
        public readonly int Y;
        public int ColorIndex;
        public Vertex(int x, int y)
        {
            X = x;
            Y = y; 
            Type = VertexType.Standard;
        }

        public void Draw()
        {
            FillSquare();

            if (Type == VertexType.Endpoint)
            {
                Flow.Sd.DrawInnerRectangle(X, Y, Flow.Colors[ColorIndex]);
            }
            else if (Type == VertexType.Bridge)
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