using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Flow
{
    internal class Grid
    {
        private Square[,] _squares;
        public Square getVertex(int x, int y) {
            if (!(0 <= x && x < Flow.GraphDimX && 0 <= y && y < Flow.GraphDimY)) return null;
            return _squares[x, y];
        }
        private Border[,] _horizontalBorders; //vertical displacement
        private Border[,] _verticalBorders; //horizontal displacement
        public Border getEdge(int x1, int y1, int x2, int y2)
        {
            if (x1 + 1 == x2 && y1 == y2)
            {
                if (!(0 <= x1 && x2 < Flow.GraphDimX + 1 && 0 <= y1 && y1 < Flow.GraphDimY)) return null;
                return _verticalBorders[x2, y2];
            }
            else if (y1 + 1 == y2 && x1 == x2)
            {
                if (!(0 <= y1 && y2 < Flow.GraphDimY + 1 && 0 <= x1 && x1 < Flow.GraphDimX)) return null;
                return _horizontalBorders[x2, y2];
            }
            else return null;
        }

        public Square[] EndpointSquares;
        public int NumEndpointSquares;

        public Border[] PortalBarriers;
        public int NumPortalBarriers;
        public Grid()
        {
            _squares = new Square[Flow.GraphDimX, Flow.GraphDimY];
            _horizontalBorders = new Border[Flow.GraphDimX, Flow.GraphDimY + 1];
            _verticalBorders = new Border[Flow.GraphDimX + 1, Flow.GraphDimY];

            for (int i = 0; i < Flow.GraphDimX; i++)
            {
                for (int j = 0; j < Flow.GraphDimY; j++)
                {
                    _squares[i, j] = new Square(i, j);
                }
            }

            for (int i = 0; i < Flow.GraphDimX; i++)
            {
                for (int j = 0; j < Flow.GraphDimY + 1; j++)
                {
                    _horizontalBorders[i, j] = new Border(i, j - 1, i, j);
                }
            }

            for (int i = 0; i < Flow.GraphDimX + 1; i++)
            {
                for (int j = 0; j < Flow.GraphDimY; j++)
                {
                    _verticalBorders[i, j] = new Border(i - 1, j, i, j);
                }
            }

            EndpointSquares = new Square[Flow.Colors.Length * 2];
            NumEndpointSquares = 0;
            PortalBarriers = new Border[Flow.Colors.Length * 2];
            NumPortalBarriers = 0;
        }

        public void Update()
        {
            Input.KeyboardInputType keyInput = Input.GetKeyboardInputType();
            if (keyInput == Input.KeyboardInputType.Endpoint || keyInput == Input.KeyboardInputType.Bridge || keyInput == Input.KeyboardInputType.Gone) //vertices
            {
                if (!Input.IsClickingOnNode()) return;

                (int, int) nodeCoordinates = Input.NodeCoordinates();
                Square node = _squares[nodeCoordinates.Item1, nodeCoordinates.Item2];

                if (node == null || node.Type != Square.SquareType.Standard) return;

                if (keyInput == Input.KeyboardInputType.Endpoint)
                {
                    node.Type = Square.SquareType.Endpoint;
                    node.ColorIndex = NumEndpointSquares / 2;
                    EndpointSquares[NumEndpointSquares] = node;
                    NumEndpointSquares++;
                }
                else if (keyInput == Input.KeyboardInputType.Bridge)
                {
                    node.Type = Square.SquareType.Bridge;
                }
                else // Gone
                {
                    int x = node.X;
                    int y = node.Y;
                    _squares[nodeCoordinates.Item1, nodeCoordinates.Item2] = null;
                    BorderRemovals(x, y);
                }
            }
            else if (keyInput == Input.KeyboardInputType.Wall || keyInput == Input.KeyboardInputType.Portal) //edges
            {
                if (!Input.IsClickingOnEdge()) return;

                (int, int, int, int) edgePair = Input.EdgeCoordinates();
                Border edge;
                if (edgePair.Item1 + 1 == edgePair.Item3) //vertical edge
                {
                    edge = _verticalBorders[edgePair.Item3, edgePair.Item4];
                }
                else //horizontal edge
                {
                    edge = _horizontalBorders[edgePair.Item3, edgePair.Item4];
                }

                if (edge == null || edge.Type == Border.BorderType.Wall) return;

                if (keyInput == Input.KeyboardInputType.Wall && edge.Type == Border.BorderType.Standard)
                {
                    edge.Type = Border.BorderType.Wall;
                }
                else // Portal
                {
                    if (edge.Type == Border.BorderType.Standard)
                    {
                        edge.Type = Border.BorderType.Portal;
                        edge.ColorIndex = NumPortalBarriers / 2;
                        PortalBarriers[NumPortalBarriers] = edge;
                        NumPortalBarriers++;
                    }
                    else if (Input.JustClicked)
                    {
                        edge.PointFirst = !edge.PointFirst;
                    }
                }
            }
        }

        private void BorderRemovals(int x, int y)
        {
            //left
            if (x == 0 || _squares[x - 1, y] == null) _verticalBorders[x, y] = null;
            //right
            if (x == Flow.GraphDimX - 1 || _squares[x + 1, y] == null) _verticalBorders[x + 1, y] = null;
            //top
            if (y == 0 || _squares[x, y - 1] == null) _horizontalBorders[x, y] = null;
            //bottom
            if (y == Flow.GraphDimY - 1 || _squares[x, y + 1] == null) _horizontalBorders[x, y + 1] = null;
        }

        public void Finish()
        {
            foreach(Border border in _horizontalBorders)
            {
                if (border != null && border.Type == Border.BorderType.Standard &&
                    (getVertex(border.X1, border.Y1) == null || getVertex(border.X2, border.Y2) == null))
                {
                    border.Type = Border.BorderType.Wall;
                }
            }

            foreach (Border border in _verticalBorders)
            {
                if (border != null && border.Type == Border.BorderType.Standard &&
                    (getVertex(border.X1, border.Y1) == null || getVertex(border.X2, border.Y2) == null))
                {
                    border.Type = Border.BorderType.Wall;
                }
            }
        }

        public void Draw()
        {
            foreach (Square node in _squares)
            {
                if (node != null) node.Draw();
            }
            foreach(Border edge in _horizontalBorders)
            {
                if (edge != null) edge.Draw();
            }
            foreach(Border edge in _verticalBorders)
            {
                if (edge != null) edge.Draw();
            }
        }
    }
}
