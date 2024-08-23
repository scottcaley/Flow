using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    public static class Input
    {
        public enum KeyboardInputType
        {
            None,
            Endpoint,
            Bridge,
            Gone,
            Wall,
            Portal
        }

        public static KeyboardInputType GetKeyboardInputType()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.E))
            {
                return KeyboardInputType.Endpoint;
            }
            else if (keyboardState.IsKeyDown(Keys.B))
            {
                return KeyboardInputType.Bridge;
            }
            else if (keyboardState.IsKeyDown(Keys.G))
            {
                return KeyboardInputType.Gone;
            }
            else if (keyboardState.IsKeyDown(Keys.W))
            {
                return KeyboardInputType.Wall;
            }
            else if (keyboardState.IsKeyDown(Keys.P))
            {
                return KeyboardInputType.Portal;
            }
            return KeyboardInputType.None;
        }

        public static (int, int) NodeCoordinates()
        {
            MouseState mouseState = Mouse.GetState();
            int x = (mouseState.X - Flow.CellDim) / Flow.CellDim;
            int y = (mouseState.Y - Flow.CellDim) / Flow.CellDim;
            return (x, y);
        }

        public static bool IsClickingOnNode()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton != ButtonState.Pressed) return false;

            (int, int) coordinates = NodeCoordinates();
            int x = coordinates.Item1;
            int y = coordinates.Item2;
            return (0 <= x && x < Flow.GraphDim && 0 <= y && y < Flow.GraphDim);
        }

        public static ((int, int), (int, int)) EdgeCoordinates()
        {
            MouseState mouseState = Mouse.GetState();
            float x = (float)mouseState.X / (float)Flow.CellDim;
            float y = (float)mouseState.Y / (float)Flow.CellDim;

            int diagonalX = (int)MathF.Floor(x - y);
            int diagonalY = (int)MathF.Floor(x + y);

            if ((diagonalX + diagonalY) % 2 == 0) //horizontal edge
            {
                diagonalY -= 2; //non-homogenous transformation
                int leftX = (diagonalX + diagonalY) / 2;
                int leftY = (-diagonalX + diagonalY) / 2;
                return ((leftX, leftY), (leftX + 1, leftY));
            }
            else //vertical edge
            {
                diagonalX += 1; //non-homogeneous transformation
                diagonalY -= 2;
                int topX = (diagonalX + diagonalY) / 2;
                int topY = (-diagonalX + diagonalY) / 2;
                return ((topX, topY), (topX, topY + 1));
            }
        }

        public static bool IsClickingOnEdge()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton != ButtonState.Pressed) return false;

            ((int, int), (int, int)) coordinates = EdgeCoordinates();
            int x1 = coordinates.Item1.Item1;
            int y1 = coordinates.Item1.Item2;
            int x2 = coordinates.Item2.Item1;
            int y2 = coordinates.Item2.Item2;
            Debug.WriteLine($"({x1}, {y1})   ({x2}, {y2})");
            return (0 <= x1 && x1 < Flow.GraphDim && 0 <= y1 && y1 < Flow.GraphDim
                && 0 <= x2 && x2 < Flow.GraphDim && 0 <= y2 && y2 < Flow.GraphDim);
        }
    }
}
