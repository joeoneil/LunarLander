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
using Rectangle = LunarLander.geometry2d.Rectangle;

namespace LunarLander.mode; 

public class Oscilliscope : IGameMode {

    private static readonly byte[] active = new byte[] {
        1, 1, 1, 1, 1, 1, 1, 1, // $4000
        1, 1, 1, 1, 1, 1, 1, 1, // $4001
        1, 1, 1, 1, 1, 1, 1, 1, // $4002
        1, 1, 1, 1, 1, 1, 1, 1, // $4003
        1, 1, 1, 1, 1, 1, 1, 1, // $4004
        1, 1, 1, 1, 1, 1, 1, 1, // $4005
        1, 1, 1, 1, 1, 1, 1, 1, // $4006
        1, 1, 1, 1, 1, 1, 1, 1, // $4007
        1, 1, 1, 1, 1, 1, 1, 1, // $4008
        0, 0, 0, 0, 0, 0, 0, 0, // $4009
        1, 1, 1, 1, 1, 1, 1, 1, // $400A
        1, 1, 1, 1, 1, 1, 1, 1, // $400B
        0, 0, 1, 1, 1, 1, 1, 1, // $400C
        0, 0, 0, 0, 0, 0, 0, 0, // $400D
        1, 0, 0, 0, 1, 1, 1, 1, // $400E
        1, 1, 1, 1, 1, 0, 0, 0, // $400F
        1, 1, 0, 0, 1, 1, 1, 1, // $4010
        0, 1, 1, 1, 1, 1, 1, 1, // $4011
        1, 1, 1, 1, 1, 1, 1, 1, // $4012
        1, 1, 1, 1, 1, 1, 1, 1, // $4013
        0, 0, 0, 0, 0, 0, 0, 0, // $4014
        1, 1, 0, 1, 1, 1, 1, 1, // $4015
        0, 0, 0, 0, 0, 0, 0, 0, // $4016
        1, 1, 0, 0, 0, 0, 0, 0, // $4017
        0, 0, 0, 0, 0, 0, 0, 0, // $4018
        0, 0, 0, 0, 0, 0, 0, 0, // $4019
        0, 0, 0, 0, 0, 0, 0, 0, // $401A
        0, 0, 0, 0, 0, 0, 0, 0, // $401B
        0, 0, 0, 0, 0, 0, 0, 0, // $401C
        0, 0, 0, 0, 0, 0, 0, 0, // $401D
        0, 0, 0, 0, 0, 0, 0, 0, // $401E
        0, 0, 0, 0, 0, 0, 0, 0, // $401F
    };

    private static int noiseIndex = 0;

    private static readonly Rectangle window = new Rectangle(new Point(64, 800), 600 - 128, 400);
    private static readonly Line xAxis = new Line(new Point(64, 1000), new Point(600 - 64, 1000));
    

    private Oscilliscope() { }
    public static readonly Oscilliscope instance = new();

    private Image screen;
    private Text[] registers = new Text[32 * 8];
    
    private InputManager inputManager;

    public void LoadContent(ContentManager content) {
    }

    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height) {
        screen = new Image(graphicsDeviceService.GraphicsDevice, width, height);
        inputManager = new InputManager();
        const int scale = 18;
        for (int i = 0; i < 32 * 8; i++) {
            int col = i % 8;
            int row = i / 8;
            int extra = row > 15 ? 5 : 0;
            row %= 16;
            int row_extra = row / 4;
            registers[i] = new Text("0", new Point(col * scale + extra * (scale * 2) + (scale * 8), row * (scale * 2) + row_extra * (scale * 2) +
                (scale * 4)), scale);
        }
        inputManager.onPressed(GenericButton.KeyQ, () => {
            RP2A03_API.pulsePlayNote(0, (byte)LunarLander.rng.NextInt64(11), 3, 100);
        });
        inputManager.onPressed(GenericButton.KeyW, () => {
            RP2A03_API.pulsePlayNote(1, (byte)LunarLander.rng.NextInt64(11), 2, 100);
        });
        inputManager.onPressed(GenericButton.KeyE, () => {
            RP2A03_API.trianglePlayNote((byte)LunarLander.rng.NextInt64(11), 4, 300);
        });
        inputManager.onPressed(GenericButton.KeyR, () => {
            RP2A03_API.noisePlayNote((byte)LunarLander.rng.NextInt64(15), 1, 4);
        });
        inputManager.onPressed(GenericButton.KeyT, () => {
            byte note = (byte)LunarLander.rng.NextInt64(11);
            RP2A03_API.pulsePlayNote(0, note, 3, 100);
            RP2A03_API.pulsePlayNote(1, note, 2, 100);
        });
        inputManager.onHeld(GenericButton.KeyUp, () => {
            RP2A03.testFreq *= 1.05f;
        });
        inputManager.onHeld(GenericButton.KeyDown, () => {
            RP2A03.testFreq *= 0.95f;
        });
    }

    public void ReInitialize() {
    }

    public void Update(GameTime gameTime) {
        inputManager.update(Keyboard.GetState());
        byte[] registers = new byte[32];
        for (int i = 0; i < 32; i++) {
            registers[i] = RP2A03.read(i);
        }
        for (int i = 0; i < 32; i++) {
            for (int j = 0; j < 8; j++) {
                this.registers[i * 8 + j].setText(((registers[i] >> (7 - j)) & 1).ToString());
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {

        screen.reset();
        
        for(int i = 0; i < 32 * 8; i++) {
            foreach(Line l in registers[i].Lines) {
                Drawing.drawLine(screen, l, active[i] == 1 ? Color.White : Color.DarkGray);
            }
        }

        byte[] audio = AudioBuffer.getPrevBuffer();

        Drawing.drawPolygon(screen, window, Color.White);
        Drawing.drawLine(screen, xAxis, Color.White);

        for (int i = 0; i < (audio.Length / 2) - 1; i++) {
            short left = (short)((audio[i * 2 + 1] << 8) | audio[i * 2]);
            short right = (short)((audio[i * 2 + 3] << 8) | audio[i * 2 + 2]);
            
            // convert 2s complement unsigned short to double
            double left_d = (double)left / (double)short.MaxValue;
            double right_d = (double)right / (double)short.MaxValue;
            
            double x1 = window.left + ((2 * i) / (double)audio.Length) * window.width;
            double x2 = window.left + ((2 * i + 1) / (double)audio.Length) * window.width;
            double y1 = window.top + (window.height / 2) - (left_d) * (window.height / 2);
            double y2 = window.top + (window.height / 2) - (right_d) * (window.height / 2);
            Drawing.drawLine(screen, new Line(x1, y1, x2, y2), Color.Aqua);
        }
        
        spriteBatch.Draw(screen.toTexture2D(), Vector2.Zero, Color.White);
    }

    public void Background(GameTime gameTime) {
    }
}