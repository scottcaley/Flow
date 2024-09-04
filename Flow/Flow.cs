using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Flow
{
    public class Flow : Game
    { 
        public const int GraphDimX = 8;
        public const int GraphDimY = 8;
        public const int CellDim = 128;
        public const double FrameTime = 1.0 / 60.0;

        public static readonly Color MaybeColor = new Color(0x80, 0x80, 0x80);
        public static readonly Color GoodColor = Color.White;
        public static readonly Color[] Colors = new Color[]
        {
            new Color(0xFE, 0x00, 0x00),
            new Color(0x00, 0x8A, 0x00),
            new Color(0x0C, 0x2A, 0xFE),
            new Color(0xEA, 0xE1, 0x00),
            new Color(0xFD, 0x89, 0x00),
            new Color(0x01, 0xFF, 0xFF),
            new Color(0xFF, 0x08, 0xC9),
            new Color(0x9F, 0x89, 0x50),
            new Color(0x7E, 0x00, 0x7E),
            new Color(0xFE, 0xFF, 0xD9),
            new Color(0x5E, 0x50, 0x32),
            new Color(0x00, 0xFF, 0x01),
            new Color(0xA4, 0x2A, 0x29),
            new Color(0x39, 0x29, 0xB0),
            new Color(0x00, 0x7F, 0x80),
            new Color(0xFF, 0x7C, 0xEC)
        };

        public static GraphicsDeviceManager Graphics;
        public static ShapeDrawer Sd;

        private Graph _graph;
        private Puzzle _solution;
        private bool _isSolving;

        public Flow()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            TargetElapsedTime = TimeSpan.FromSeconds(FrameTime);

            Graphics.PreferredBackBufferWidth = CellDim * (GraphDimX + 2);
            Graphics.PreferredBackBufferHeight = CellDim * (GraphDimY + 3);
            Graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            Texture2D lineTexture = new Texture2D(GraphicsDevice, 1, 1);
            lineTexture.SetData(new[] { Color.White });
            _graph = new Graph();
            _isSolving = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Sd = new ShapeDrawer(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            Input.Update();

            if (!_isSolving)
            {
                _graph.Update();
                if (Input.JustPressedSpace)
                {
                    _graph.Finish();
                    _solution = new Solution(_graph);
                    _isSolving = true;
                }
            }
            else
            {
                if (Input.JustPressedSpace || Input.SpacePressDuration > 1.0)
                {
                    if (_solution.IsSolved && Input.JustPressedSpace)
                    {
                        _solution = new Solution(_graph);
                    }
                    else
                    {
                        _solution.Forward();
                    }
                }
                else if (Input.JustPressedZ || Input.ZPressDuration > 1.0)
                {
                    _solution.Back();
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Sd.Begin();
            GraphicsDevice.Clear(new Color(0x40, 0x40, 0x40));
            _graph.Draw();
            if (_isSolving) _solution.Draw();

            base.Draw(gameTime);
            Sd.End();
        }
    }
}
