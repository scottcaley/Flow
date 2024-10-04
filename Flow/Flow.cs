using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Flow
{
    public class Flow : Game
    { 
        public const int GraphDimX = 12;
        public const int GraphDimY = 15;
        public const int CellDim = 80;
        public const double FrameTime = 1.0 / 120.0;

        public static readonly Color MaybeColor = new Color(0x50, 0x50, 0x50);
        public static readonly Color GoodColor = Color.White;
        public static readonly Color UncertainGoodColor = new Color(0xB0, 0xB0, 0xB0);
        public static readonly Color BadColor = Color.Black;
        public static readonly Color UncertainBadColor = new Color(0x20, 0x20, 0x20);
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
        public static readonly Color[] UncertainColors = new Color[]
        {
            new Color(0x7F, 0x00, 0x00),
            new Color(0x00, 0x45, 0x00),
            new Color(0x06, 0x15, 0x7F),
            new Color(0x75, 0x70, 0x00),
            new Color(0x7E, 0x44, 0x00),
            new Color(0x00, 0x7F, 0x7F),
            new Color(0x7F, 0x04, 0x64),
            new Color(0x4F, 0x44, 0x28),
            new Color(0x3F, 0x00, 0x3F),
            new Color(0x7F, 0x7F, 0x6C),
            new Color(0x2F, 0x28, 0x19),
            new Color(0x00, 0x7F, 0x00),
            new Color(0x52, 0x15, 0x14),
            new Color(0x1C, 0x14, 0x58),
            new Color(0x00, 0x3F, 0x40),
            new Color(0x7F, 0x3E, 0x76)
        };

        public static GraphicsDeviceManager Graphics;
        public static ShapeDrawer Sd;

        private Grid _graph;
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
            _graph = new Grid();
            _isSolving = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Sd = new ShapeDrawer(GraphicsDevice, Content.Load<SpriteFont>("Fonts/File"));

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
                if (Input.GetKeyboardInputType() == Input.KeyboardInputType.Finish)
                {
                    while (!_solution.IsFinished()) _solution.Forward();
                }
                else if (Input.JustPressedSpace || Input.SpacePressDuration > 1.0)
                {
                    if (_solution.IsFinished() && Input.JustPressedSpace)
                    {
                        _solution = new Solution(_graph);
                    }
                    else
                    {
                        int numForwards = 1 << Input.GetDigit();
                        for (int i = 0; i < numForwards; i++) _solution.Forward();
                    }
                }
                else if (Input.JustPressedZ || Input.ZPressDuration > 1.0)
                {
                    int numForwards = 1 << Input.GetDigit();
                    for (int i = 0; i < numForwards; i++) _solution.Back();
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
