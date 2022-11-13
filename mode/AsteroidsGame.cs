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

    private static Image world;
    private static InputManager inputManager;

    private static double songTimer = 0;
    private static int noteIndex = 0;
    
    private static Song song = Song.mario;
    
#if RELEASE
    // TODO: Update Devcade bindings once controller design is finalized
    private static readonly CompoundButton _thrust = CompoundButton.fromGeneric(GenericButton.DevA1);
    private static readonly CompoundButton _rotateLeft = CompoundButton.fromGeneric(GenericButton.DevA2);
    private static readonly CompoundButton _rotateRight = CompoundButton.fromGeneric(GenericButton.DevA3);
#else
    private static readonly CompoundButton _thrust = CompoundButton.fromGeneric(GenericButton.KeyW);
    private static readonly CompoundButton _rotateLeft = CompoundButton.fromGeneric(GenericButton.KeyA);
    private static readonly CompoundButton _rotateRight = CompoundButton.fromGeneric(GenericButton.KeyD);
#endif
    
    public void LoadContent(ContentManager content) { }

    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height) {
        world = new Image(graphicsDeviceService.GraphicsDevice, width, height);
        inputManager = new InputManager();
    }

    public void ReInitialize() { }

    public void Update(GameTime gameTime) {
        inputManager.update(Keyboard.GetState());
        
        songTimer += gameTime.TotalGameTime.Milliseconds / 1000.0;
        if (songTimer > song.noteStartTime(noteIndex)) {
            RP2A03_API.pulsePlayNote(0, song.notePitch(noteIndex), song.noteOctave(noteIndex),
                (int)(song.noteDuration(noteIndex) * 1000), 7);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
        world.reset();

        spriteBatch.Draw(world.toTexture2D(), Vector2.Zero, Color.White);
    }

    public void Background(GameTime gameTime) { }
}