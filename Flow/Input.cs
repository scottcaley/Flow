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
            Solve,
            Endpoint,
            Bridge,
            Gone,
            Wall,
            Portal
        }

        private static bool wasClicking = false;
        private static bool isClicking = false;
        public static bool JustClicked
        {
            get
            {
                return isClicking && !wasClicking;
            }
        }

        private static bool wasPressingSpace = false;
        private static bool isPressingSpace = false;
        private static int spacePressFrames = 0;
        public static bool JustPressedSpace
        {
            get
            {
                return isPressingSpace && !wasPressingSpace;
            }
        }
        public static double SpacePressDuration
        {
            get
            {
                return (double)spacePressFrames * Flow.FrameTime;
            }
        }

        private static bool wasPressingZ = false;
        private static bool isPressingZ = false;
        private static int zPressFrames = 0;
        public static bool JustPressedZ
        {
            get
            {
                return isPressingZ && !wasPressingZ;
            }
        }
        public static double ZPressDuration
        {
            get
            {
                return (double)zPressFrames * Flow.FrameTime;
            }
        }



        public static void Update()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            wasClicking = isClicking;
            isClicking = Mouse.GetState().LeftButton == ButtonState.Pressed;

            wasPressingSpace = isPressingSpace;
            isPressingSpace = keyboardState.IsKeyDown(Keys.Space);
            if (isPressingSpace) spacePressFrames++;
            else spacePressFrames = 0;

            wasPressingZ = isPressingZ;
            isPressingZ = keyboardState.IsKeyDown(Keys.Z);
            if (isPressingZ) zPressFrames++;
            else zPressFrames = 0;
        }

        public static KeyboardInputType GetKeyboardInputType()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                return KeyboardInputType.Solve;
            }
            else if (keyboardState.IsKeyDown(Keys.E))
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

        public static (int, int, int, int) EdgeCoordinates()
        {
            MouseState mouseState = Mouse.GetState();
            float x = (float)mouseState.X / (float)Flow.CellDim;
            float y = (float)mouseState.Y / (float)Flow.CellDim;

            int diagonalX = (int)MathF.Floor(x - y);
            int diagonalY = (int)MathF.Floor(x + y);

            if ((diagonalX + diagonalY) % 2 == 0) //vertical nodes
            {
                diagonalX += 1; //non-homogenous transformation
                diagonalY -= 3;
                int topX = (diagonalX + diagonalY) / 2;
                int topY = (-diagonalX + diagonalY) / 2;
                return (topX, topY, topX, topY + 1);
            }
            else //horizontal nodes
            {
                diagonalY -= 3; //non-homogeneous transformation
                int leftX = (diagonalX + diagonalY) / 2;
                int leftY = (-diagonalX + diagonalY) / 2;
                return (leftX, leftY, leftX + 1, leftY);
            }
        }

        public static bool IsClickingOnEdge()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton != ButtonState.Pressed) return false;

            (int, int, int, int) coordinates = EdgeCoordinates();
            int x1 = coordinates.Item1;
            int y1 = coordinates.Item2;
            int x2 = coordinates.Item3;
            int y2 = coordinates.Item4;

            return
                (-1 <= x1 && x1 < Flow.GraphDim && -1 <= y1 && y1 < Flow.GraphDim) &&
                ((x1 + 1 == x2 && y1 >= 0) || (y1 + 1 == y2 && x1 >= 0));
        }
    }
}
