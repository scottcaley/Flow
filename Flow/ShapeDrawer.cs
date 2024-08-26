using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow
{
    public class ShapeDrawer : SpriteBatch
    {
        private GraphicsDevice _device;
        private Texture2D _lineTexture;
        public GraphicsDevice Device
        {
            get { return _device; }
        }
        public ShapeDrawer(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            _device = graphicsDevice;

            _lineTexture = new Texture2D(_device, 1, 1);
            _lineTexture.SetData(new[] { Color.White });
        }

        private static int ToCenterPixel(int coordinate)
        {
            return (Flow.CellDim * (2 * coordinate + 3)) / 2;
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

        public void DrawEdge(int x1, int y1, int x2, int y2, Color color, bool thick)
        {
            float pixelX1;
            float pixelY1;
            float pixelX2;
            float pixelY2;

            if (x1 + 1 == x2) //horizontal nodes
            {
                pixelX1 = ToCenterPixel(x1) + 0.5f * (float)Flow.CellDim;
                pixelY1 = ToCenterPixel(y1) - 0.5f * (float)Flow.CellDim;
                pixelX2 = pixelX1;
                pixelY2 = pixelY1 + Flow.CellDim;
            }
            else //vertical nodes
            {
                pixelX1 = ToCenterPixel(x1) - 0.5f * (float)Flow.CellDim;
                pixelY1 = ToCenterPixel(y1) + 0.5f * (float)Flow.CellDim;
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
    }
}
