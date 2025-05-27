using gravity_simulation.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace gravity_simulation.Core
{
    public class Simulation : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        private Texture2D _pointTexture;
        private SpriteFont StatsFont;

        private Microsoft.Xna.Framework.Vector2 StatsFontPos;
        private Microsoft.Xna.Framework.Vector2 FPS_Pos;

        private Space space;

        const int numBodies = 220;

        public Simulation()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.ApplyChanges();

            ///

            var viewport = GraphicsDevice.Viewport;
            var random = new Random();

            space = new Space(numBodies, new Models.Vector2(viewport.Width, viewport.Height));

            for (int i = 0; i < numBodies; i++)
            {
                var x = random.Next(100, viewport.Width-100);
                var y = random.Next(100, viewport.Height-100);
                space.AddBody(new Body(10, 2, new Models.Vector2(x, y)));
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _pointTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pointTexture.SetData(new[] { Color.White });

            StatsFont = Content.Load<SpriteFont>("StatsFont");
            StatsFontPos = new Microsoft.Xna.Framework.Vector2(10, 10);
            FPS_Pos = new Microsoft.Xna.Framework.Vector2(10, 30);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            var dt = gameTime.ElapsedGameTime.TotalSeconds;
            space.Update(dt);

            base.Update(gameTime);
        }

        private void DrawNode(Quadtree node, Color outlineColor, double thickness = 0.1)
        {
            if (node is null) return;

            Rectangle OuterRect = new Rectangle(
                (int)(node.Boundary.Center.X - node.Boundary.HalfSize.X),
                (int)(node.Boundary.Center.Y - node.Boundary.HalfSize.Y),
                (int)node.Boundary.HalfSize.X * 2,
                (int)node.Boundary.HalfSize.Y * 2
            );

            Rectangle InnerRect = new Rectangle(
                (int)(node.Boundary.Center.X - node.Boundary.HalfSize.X + thickness*20),
                (int)(node.Boundary.Center.Y - node.Boundary.HalfSize.Y + thickness*20),
                (int)(node.Boundary.HalfSize.X * (2 - thickness)),
                (int)(node.Boundary.HalfSize.Y * (2 - thickness))
            );

            _spriteBatch.Draw(_pointTexture, OuterRect, outlineColor);
            _spriteBatch.Draw(_pointTexture, InnerRect, Color.Black);

            DrawNode(node.Topleft, outlineColor, thickness);
            DrawNode(node.Topright, outlineColor, thickness);
            DrawNode(node.Bottomleft, outlineColor, thickness);
            DrawNode(node.Bottomright, outlineColor, thickness);
        }

        public void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 point1, Vector2 point2, float thickness)
        {
            float distance = Vector2.Distance(point1, point2);
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
        
            Vector2 origin = new Vector2(0, 0.5f);
            Vector2 scale = new Vector2(distance, thickness);
            spriteBatch.Draw(texture, point1, null, Color.White, angle, origin, scale, SpriteEffects.None, 0f);
        }

        internal void DrawParticleLine(Quadtree node)
        {
            if (node.isLeaf())
            {
                foreach (Body body in node.Bodies)
                {
                    Vector2 bodyPosition = new Vector2((float) body.Position.X, (float) body.Position.Y);
                    Vector2 nodePosition = new Vector2(
                        (float) node.Boundary.Center.X,
                        (float) node.Boundary.Center.Y
                    );

                    DrawLine(_spriteBatch, _pointTexture, bodyPosition, nodePosition, 2f);
                }
            }
            else
            {
                DrawParticleLine(node.Topleft);
                DrawParticleLine(node.Topright);
                DrawParticleLine(node.Bottomleft);
                DrawParticleLine(node.Bottomright);
            }
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();


            // Visualize the quadtree

            //Quadtree quadtree = space.Quadtree;
            //DrawNode(quadtree, Color.Green, 0.1);

            // Draw the bodies

            foreach (var body in space.Bodies)
            {
                double bodyRadius = body.Radius;
                Models.Vector2 bodyPosition = body.Position;

                Rectangle rect = new Rectangle(
                    (int)(bodyPosition.X - bodyRadius),
                    (int)(bodyPosition.Y - bodyRadius),
                    (int)(bodyRadius * 2),
                    (int)(bodyRadius * 2)
                    );

                _spriteBatch.Draw(
                    _pointTexture,
                    rect,
                    Color.White
                    );
            }

            // Draw delta-time

            var dt = gameTime.ElapsedGameTime.TotalSeconds;
            _spriteBatch.DrawString(StatsFont, $"Delta Time: {dt}", StatsFontPos, Color.White);
            _spriteBatch.DrawString(StatsFont, $"FPS: {Math.Floor(1/dt)}", FPS_Pos, Color.White);

            // Visualize points within quadrants

            // DrawParticleLine(quadtree);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
