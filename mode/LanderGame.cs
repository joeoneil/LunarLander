using System;
using System.Collections.Generic;
using System.Globalization;
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

public class LanderGame : IGameMode {
    
    private static readonly InputManager _inputManager = new ();

    private List<Shape> _shapes = new ();
    private Point center;

    private Polygon lander;
    private List<Line> landerFragments;
    private List<Point> fragmentVelocities;
    private List<double> fragmentRotations;
    private double deadTimer;
    private double invulnTimer;
    private int deathCount;

    private Point landerVelocity = new (0, 0);
    private Point landerPosition = new (0, 0);
    private Point gravity = new (0, -324);
    private double theta;
    private const double thrust_power = 275;
    private double thrust_graphic;
    private double fuel;
    private double start_fuel;
    private VectorText fuelText;
    private VectorText fuelCountText;
    private string fuelCountString;
    public double startingFuelBonus { get; set; } = 20;
    public double gravityScale { get; set; } = 0.1;

    private double updateTime;
    private bool rotateLock;
    private bool thrustLock;
    private bool isDead;

    private const double breakThreshold = 15;
    private int godModeStep;
    private bool godMode;
    
    private Image world;
    private Level currentLevel;

    private List<Level> levels;
    private int levelIndex;
    private double levelTimer;
    private double levelCompleteTimer;
    private bool levelComplete;
    private VectorText levelCompleteText;

    private static Texture2D meatball;
    public static int meatballsCollected { get; private set; }

    private static readonly Dictionary<string, Song> songs = new();

    private LanderGame() { }

    public static LanderGame instance { get; } = new ();
    
    private static readonly CompoundButton _thrust = CompoundButton.fromGeneric(GenericButton.DevA2) | GenericButton.KeyW;

    private static readonly CompoundButton _rotateLeft =
        CompoundButton.fromGeneric(GenericButton.DevA1) | GenericButton.KeyA;

    private static readonly CompoundButton _rotateRight =
        CompoundButton.fromGeneric(GenericButton.DevA3) | GenericButton.KeyD;
    private static readonly CompoundButton _a = CompoundButton.fromGeneric(GenericButton.DevB1) | GenericButton.KeyV;
    private static readonly CompoundButton _b = CompoundButton.fromGeneric(GenericButton.DevB2) | GenericButton.KeyB;
    private static readonly CompoundButton _up = CompoundButton.fromGeneric(GenericButton.DevStickUp) | GenericButton.KeyUp;
    private static readonly CompoundButton _down = CompoundButton.fromGeneric(GenericButton.DevStickDown) | GenericButton.KeyDown;
    private static readonly CompoundButton _left = CompoundButton.fromGeneric(GenericButton.DevStickLeft) | GenericButton.KeyLeft;

    private static readonly CompoundButton _right = CompoundButton.fromGeneric(GenericButton.DevStickRight) |
                                                    GenericButton.KeyRight;
    private static readonly CompoundButton _start = CompoundButton.fromGeneric(GenericButton.DevB4) | GenericButton.KeyEnter;

    private static readonly CompoundButton
        _reset = CompoundButton.fromGeneric(GenericButton.DevB3) | GenericButton.KeyR;
    

    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height) {
        levels = new List<Level>(new[] {
            Level.level1,
            Level.level2,
            Level.level3,
        });
        
        songs.Add("win", Song.song1);

        center = new Point(width / 2.0, height / 2.0);

        world = new Image(graphicsDeviceService.GraphicsDevice, width, height);
        
        fuelCountText = new VectorText($"{Math.Ceiling(fuel)}", new Point(266, 5), 16);

        _inputManager.onHeld(_rotateLeft, () => {
            rotateLander(-8);
        });
        _inputManager.onHeld(_rotateRight, () => {
            rotateLander(8);
        });
        _inputManager.onHeld(_thrust, () => {
            thrustLander(false);
        });
        _inputManager.onHeld(!_thrust, () => {
            thrustLander(true);
        });
        _inputManager.onPressed(_thrust, () => {
            if (!isDead) {
                RP2A03_API.noisePlayNote(13, 1, 3, true);
                RP2A03_API.noisePlayNote(13, -1, 3, true);
            }
        });
        _inputManager.onReleased(_thrust, () => {
            if (!isDead) {
                RP2A03_API.noisePlayNote(13, 1, 0, true);
            }
        });
        _inputManager.onPressed(_reset, () => {
            fuel = start_fuel - currentLevel.fuel;
            loadLevel(levels[levelIndex]);
        });
        _inputManager.onPressed(_up, () => {
            if (godModeStep is 0 or 1) {
                godModeStep++;
            }
            else {
                godModeStep = 0;
            }
        });
        _inputManager.onPressed(_down, () => {
            if (godModeStep is 2 or 3) {
                godModeStep++;
            }
            else {
                godModeStep = 0;
            }
        });
        _inputManager.onPressed(_left, () => {
            if (godModeStep is 4 or 6) {
                godModeStep++;
            }
            else {
                godModeStep = 0;
            }
        });
        _inputManager.onPressed(_right, () => {
            if (godModeStep is 5 or 7) {
                godModeStep++;
            }
            else {
                godModeStep = 0;
            }
        });
        _inputManager.onPressed(_b, () => {
            if (godModeStep is 8) {
                godModeStep++;
            }
            else {
                godModeStep = 0;
            }
        });
        _inputManager.onPressed(_a, () => {
            if (godModeStep is 9) {
                godModeStep++;
            }
            else {
                godModeStep = 0;
            }
        });
        _inputManager.onPressed(_start, () => {
            if (godModeStep is 10) {
                godMode = true;
                fuel += 9999;
            }
            else {
                godModeStep = 0;
            }

            if (!levelComplete) {
                return;
            }
            songs["win"].stop();
            loadLevel(levels[levelIndex]); // level index is incremented in update
        });

        // testText = new Text(" !\"#$%&'()*+,-./\n0123456789:;<=>?\n@ABCDEFGHIJKLMNO\nPQRSTUVWXYZ[\\]^_\n`abcdefghijklmno", new Point(10, 200), 16);
        // testText = new Text("HELLO, WORLD!", new Point(10, 200), 16);

        fuelText = new VectorText("FUEL REMAINING: ", new Point(10, 5), 16);
    }

    public void ReInitialize() {
        // do nothing
        godModeStep = 0;
        godMode = false;
        levelIndex = 0;
        fuel = startingFuelBonus;
        loadLevel(levels[levelIndex]);
        gravity = new Point(0, 324 * -gravityScale);
    }

    public void Update(GameTime gameTime) {
        
        _inputManager.update(GamePad.GetState(1), Keyboard.GetState());

        updateTime = gameTime.ElapsedGameTime.Milliseconds / 1000.0;
        rotateLock = true;
        thrustLock = true;
        
        foreach(Song s in songs.Values) {
            s.update();
        }

        if (levelComplete) {
            return;
        }

        levelTimer += updateTime;
        invulnTimer -= updateTime;

        if (landerPosition.magnitude() > 20000) {
            // If the lander is too far away from the center, reset it.
            loadLevel(currentLevel);
        }

        if (!isDead) {
            landerVelocity += gravity * updateTime;
            landerPosition += landerVelocity * updateTime;
            currentLevel.translate(landerVelocity * updateTime);
            
            // check for collisions
            foreach (Shape s in _shapes) {
                if (!s.intersects(lander)) {
                    continue;
                }
                // pop the lander out of the shape
                Point normal = s.contactNormal(lander);
                // if the lander's velocity relative to the shape is greater than 50, it's dead
                Point proj = normal.project(landerVelocity);
                if (landerVelocity.project(normal).magnitude() > breakThreshold && invulnTimer < 0 && !godMode) {
                    RP2A03_API.noisePlayNote(14, 200);
                    isDead = true;
                    deathCount++;
                    deadTimer = 0;
                    landerFragments = new List<Line>(new[] {
                        new Line(lander.points[0].clone(), lander.points[1].clone()),
                        new Line(lander.points[1].clone(), lander.points[2].clone()),
                        new Line(lander.points[2].clone(), lander.points[0].clone())
                    });
                    fragmentVelocities = new List<Point>();
                    fragmentRotations = new List<double>();
                    foreach (Line _ in landerFragments) {
                        fragmentVelocities.Add(new Point(LunarLander.rng.NextDouble() * 40 - 20, LunarLander.rng.NextDouble() * 40 - 20) - landerVelocity);
                        fragmentRotations.Add(LunarLander.rng.NextDouble() * 4 - 2);
                    }
                }
                // zero the lander's velocity in the direction of the normal
                double mag = landerVelocity.magnitude();
                landerVelocity -= proj;
                landerVelocity.cap(mag); // cap the velocity to the original magnitude
                landerVelocity *= 0.94;
                if (landerVelocity.magnitude() < 6) {
                    landerVelocity = new Point(0, 0);
                }

                currentLevel.translate(normal); 
            }
        }
        else {
            deadTimer += updateTime;
            for (int i = 0; i < landerFragments.Count; i++) {
                fragmentVelocities[i] -= gravity * updateTime;
                landerFragments[i].translate(fragmentVelocities[i] * updateTime);
                landerFragments[i].rotate(fragmentRotations[i] * updateTime);
            }
            if (deadTimer > 2.75) {
                isDead = false;
                deadTimer = 0;
                invulnTimer = 2.5;
                landerFragments = null;
                fragmentVelocities = null;
                fragmentRotations = null;
                landerVelocity = new Point(0, 0);
                theta = 0;
                lander = new Polygon(new List<Point>(new [] { new Point(0, 0), new Point(20, 50), new Point(-20, 50) }));
                lander.translate(center - lander.getCentroid());
                currentLevel.translate(-landerPosition);
                landerPosition = new Point(0, 0);
            }
        }
        
        // check for level completion
        if (currentLevel.goal.intersects(lander) && landerVelocity.magnitude() < 3 && !isDead) {
            levelCompleteTimer += updateTime;
            if (levelCompleteTimer > 0.5) {
                songs["win"].play();
                levelCompleteText = new VectorText(
                    $"COMPLETED LEVEL {levelIndex + 1}!\n" +
                    $"TIME: {Math.Floor(levelTimer)}.{Math.Round(levelTimer * 100 % 100)}\n" +
                    $"FUEL USED: {Math.Floor(start_fuel - fuel)}.{Math.Round((start_fuel - fuel) * 100 % 100)}\n" +
                    $"DEATHS: {deathCount}\n" +
                    $"{(currentLevel.meatball == null ? "COLLECTED MEATBALL" : "")}\n" +
                    "PRESS B3 TO CONTINUE",
                    new Point(10, 200), 16);
                levelIndex++;
                if (levelIndex >= levels.Count) {
                    levelIndex = 0;
                }

                levelComplete = true;
                thrustLock = false;
                rotateLock = false;
            }
        }
        else {
            levelCompleteTimer = 0;
        }
        
        // check for meatball collisions
        if (currentLevel.meatball == null || !currentLevel.meatball.intersects(lander)) {
            return;
        }
        currentLevel.meatball = null;
        meatballsCollected++;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
        

        Point lander_back = lander.getLines()[1].getCentroid();
        Polygon thrust = new (new List<Point>(new [] { lander_back + new Point(-10, 0), lander_back + new Point(10, 0), lander_back + new Point(0, thrust_graphic * 25) }));
        Polygon thrust_orange = new (new List<Point>(new [] { lander_back + new Point(-10, 0), lander_back + new Point(10, 0), lander_back + new Point(0, thrust_graphic * 10) }));
        thrust.rotate(theta, lander_back);
        thrust_orange.rotate(theta, lander_back);
        
        // draw the lander's velocity vector
        // Drawing.drawLine(world, new Line(lander.getCentroid(), lander.getCentroid() - landerVelocity / 3), Color.Red);

        if (!isDead) {
            foreach(Shape s in _shapes) {
                Drawing.drawShape(world, s, Color.White);
            }

            if (thrust_graphic > 0) {
                Drawing.drawPolygon(world, thrust, Color.Red);
                Drawing.drawPolygon(world, thrust_orange, Color.Orange);
            }
            Color c = invulnTimer %0.4 < 0.2 ? Color.Aqua : Color.Aquamarine;
            Drawing.drawPolygon(world, lander, invulnTimer > 0 ? c : Color.Aqua);
        }
        else {
            Drawing.drawCircle(world, new Circle(lander.getCentroid(), deadTimer * 100), new Color((int) (100000 / Math.Pow(deadTimer * 100, 2)), 0, 0), 1, true);
            foreach(Shape s in _shapes) {
                Drawing.drawShape(world, s, Color.White);
            }
            foreach (Line l in landerFragments) {
                Drawing.drawLine(world, l, Color.Aqua);
            }
        }
        
        // draw the current level's flagpole as a white line
        Drawing.drawLine(world, currentLevel.flagpole, Color.White);
        
        // draw the current level's flag as a red triangle
        Drawing.drawPolygon(world, currentLevel.flag, Color.Red);

        if (levelComplete) {
            foreach (Line l in levelCompleteText.Lines) {
                Drawing.drawLine(world, l, Color.White);
            }
        }
        
        string newFuelText = Math.Ceiling(fuel).ToString(CultureInfo.InvariantCulture);
        if (newFuelText != fuelCountString) {
            fuelCountText.setText(newFuelText);
            fuelCountString = newFuelText;
        }
        
        foreach(Line l in fuelCountText.Lines) {
            Drawing.drawLine(world, l, Color.Orange);
        }
        
        foreach(Line l in fuelText.Lines) {
            Drawing.drawLine(world, l, Color.White);
        }

        spriteBatch.Draw(world.toTexture2D(), Vector2.Zero, Color.White);

        if (currentLevel.meatball != null) {
            spriteBatch.Draw(meatball, currentLevel.meatball.getCentroid().toVector2() - Vector2.One * 25, null, Color.White, 0, new Vector2(0, 0), new Vector2(0.125f, 0.125f), SpriteEffects.None, 0);
        }
        
        world.reset();
    }

    public void LoadContent(ContentManager content) {
        meatball = content.Load<Texture2D>("meatball");
    }

    public void Background(GameTime gameTime) {
        // do nothing.
    }
    
    private void rotateLander(double angle) {
        if (!rotateLock) {
            return;
        }
        lander.rotate(angle * updateTime);
        theta += angle * updateTime;
        rotateLock = false;
    }

    private void thrustLander(bool reverse) {
        if (!thrustLock) {
            return;
        }
        if (reverse || fuel <= 0) {
            thrust_graphic -= updateTime * 10;
            thrust_graphic = Math.Max(0, thrust_graphic);
        }
        else {
            thrust_graphic += updateTime * 10;
            if (thrust_graphic > 1.1) {
                thrust_graphic -= LunarLander.rng.NextDouble() * (thrust_graphic / 2);
            }
            landerVelocity += new Point(thrust_power * -Math.Sin(theta), thrust_power * Math.Cos(theta)) * updateTime;
            fuel -= updateTime;
        }
        thrustLock = false;
    }

    private void loadLevel(Level level) {
        levelTimer = 0;
        invulnTimer = 2.5;
        levelComplete = false;
        deathCount = 0;
        level.reset();
        currentLevel = level;
        _shapes = level.shapes.ToList();
        theta = 0;
        lander = new Polygon(new List<Point>(new [] { new Point(0, 0), new Point(20, 50), new Point(-20, 50) }));
        lander.translate(center - lander.getCentroid());
        landerPosition = level.start - center;
        landerVelocity = new Point(0, 0);
        level.translate(-landerPosition);
        landerPosition = new Point(0, 0);
        fuel += level.fuel;
        start_fuel = fuel;
    }
}
