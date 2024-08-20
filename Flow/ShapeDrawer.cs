using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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

        public void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            float length = Vector2.Distance(start, end);
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            Draw(_lineTexture, new Rectangle((int)start.X, (int)start.Y, (int)length, 1),
                null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
