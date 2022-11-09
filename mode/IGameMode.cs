using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LunarLander.mode; 

public interface IGameMode {

   /**
    * All content for this game mode should be loaded here.
    */
    public void LoadContent(ContentManager content);

    /**
     * All non-content related initialization should be done here. Called immediately on startup.
     * This method should include the creation of all game objects.
     */
    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height);
    
    /**
     * Called when the game mode is resumed after being paused.
     * This method should reset everything that needs to be reset (if any) when the game mode is resumed.
     */
    public void ReInitialize();

    /**
     * All update logic for this game mode should be done here. This will only be called when this game mode is the active game mode.
     * This will run before draw.
     */
    public void Update(GameTime gameTime);

    /**
     * All drawing logic for this game mode should be done here. This will only be called when this game mode is the active game mode.
     * This will run after update.
     */
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime);

    /**
     * All necessary background tasks should be done here. This will run even when this game mode is not the active game mode.
     * This will run after the update method of the active game mode, but before the draw method of the active game mode.
     */
    public void Background(GameTime gameTime);
}