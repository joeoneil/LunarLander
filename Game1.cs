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
    
    private const int WINDOW_WIDTH = 600;
    private const int WINDOW_HEIGHT = (int)((21 / 9.0) * WINDOW_WIDTH);
    
    private readonly GraphicsDeviceManager _graphics; 
    private SpriteBatch _spriteBatch;
    private readonly KeyboardManager _keyboardManager = new();
    
    private readonly Dictionary<string, IGameMode> _gameModes = new();
    private string _currentGameMode = "MainMenu";
    public LunarLander() {
        instance = this;
        
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        _gameModes.Add("MainMenu", MainMenu.instance);
        _gameModes.Add("LanderGame", LanderGame.instance);
        _gameModes.Add("Racing", Racing.instance);
        _gameModes.Add("AsteroidsGame", AsteroidsGame.instance);
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
        RP2A03_API.pulsePlayNote(0, 440.0, -1, 15);

        foreach(IGameMode gm in _gameModes.Values) {
            gm.Initialize(_graphics, WINDOW_WIDTH, WINDOW_HEIGHT);
        }
        
        // this is called even though the game mode has been initialized already in case some initialization
        // is done in the ReInitialize method that is not done in the initialize method
        _gameModes[_currentGameMode].ReInitialize();
        
        _keyboardManager.OnPressed(Keys.Escape, () => {
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

    protected override void Update(GameTime gameTime)
    {
        _keyboardManager.Update(Keyboard.GetState());
        

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
