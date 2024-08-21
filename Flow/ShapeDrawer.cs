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

        private static int ToPixel(int coordinate)
        {
            return Flow.CellDim * (coordinate + 1);
        }

        public void DrawThinLine(int x1, int y1, int x2, int y2, Color color)
        {
            Vector2 start = new Vector2(ToPixel(x1), ToPixel(y1));
            Vector2 end = new Vector2(ToPixel(x2), ToPixel(y2));

            float length = Vector2.Distance(start, end);
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            Draw(_lineTexture, new Rectangle((int)start.X, (int)start.Y, (int)length, 1),
                null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }


        public void DrawThickLine(int x1, int y1, int x2, int y2, Color color)
        {
            Vector2 start = new Vector2(ToPixel(x1), ToPixel(y1));
            Vector2 end = new Vector2(ToPixel(x2), ToPixel(y2));

            Vector2 difference = end - start;
            Vector2 perpendicular = Vector2.Normalize(new Vector2(-1f * difference.Y, difference.X));

            float length = Vector2.Distance(start, end);
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            start -= 4f * perpendicular;

            Draw(_lineTexture, new Rectangle((int)start.X, (int)start.Y, (int)length, 8),
                null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }



        public void DrawOuterRectangle(int x, int y, Color color)
        {
            Draw(_lineTexture, new Rectangle(ToPixel(x), ToPixel(y), Flow.CellDim, Flow.CellDim), color);
        }

        public void DrawInnerRectangle(int x, int y, Color color)
        {
            Draw(_lineTexture, new Rectangle(ToPixel(x) + Flow.CellDim / 4, ToPixel(y) + Flow.CellDim / 4, Flow.CellDim / 2, Flow.CellDim / 2), color);
        }
    }
}
