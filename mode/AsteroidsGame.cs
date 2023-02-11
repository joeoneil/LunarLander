using LunarLander.audio;
using LunarLander.graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEvents;

namespace LunarLander.mode; 

public class AsteroidsGame : IGameMode {

    private AsteroidsGame() { }
    public static AsteroidsGame instance { get; } = new();

    private Image world;
    private InputManager inputManager;

    private double songTimer = 0;
    private int noteIndex = 0;
    
#if RELEASE
    // TODO: Update Devcade bindings once controller design is finalized
    private static readonly CompoundButton _thrust = CompoundButton.fromGeneric(GenericButton.DevA1);
    private static readonly CompoundButton _rotateLeft = CompoundButton.fromGeneric(GenericButton.DevA2);
    private static readonly CompoundButton _rotateRight = CompoundButton.fromGeneric(GenericButton.DevA3);
#else
    public static CompoundButton thrust { get; } = CompoundButton.fromGeneric(GenericButton.KeyW);
    public static CompoundButton rotateLeft { get; } = CompoundButton.fromGeneric(GenericButton.KeyA);
    public static CompoundButton rotateRight { get; } = CompoundButton.fromGeneric(GenericButton.KeyD);
#endif
    
    public void LoadContent(ContentManager content)
    {
        // Method intentionally left empty.
    }

    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height) {
        world = new Image(graphicsDeviceService.GraphicsDevice, width, height);
        inputManager = new InputManager();
    }

    public void ReInitialize()
    {
        // Method intentionally left empty.
    }

    public void Update(GameTime gameTime) {
        inputManager.update(Keyboard.GetState());
        
        songTimer += gameTime.TotalGameTime.Milliseconds / 1000.0;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
        world.reset();

        spriteBatch.Draw(world.toTexture2D(), Vector2.Zero, Color.White);
    }

    public void Background(GameTime gameTime) { }
}