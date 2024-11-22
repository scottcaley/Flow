using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    public class ShapeDrawer : SpriteBatch
    {
        private GraphicsDevice _device;
        private Texture2D _lineTexture;
        private SpriteFont _font;
        public GraphicsDevice Device
        {
            get { return _device; }
        }
        public ShapeDrawer(GraphicsDevice graphicsDevice, SpriteFont font) : base(graphicsDevice)
        {
            _device = graphicsDevice;

            _lineTexture = new Texture2D(_device, 1, 1);
            _lineTexture.SetData(new[] { Color.White });
            _font = font;
        }

        private static int ToCenterPixel(int coordinate)
        {
            return Flow.CellDim * (2 * coordinate + 3) / 2;
        }

        private void DrawThinLine(float x1, float y1, float x2, float y2, Color color)
        {
            Vector2 start = new Vector2(x1, y1);
            Vector2 end = new Vector2(x2, y2);

            float length = Vector2.Distance(start, end);
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            Draw(_lineTexture, new Rectangle((int)start.X, (int)start.Y, (int)length, 1),
                null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }


        private void DrawThickLine(float x1, float y1, float x2, float y2, Color color)
        {
            Vector2 start = new Vector2(x1, y1);
            Vector2 end = new Vector2(x2, y2);

            Vector2 difference = end - start;
            Vector2 perpendicular = Vector2.Normalize(new Vector2(-1f * difference.Y, difference.X));

            float length = Vector2.Distance(start, end);
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            start -= Flow.CellDim / 16 * perpendicular;

            Draw(_lineTexture, new Rectangle((int)start.X, (int)start.Y, (int)length, Flow.CellDim / 8),
                null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        public void DrawBorder(int x1, int y1, int x2, int y2, Color color, bool thick)
        {
            float pixelX1;
            float pixelY1;
            float pixelX2;
            float pixelY2;

            if (x1 + 1 == x2) //horizontal nodes
            {
                pixelX1 = ToCenterPixel(x1) + 0.5f * Flow.CellDim;
                pixelY1 = ToCenterPixel(y1) - 0.5f * Flow.CellDim;
                pixelX2 = pixelX1;
                pixelY2 = pixelY1 + Flow.CellDim;
            }
            else //vertical nodes
            {
                pixelX1 = ToCenterPixel(x1) - 0.5f * Flow.CellDim;
                pixelY1 = ToCenterPixel(y1) + 0.5f * Flow.CellDim;
                pixelX2 = pixelX1 + Flow.CellDim;
                pixelY2 = pixelY1;
            }


            if (thick)
            {
                DrawThickLine(pixelX1, pixelY1, pixelX2, pixelY2, color);
            }
            else
            {
                DrawThinLine(pixelX1, pixelY1, pixelX2, pixelY2, color);
            }
        }

        public void DrawPortalDirection(int x1, int y1, int x2, int y2, Color color)
        {
            if (x1 != x2) //horizontal difference
            {
                int cornerX = (ToCenterPixel(x1) + ToCenterPixel(x2)) / 2;
                if (x1 < x2) cornerX -= Flow.CellDim / 4;
                int cornerY = ToCenterPixel(y1) - Flow.CellDim / 8;
                Draw(_lineTexture, new Rectangle(cornerX, cornerY, Flow.CellDim / 4, Flow.CellDim / 4), color);
            }
            else //vertical difference
            {
                int cornerX = ToCenterPixel(x1) - Flow.CellDim / 8;
                int cornerY = (ToCenterPixel(y1) + ToCenterPixel(y2)) / 2;
                if (y1 < y2) cornerY -= Flow.CellDim / 4;
                Draw(_lineTexture, new Rectangle(cornerX, cornerY, Flow.CellDim / 4, Flow.CellDim / 4), color);
            }
        }

        public void DrawOuterRectangle(int x, int y, Color color)
        {
            Draw(_lineTexture, new Rectangle(ToCenterPixel(x) - Flow.CellDim / 2, ToCenterPixel(y) - Flow.CellDim / 2, Flow.CellDim, Flow.CellDim), color);
        }

        public void DrawInnerRectangle(int x, int y, Color color)
        {
            Draw(_lineTexture, new Rectangle(ToCenterPixel(x) - Flow.CellDim / 4, ToCenterPixel(y) - Flow.CellDim / 4, Flow.CellDim / 2, Flow.CellDim / 2), color);
        }

        public void DrawCross(int x, int y, Color color)
        {
            Draw(_lineTexture, new Rectangle(ToCenterPixel(x) - Flow.CellDim / 4, ToCenterPixel(y) - Flow.CellDim / 2, 1, Flow.CellDim),
                null, color, 0f, Vector2.Zero, SpriteEffects.None, 0);
            Draw(_lineTexture, new Rectangle(ToCenterPixel(x) + Flow.CellDim / 4, ToCenterPixel(y) - Flow.CellDim / 2, 1, Flow.CellDim),
                null, color, 0f, Vector2.Zero, SpriteEffects.None, 0);
            Draw(_lineTexture, new Rectangle(ToCenterPixel(x) - Flow.CellDim / 2, ToCenterPixel(y) - Flow.CellDim / 4, Flow.CellDim, 1),
                null, color, 0f, Vector2.Zero, SpriteEffects.None, 0);
            Draw(_lineTexture, new Rectangle(ToCenterPixel(x) - Flow.CellDim / 2, ToCenterPixel(y) + Flow.CellDim / 4, Flow.CellDim, 1),
                null, color, 0f, Vector2.Zero, SpriteEffects.None, 0);
        }


        public void DrawPath(int x1, int y1, int x2, int y2, Color color)
        {
            int xPixel;
            int width;
            int yPixel;
            int height;

            if (x1 != x2) //horizontal path
            {
                xPixel = ToCenterPixel(Math.Min(x1, x2));
                width = Flow.CellDim * Math.Abs(x1 - x2);
                yPixel = ToCenterPixel(y1) - Flow.CellDim / 16;
                height = Flow.CellDim / 8;
            }
            else //vertical path
            {
                xPixel = ToCenterPixel(x1) - Flow.CellDim / 16;
                width = Flow.CellDim / 8;
                yPixel = ToCenterPixel(Math.Min(y1, y2));
                height = Flow.CellDim * Math.Abs(y1 - y2);
            }

            Draw(_lineTexture, new Rectangle(xPixel, yPixel, width, height),
                    null, color, 0f, Vector2.Zero, SpriteEffects.None, 0);
        }



        public void DrawHalfPath(int x, int y, int xDest, int yDest, Color color)
        {
            int xPixel;
            int width;
            int yPixel;
            int height;

            if (x != xDest) //horizontal path
            {
                xPixel = x < xDest ? ToCenterPixel(x) : ToCenterPixel(xDest) + Flow.CellDim / 2;
                width = Flow.CellDim / 2;
                yPixel = ToCenterPixel(y) - Flow.CellDim / 16;
                height = Flow.CellDim / 8;
            }
            else //vertical path
            {
                xPixel = ToCenterPixel(x) - Flow.CellDim / 16;
                width = Flow.CellDim / 8;
                yPixel = y < yDest ? ToCenterPixel(y) : ToCenterPixel(yDest) + Flow.CellDim / 2;
                height = Flow.CellDim / 2;
            }

            Draw(_lineTexture, new Rectangle(xPixel, yPixel, width, height),
                    null, color, 0f, Vector2.Zero, SpriteEffects.None, 0);
        }




        public void DisplayLine(string line)
        {
            DrawString(_font, line, new Vector2(Flow.CellDim, Flow.CellDim * Flow.GraphDimY + Flow.CellDim * 3 / 2), Color.White);
        }

    }
}
