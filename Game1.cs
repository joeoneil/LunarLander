using System;
using System.Collections.Generic;
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
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
        _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
        _graphics.ApplyChanges();
        
        foreach(IGameMode gm in _gameModes.Values) {
            gm.Initialize(_graphics, WINDOW_WIDTH, WINDOW_HEIGHT);
        }
        
        // this is called even though the game mode has been initialized already in case some initialization is done in the ReInitialize method
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
