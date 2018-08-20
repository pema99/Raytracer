using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raytracer
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D Blank;

        Raytracer Raytracer { get; set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 600;
            graphics.PreferredBackBufferHeight = 400;
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Blank = new Texture2D(GraphicsDevice, 1, 1);
            Blank.SetData<Color>(new Color[] { Color.White });
            var Rendertarget = new Texture2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            Raytracer = new Raytracer(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, 75, Rendertarget);
        }

        protected override void Update(GameTime gameTime)
        {
            Raytracer.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);
            spriteBatch.Begin();

            Raytracer.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
