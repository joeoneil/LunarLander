using System;
using System.Collections.Generic;
using System.Linq;
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

public class Options : IGameMode {

    public static Options instance = new();

    private static Image background;
    private static readonly InputManager _inputManager = new();
    private int selected = -1;

    // private static readonly CompoundButton up = CompoundButton.or(GenericButton.DevStickUp, GenericButton.KeyUp) |
    //                                    GenericButton.KeyW;
    // private static readonly CompoundButton down = CompoundButton.or(GenericButton.DevStickDown, GenericButton.KeyDown) |
    //                                      GenericButton.KeyS;
    // private static readonly CompoundButton left = CompoundButton.or(GenericButton.DevStickLeft, GenericButton.KeyLeft) |
    //                                      GenericButton.KeyA;
    // private static readonly CompoundButton right = CompoundButton.or(GenericButton.DevStickRight, GenericButton.KeyRight) |
    //                                       GenericButton.KeyD;
    private static readonly CompoundButton up = CompoundButton.fromGeneric(GenericButton.DevStickUp) | GenericButton.KeyW | GenericButton.KeyUp;
    private static readonly CompoundButton down = CompoundButton.fromGeneric(GenericButton.DevStickDown) | GenericButton.KeyS | GenericButton.KeyDown;
    private static readonly CompoundButton left = CompoundButton.fromGeneric(GenericButton.DevStickLeft) | GenericButton.KeyA | GenericButton.KeyLeft;
    private static readonly CompoundButton right = CompoundButton.fromGeneric(GenericButton.DevStickRight) | GenericButton.KeyD | GenericButton.KeyRight;

    private static readonly List<IOption> options = new();

    private Point center;

    private int heldFrames;

    private interface IOption {
        public Text getText();
        public Text getValueText();
        public void left();
        public void right();
        public bool isDefault();
    }
    private class Option<T> : IOption {
        private readonly Text text;
        private readonly Text valueText;
        public T value;
        private readonly T def;
            
        private Update onLeft;
        private Update onRight;
        private Display display;

        public delegate void Update(Option<T> self);
        public delegate string Display(T value);
        
        public Option(string text, T value, Point position, int scale) {
            this.text = new Text(text, position, scale);
            this.value = value;
            this.def = value;
            this.valueText = new Text(value.ToString(), position + new Point((text.Length + 2) * scale, 0), scale);
            this.display = T => T.ToString();
            this.onLeft = _ => { };
            this.onRight = _ => { };
        }
        
        public void SetOnLeft(Update onLeft) {
            this.onLeft = onLeft;
        }
        
        public void SetOnRight(Update onRight) {
            this.onRight = onRight;
        }
        
        public void setDisplay(Display display) {
            this.display = display;
            valueText.setText(display(value));
        }
        
        public Text getText() {
            return text;
        }
        
        public Text getValueText() {
            return valueText;
        }

        public void left() {
            onLeft?.Invoke(this);
            update();
        }
        
        public void right() {
            onRight?.Invoke(this);
            update();
        }
        
        private void update() {
            valueText.setText(display(value));
        }

        public bool isDefault() {
            return value.Equals(def);
        }
    }
    private Options() {
        instance = this;
    }
    
    public void LoadContent(ContentManager content) {
    }

    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height) {
        background = new Image(graphicsDeviceService.GraphicsDevice, width, height);
        center = new Point(width / 2.0, height / 2.0);
        
        Option<int> gravityOption = new ("GRAVITY", 2, center + new Point(-200, 0), 16);
        gravityOption.setDisplay(value => value / 2 + "." + (value * 5) % 10);
        gravityOption.SetOnLeft(self => {
            self.value = Math.Max(0, self.value - 1);
            LanderGame.gravityScale = self.value / 20.0;
        });
        gravityOption.SetOnRight(self => {
            self.value = Math.Min(6, self.value + 1);
            LanderGame.gravityScale = self.value / 20.0;
        });
        options.Add(gravityOption);
        
        Option<int> fuelOption = new ("STARTING FUEL", 20, center + new Point(-200, 40), 16);
        fuelOption.setDisplay(value => value + " L");
        fuelOption.SetOnLeft(self => {
            self.value = Math.Max(0, self.value - 5);
            LanderGame.startingFuelBonus = self.value;
        });
        fuelOption.SetOnRight(self => {
            self.value = Math.Min(100, self.value + 5);
            LanderGame.startingFuelBonus = self.value;
        });
        options.Add(fuelOption);
        
        Option<int> volumeOption = new ("VOLUME", 100, center + new Point(-200, 80), 16);
        volumeOption.setDisplay(value => value + "%");
        volumeOption.SetOnLeft(self => {
            self.value = Math.Max(0, self.value - 5);
            AudioBuffer.gain = self.value / 100.0;
        });
        volumeOption.SetOnRight(self => {
            self.value = Math.Min(200, self.value + 5);
            AudioBuffer.gain = self.value / 100.0;
        });
        options.Add(volumeOption);
        
        _inputManager.onHeld(up, () => {
            if (heldFrames == 0 || heldFrames > 30 && heldFrames % 5 == 0) {
                selected--;
                if (selected < 0) selected = options.Count - 1;
                RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
            }
            heldFrames++;
        });
        _inputManager.onHeld(down, () => {
            if (heldFrames == 0 || heldFrames > 30 && heldFrames % 5 == 0) {
                selected++;
                if (selected >= options.Count) selected = 0;
                RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
            }
            heldFrames++;
        });
        _inputManager.onHeld(left, () => {
            if (selected == -1) return;
            if (heldFrames == 0 || heldFrames > 30 && heldFrames % 5 == 0) {
                options[selected].left();
                RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
            }
            heldFrames++;
        });
        _inputManager.onHeld(right, () => {
            if (selected == -1) return;
            if (heldFrames == 0 || heldFrames > 30 && heldFrames % 5 == 0) {
                options[selected].right();
                RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
            }
            heldFrames++;
        });
        _inputManager.onReleased(up | down | left | right, () => { 
            heldFrames = 0;
        });
    }

    public void ReInitialize() {
        selected = -1;
    }

    public void Update(GameTime gameTime) {
        _inputManager.update(Keyboard.GetState(), GamePad.GetState(1));
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
        background.reset();

        int i = 0;
        foreach (Text text in options.Select(option => option.getText())) {
            foreach (Line l in text.Lines) {
                Drawing.drawLine(background, l, i == selected ? Color.White : Color.Gray);
            }
            i++;
        }

        i = 0;
        foreach (IOption option in options) {
            foreach (Line l in option.getValueText().Lines) {
                Drawing.drawLine(background, l, option.isDefault() ? i == selected ? Color.White : Color.Gray : i == selected ? Color.Aquamarine : Color.MediumAquamarine);
            }
            i++;
        }

        spriteBatch.Draw(background.toTexture2D(), Vector2.Zero, Color.White);
    }

    public void Background(GameTime gameTime) {
    }
}