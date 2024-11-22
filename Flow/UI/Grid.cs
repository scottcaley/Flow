using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Flow.UI
{
    internal class Grid
    {
        private Square[,] _squares;
        public Square GetSquare(int x, int y)
        {
            if (!(0 <= x && x < Flow.GraphDimX && 0 <= y && y < Flow.GraphDimY)) return null;
            return _squares[x, y];
        }
        private Border[,] _horizontalBorders; //vertical displacement
        private Border[,] _verticalBorders; //horizontal displacement
        public Border GetBorder(int x1, int y1, int x2, int y2)
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
            if (!Input.JustClicked) return;

            Input.KeyboardInputType keyInput = Input.GetKeyboardInputType();
            if (keyInput == Input.KeyboardInputType.Endpoint || keyInput == Input.KeyboardInputType.Bridge || keyInput == Input.KeyboardInputType.Gone) //vertices
            {
                if (!Input.IsClickingOnSquare()) return;

                (int, int) squareCoordinates = Input.SquareCoordinates();
                int x = squareCoordinates.Item1;
                int y = squareCoordinates.Item2;

                if (keyInput == Input.KeyboardInputType.Endpoint)
                {
                    AddEndpoint(x, y);
                }
                else if (keyInput == Input.KeyboardInputType.Bridge)
                {
                    AddBridge(x, y);
                }
                else // Gone
                {
                    AddGone(x, y);
                }
            }
            else if (keyInput == Input.KeyboardInputType.Wall || keyInput == Input.KeyboardInputType.Portal) //borders
            {
                if (!Input.IsClickingOnBorder()) return;

                (int, int, int, int) borderPair = Input.BorderCoordinates();
                int x1 = borderPair.Item1;
                int y1 = borderPair.Item2;
                int x2 = borderPair.Item3;
                int y2 = borderPair.Item4;

                if (keyInput == Input.KeyboardInputType.Wall)
                {
                    AddWall(x1, y1, x2, y2);
                }
                else // Portal
                {
                    AddPortal(x1, y1, x2, y2);
                }
            }
        }

        private void AddEndpoint(int x, int y)
        {
            Square square = _squares[x, y];
            if (square == null || square.Type != Square.SquareType.Standard) return;

            square.Type = Square.SquareType.Endpoint;
            square.ColorIndex = NumEndpointSquares / 2;
            EndpointSquares[NumEndpointSquares] = square;
            NumEndpointSquares++;
        }

        private void AddBridge(int x, int y)
        {
            Square square = _squares[x, y];
            if (square == null || square.Type != Square.SquareType.Standard) return;

            square.Type = Square.SquareType.Bridge;
        }

        private void AddGone(int x, int y)
        {
            Square square = _squares[x, y];
            if (square == null || square.Type != Square.SquareType.Standard) return;

            _squares[x, y] = null;
            BorderRemovals(x, y);
        }

        private void AddWall(int x1, int y1, int x2, int y2)
        {
            Border border = GetBorder(x1, y1, x2, y2);
            if (border == null || border.Type != Border.BorderType.Standard) return;

            border.Type = Border.BorderType.Wall;
        }

        private void AddPortal(int x1, int y1, int x2, int y2)
        {
            Border border = GetBorder(x1, y1, x2, y2);
            if (border == null || border.Type == Border.BorderType.Wall) return; //different condition here

            if (border.Type == Border.BorderType.Standard)
            {
                border.Type = Border.BorderType.Portal;
                border.ColorIndex = NumPortalBarriers / 2;
                PortalBarriers[NumPortalBarriers] = border;
                NumPortalBarriers++;
            }
            else if (border.Type == Border.BorderType.Portal)
            {
                border.PointFirst = !border.PointFirst;
            }
        }


        public void LoadFile(string jsonFilePath)
        {
            string jsonString = File.ReadAllText(jsonFilePath);

            // Parse the JSON
            var jsonDoc = JsonDocument.Parse(jsonString);

            // Loop through each action in the JSON
            foreach (var action in jsonDoc.RootElement.GetProperty("actions").EnumerateArray())
            {
                string type = action.GetProperty("type").GetString();
                switch (type)
                {
                    case "AddEndpoint":
                        int x = action.GetProperty("x").GetInt32();
                        int y = action.GetProperty("y").GetInt32();
                        AddEndpoint(x, y);
                        break;

                    case "AddBridge":
                        x = action.GetProperty("x").GetInt32();
                        y = action.GetProperty("y").GetInt32();
                        AddBridge(x, y);
                        break;

                    case "AddGone":
                        x = action.GetProperty("x").GetInt32();
                        y = action.GetProperty("y").GetInt32();
                        AddGone(x, y);
                        break;

                    case "AddWall":
                        int x1 = action.GetProperty("x1").GetInt32();
                        int y1 = action.GetProperty("y1").GetInt32();
                        int x2 = action.GetProperty("x2").GetInt32();
                        int y2 = action.GetProperty("y2").GetInt32();
                        AddWall(x1, y1, x2, y2);
                        break;

                    case "AddPortal":
                        x1 = action.GetProperty("x1").GetInt32();
                        y1 = action.GetProperty("y1").GetInt32();
                        x2 = action.GetProperty("x2").GetInt32();
                        y2 = action.GetProperty("y2").GetInt32();
                        AddPortal(x1, y1, x2, y2);
                        break;

                    default:
                        Console.WriteLine($"Unknown action type: {type}");
                        break;
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
            foreach (Border border in _horizontalBorders)
            {
                if (border != null && border.Type == Border.BorderType.Standard &&
                    (GetSquare(border.X1, border.Y1) == null || GetSquare(border.X2, border.Y2) == null))
                {
                    border.Type = Border.BorderType.Wall;
                }
            }

            foreach (Border border in _verticalBorders)
            {
                if (border != null && border.Type == Border.BorderType.Standard &&
                    (GetSquare(border.X1, border.Y1) == null || GetSquare(border.X2, border.Y2) == null))
                {
                    border.Type = Border.BorderType.Wall;
                }
            }
        }

        public void Draw()
        {
            foreach (Square square in _squares)
            {
                if (square != null) square.Draw();
            }
            foreach (Border border in _horizontalBorders)
            {
                if (border != null) border.Draw();
            }
            foreach (Border border in _verticalBorders)
            {
                if (border != null) border.Draw();
            }
        }
    }
}
