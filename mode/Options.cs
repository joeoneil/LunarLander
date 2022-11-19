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

    private static readonly CompoundButton up = CompoundButton.or(GenericButton.DevStickUp, GenericButton.KeyUp) |
                                       GenericButton.KeyW;
    private static readonly CompoundButton down = CompoundButton.or(GenericButton.DevStickDown, GenericButton.KeyDown) |
                                         GenericButton.KeyS;
    private static readonly CompoundButton left = CompoundButton.or(GenericButton.DevStickLeft, GenericButton.KeyLeft) |
                                         GenericButton.KeyA;
    private static readonly CompoundButton right = CompoundButton.or(GenericButton.DevStickRight, GenericButton.KeyRight) |
                                          GenericButton.KeyD;

    private static readonly List<IOption> options = new();

    private Point center;

    private interface IOption {
        public Text getText();
        public Text getValueText();
        public void left();
        public void right();
    }
    private class Option<T> : IOption {
        private readonly Text text;
        private readonly Text valueText;
        public T value;
        
        private Update onLeft;
        private Update onRight;
        private Display display;

        public delegate void Update(Option<T> self);
        public delegate string Display(T value);
        
        public Option(string text, T value, Point position, int scale) {
            this.text = new Text(text, position, scale);
            this.value = value;
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
    }
    private Options() {
        instance = this;
    }
    
    public void LoadContent(ContentManager content) {
    }

    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height) {
        background = new Image(graphicsDeviceService.GraphicsDevice, width, height);
        center = new Point(width / 2.0, height / 2.0);
        
        Option<int> gravityOption = new ("GRAVITY", 1, center + new Point(-200, 0), 16);
        gravityOption.setDisplay(value => value / 10 + "." + value % 10);
        gravityOption.SetOnLeft(self => self.value = Math.Max(0, self.value - 1));
        gravityOption.SetOnRight(self => self.value = Math.Min(10, self.value + 1));
        options.Add(gravityOption);
        
        Option<int> fuelOption = new ("STARTING FUEL", 20, center + new Point(-200, 40), 16);
        fuelOption.setDisplay(value => value + " L");
        fuelOption.SetOnLeft(self => self.value = Math.Max(0, self.value - 5));
        fuelOption.SetOnRight(self => self.value = Math.Min(100, self.value + 5));
        options.Add(fuelOption);
        
        Option<int> volumeOption = new ("VOLUME", 100, center + new Point(-200, 80), 16);
        volumeOption.setDisplay(value => value + "%");
        volumeOption.SetOnLeft(self => {
            self.value = Math.Max(0, self.value - 5);
            RP2A03.gain = self.value / 100.0;
        });
        volumeOption.SetOnRight(self => {
            self.value = Math.Min(200, self.value + 5);
            RP2A03.gain = self.value / 100.0;
        });
        options.Add(volumeOption);
        
        _inputManager.onPressed(up, () => {
            selected = Math.Max(0, selected - 1);
            RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
        });
        _inputManager.onPressed(down, () => {
            selected = Math.Min(options.Count - 1, selected + 1);
            RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
        });
        _inputManager.onPressed(left, () => {
            if (selected == -1) return;
            options[selected].left();
            RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
        });
        _inputManager.onPressed(right, () => {
            if (selected == -1) return;
            options[selected].right();
            RP2A03_API.pulsePlayNote(0, 9, 3, 2, 2);
        });
    }

    public void ReInitialize() {
        selected = -1;
    }

    public void Update(GameTime gameTime) {
        _inputManager.update(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One));
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
        foreach (Text text in options.Select(option => option.getValueText())) {
            foreach (Line l in text.Lines) {
                Drawing.drawLine(background, l, i == selected ? Color.White : Color.Gray);
            }
            i++;
        }

        spriteBatch.Draw(background.toTexture2D(), Vector2.Zero, Color.White);
    }

    public void Background(GameTime gameTime) {
    }
}