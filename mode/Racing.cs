using LunarLander.graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEvents;

namespace LunarLander.mode; 

public class Racing : IGameMode {

    private Racing() { }
    public static readonly Racing instance = new ();

    private static Image world;
    private static InputManager inputManager;

    public void LoadContent(ContentManager content) {
        //
    }

    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height) {
        world = new Image(graphicsDeviceService.GraphicsDevice, width, height);
        inputManager = new InputManager();
    }

    public void ReInitialize() {
        //
    }

    public void Update(GameTime gameTime) {
#if RELEASE
        inputManager.update(GamePad.GetState(PlayerIndex.One));
#else
        inputManager.update(Keyboard.GetState());
#endif
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
        world.reset();
        
        spriteBatch.Draw(world.toTexture2D(), new Vector2(0, 0), Color.White);
    }

    public void Background(GameTime gameTime) {
        //
    }
}