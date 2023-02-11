using System;
using System.Collections.Generic;
using LunarLander.audio;
using LunarLander.data;
using LunarLander.geometry2d;
using LunarLander.graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEvents;
using Point = LunarLander.geometry2d.Point;

namespace LunarLander.mode; 

public class MainMenu : IGameMode {

    private class MenuItem {
        public VectorText text { get; private set; }
        public string internalName { get; private set; }
        
        public MenuItem(string internalName) {
            this.text = null;
            this.internalName = internalName;
        }
        
        public void setText(VectorText text) {
            this.text = text;
        }
    }
    
    private static List<MenuItem> menuItems;
    private static int selectedItem;
    private static Point center;

    private static InputManager _inputManager;

    private static Image world;
    
    private readonly CompoundButton down = CompoundButton.fromGeneric(GenericButton.KeyDown) | GenericButton.KeyS | GenericButton.DevStickDown;
    private readonly CompoundButton up = CompoundButton.fromGeneric(GenericButton.KeyUp) | GenericButton.KeyW | GenericButton.DevStickUp;
    private readonly CompoundButton enter = CompoundButton.fromGeneric(GenericButton.KeyEnter) | GenericButton.KeySpace |
                                   GenericButton.DevA1;

    private static int heldFrames;

    private MainMenu() { }

    public static MainMenu instance { get; } = new();

    public void LoadContent(ContentManager content) {
        // No content to load
    }

    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height) {
        _inputManager = new InputManager();
        world = new Image(graphicsDeviceService.GraphicsDevice, width, height);
        selectedItem = -1;

        menuItems = new List<MenuItem> {
            new ("Instructions"),
            new ("LanderGame"),
            // new (null),
            // new (null),
            // new (null),
            // new (null),
            // new (null),
            new ("Oscilloscope"),
            new ("Options"),
            new ("Exit")
        };
        
        List<string> menuItemsString = new ( new [] {
            // All caps because I haven't implemented lower case letters yeti
            "INSTRUCTIONS",
            "LUNAR LANDER",
            // "RACING [NYI]",
            // "ASTEROIDS [NYI]",
            // "GRAVITAR [NYI]",
            // "LEVEL SELECT [NYI]",
            // "LEADERBOARD [NYI]",
            "OSCILLOSCOPE",
            "OPTIONS",
            "EXIT"
        });
        
        // Calculate the center of the screen
        center = new Point(width / 2.0, height / 2.0);

        const int fontSize = 24;
        
        // Create the menu items
        for (int i = 0; i < menuItemsString.Count; i++) {
            Point tlc = new (
                center.x - menuItemsString[i].Length * fontSize / 2.0,
                center.y + ((i + 1) * fontSize * 3) - menuItemsString.Count * fontSize * 3 / 2.0
            );
            menuItems[i].setText(new VectorText(menuItemsString[i], tlc, fontSize));
        }
        
        _inputManager.onHeld(up, () => {
            if (heldFrames != 0 && (heldFrames <= 30 || heldFrames % 5 != 0)) {
                heldFrames++;
                return;
            }
            RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
            selectedItem--;
            if (selectedItem < 0) selectedItem = menuItems.Count - 1;
            heldFrames++;
        });
        _inputManager.onHeld(down, () => {
            if (heldFrames != 0 && (heldFrames <= 30 || heldFrames % 5 != 0)) {
                heldFrames++;
                return;
            }
            RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
            selectedItem++;
            if (selectedItem >= menuItems.Count) selectedItem = 0;
            heldFrames++;
        });
        _inputManager.onPressed(enter, () => {
            RP2A03_API.pulsePlayNote(0, 9, 2, 4, 4);
            if (selectedItem == -1) return;
            switch (menuItems[selectedItem].internalName) {
                case null:
                    return; // null is a placeholder for a menu item that doesn't do anything / isn't implemented yet
                case "Exit":
                    LunarLander.running = false;
                    // wait for threads to exit
                    while (RP2A03.running) {
                        // spin
                    }
                    while (AudioBuffer.running) {
                        // spin
                    }
                    LunarLander.instance.Exit();
                    break;
                default:
                    LunarLander.instance.ChangeGameMode(menuItems[selectedItem].internalName);
                    break;
            }
        });
        _inputManager.onReleased(up | down, () => {
            heldFrames = 0;
        });
    }

    public void ReInitialize() {
        selectedItem = -1; // prevents the menu from automatically selecting the first item
    }

    public void Update(GameTime gameTime) {
        _inputManager.update(Keyboard.GetState(), GamePad.GetState(1));
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
        // Draw the menu items
        for (int i = 0; i < menuItems.Count; i++) {
            foreach(Line l in menuItems[i].text.Lines) {
                Drawing.drawLine(world, l, i == selectedItem ? Color.White : Color.Gray);
            }
        }

        spriteBatch.Draw(world.toTexture2D(), Vector2.Zero, Color.White);
    }

    public void Background(GameTime gameTime) {
    }
}