using System;
using System.Collections.Generic;
using System.Linq;
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
        public Text text { get; private set; }
        public string internalName { get; private set; }
        
        public MenuItem(Text text, string internalName) {
            this.text = text;
            this.internalName = internalName;
        }
        
        public MenuItem(string internalName) {
            this.text = null;
            this.internalName = internalName;
        }
        
        public void setText(Text text) {
            this.text = text;
        }
    }
    
    private static List<MenuItem> menuItems;
    private static int selectedItem;
    private static Point center;

    private static InputManager _inputManager;

    private static Image world;
    
    // button definitions
    #if RELEASE
        private CompoundButton down = CompoundButton.fromGeneric(GenericButton.DevA3);
        private CompoundButton up = CompoundButton.fromGeneric(GenericButton.DevA1);
        private CompoundButton enter = CompoundButton.fromGeneric(GenericButton.DevMenu);
    #else
        private CompoundButton down = CompoundButton.fromGeneric(GenericButton.KeyDown) | GenericButton.KeyS;
        private CompoundButton up = CompoundButton.fromGeneric(GenericButton.KeyUp) | GenericButton.KeyW;
        private CompoundButton enter = CompoundButton.fromGeneric(GenericButton.KeyEnter) | GenericButton.KeySpace;
    #endif

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
            new ("LanderGame"),
            new (null),
            new (null),
            new (null),
            new (null),
            new (null),
            new ("Exit")
        };
        
        List<string> menuItemsString = new ( new [] {
            // All caps because I haven't implemented lower case letters yet
            "LUNAR LANDER",
            "RACING",
            "ASTEROIDS",
            "GRAVITAR",
            "LEVEL SELECT",
            "LEADERBOARD",
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
            menuItems[i].setText(new Text(menuItemsString[i], tlc, fontSize));
        }
        
        _inputManager.onPressed(up, () => {
            switch (selectedItem) {
                case > 0:
                    selectedItem--;
                    break;
                case 0:
                    selectedItem = menuItems.Count - 1;
                    break;
                default:
                    selectedItem = 0;
                    break;
            }
        });
        _inputManager.onPressed(down, () => {
            if (selectedItem == menuItems.Count - 1) {
                selectedItem = 0;
            }
            else {
                selectedItem++;
            }
        });
        _inputManager.onPressed(enter, () => {
            if (selectedItem == -1) return;
            switch (menuItems[selectedItem].internalName) {
                case null:
                    return; // null is a placeholder for a menu item that doesn't do anything / isn't implemented yet
                case "Exit":
                    LunarLander.instance.Exit();
                    break;
                default:
                    LunarLander.instance.ChangeGameMode(menuItems[selectedItem].internalName);
                    break;
            }
        });
    }

    public void ReInitialize() {
        selectedItem = -1; // prevents the menu from automatically selecting the first item
    }

    public void Update(GameTime gameTime) {
        _inputManager.update(Keyboard.GetState());
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