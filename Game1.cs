using System.Collections.Generic;
using LunarLander.geometry2d;
using LunarLander.graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEvents;
using Point = LunarLander.geometry2d.Point;

namespace LunarLander;

public class LunarLander : Game {
    private const int WINDOW_WIDTH = 600;
    private const int WINDOW_HEIGHT = (int)((21 / 9.0) * WINDOW_WIDTH);
    private readonly Point center = new(WINDOW_WIDTH / 2.0, WINDOW_HEIGHT / 2.0);
    
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private readonly KeyboardManager _keyManager = new KeyboardManager();

    private readonly List<Shape> _shapes = new ();

    private Polygon lander;

    private Image world;

    public LunarLander()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
        _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
        _graphics.ApplyChanges();

        world = new Image(_graphics.GraphicsDevice, WINDOW_WIDTH, WINDOW_HEIGHT);

        lander = new Polygon(new List<Point>(new [] { new Point(0, 0), new Point(20, 50), new Point(-20, 50) }));
        lander.translate(center - lander.getCentroid());
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        // Don't need to load any content as we're not using any textures.
        // Everything is vector based. (not texture cringe)
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin();
        
        world.reset();
        
        foreach (Shape s in _shapes) {
            Drawing.drawShape(world, s, Color.White, 1);
        }

        Drawing.drawPolygon(world, lander, Color.Aqua, 1);

        _spriteBatch.Draw(world.toTexture2D(), Vector2.Zero, Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
