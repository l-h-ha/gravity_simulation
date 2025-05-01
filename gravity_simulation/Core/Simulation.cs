using gravity_simulation.Models;
using gravity_simulation.Constants;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace gravity_simulation.Core
{
    public class Simulation : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private BasicEffect BasicEffect;
        private VertexPositionColor[] points;
        
        private Space space;

        const int numBodies = 100;
        const int targetWidth = 5;
        const int targetHeight = 5;

        private double targetWidthRatio;
        private double targetHeightRatio;

        public Simulation()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            BasicEffect = new BasicEffect(GraphicsDevice);
            BasicEffect.VertexColorEnabled = true;
            BasicEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);

            var viewport = GraphicsDevice.Viewport;
            float screenWidth = viewport.Width;
            float screenHeight = viewport.Height;

            BasicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, screenWidth, screenHeight, 0, 0.1f, 1000);
            BasicEffect.World = Matrix.Identity;

            targetWidthRatio = targetWidth / screenWidth;
            targetHeightRatio = targetHeight / screenHeight;

            ///

            var random = new Random();
            space = new Space(numBodies);
            points = new VertexPositionColor[numBodies];

            for (int i = 0; i < numBodies; i++)
            {
                var x = random.Next(0, viewport.Width);
                var y = random.Next(0, viewport.Height);
                points[i] = new VertexPositionColor(new Vector3(x, y, 0), Color.White);
                space.AddBody(new Body(1, 1, new Models.Vector2(x, y)));
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // TODO: Add your update logic here

            var dt = gameTime.ElapsedGameTime.TotalSeconds;
            space.Update(dt, targetWidthRatio, targetHeightRatio);

            for (int i = 0; i < numBodies; i++)
            {
                points[i] = new VertexPositionColor(
                    new Vector3((float)space.bodies[i].Position.X, (float)space.bodies[i].Position.Y, 0), 
                    Color.White
                    );
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.PointList, points, 0, points.Length);
            }

            base.Draw(gameTime);
        }
    }
}
