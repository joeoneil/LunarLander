using System;
using System.Collections.Generic;
using System.Threading;
using LunarLander.audio;
using LunarLander.mode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEvents;

namespace LunarLander;

public class LunarLander : Game {
    public static LunarLander instance { get; private set; }

    public static readonly Random rng = new(DateTime.Now.Millisecond);

    private readonly int WINDOW_WIDTH;
    private readonly int WINDOW_HEIGHT;
    
    private readonly GraphicsDeviceManager _graphics; 
    private SpriteBatch _spriteBatch;
    private readonly InputManager _inputManager = new();
    
    private readonly Dictionary<string, IGameMode> _gameModes = new();
    private string _currentGameMode = "MainMenu";

    private static readonly CompoundButton menuButton = CompoundButton.fromGeneric(GenericButton.DevB4) | GenericButton.KeyEscape;

    public static bool running { get; set; } = true;
    public LunarLander() {
        instance = this;
        
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        this.Window.IsBorderless = true;
        
        int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        int maxWidthFromHeight = (int) (screenHeight * 9f / 21f);
        int maxHeightFromWidth = (int) (screenWidth * 21f / 9f);
        WINDOW_WIDTH = Math.Min(maxWidthFromHeight, screenWidth);
        WINDOW_HEIGHT = Math.Min(maxHeightFromWidth, screenHeight);
        
        _gameModes.Add("MainMenu", MainMenu.instance);
        _gameModes.Add("Instructions", Instructions.instance);
        _gameModes.Add("LanderGame", LanderGame.instance);
        _gameModes.Add("Racing", Racing.instance);
        _gameModes.Add("AsteroidsGame", AsteroidsGame.instance);
        _gameModes.Add("Oscilloscope", Oscilliscope.instance);
        _gameModes.Add("Options", Options.instance);
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
        _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
        _graphics.ApplyChanges();

        RP2A03.start();

        RP2A03_API.setNoiseMode(true);
        Thread.Sleep(10); // init lfsr
        
        RP2A03_API.setPulse1(true);
        RP2A03_API.setPulse2(true);
        RP2A03_API.setTriangle(true);
        RP2A03_API.setNoise(true);
        RP2A03_API.pulsePlayNote(0, 440.0, 1);
        RP2A03_API.pulsePlayNote(1, 440.0, 1);

        foreach(IGameMode gm in _gameModes.Values) {
            gm.Initialize(_graphics, (uint)WINDOW_WIDTH, (uint)WINDOW_HEIGHT);
        }
        
        // this is called even though the game mode has been initialized already in case some initialization
        // is done in the ReInitialize method that is not done in the initialize method
        _gameModes[_currentGameMode].ReInitialize();
        
        _inputManager.onPressed(menuButton, () => {
            ChangeGameMode("MainMenu");
        });

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        // Don't need to load any content as we're not using any textures.
        // Everything is vector based. (not texture cringe)
        foreach(IGameMode gm in _gameModes.Values) {
            gm.LoadContent(Content);
        }
    }

    protected override void Update(GameTime gameTime) {
        _inputManager.update(Keyboard.GetState(), GamePad.GetState(1));

        _gameModes[_currentGameMode].Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin();

        _gameModes[_currentGameMode].Draw(_spriteBatch, gameTime);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    public void ChangeGameMode(string newGameMode) {
        _currentGameMode = newGameMode;
        _gameModes[_currentGameMode].ReInitialize();
    }
}
