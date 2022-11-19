using System.Collections.Generic;
using System.Linq;
using LunarLander.data;
using LunarLander.geometry2d;
using LunarLander.graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Point = LunarLander.geometry2d.Point;

namespace LunarLander.mode; 

public class Instructions : IGameMode {
    public static Instructions instance = new();
    
    private static Image background;
    private static List<Text> textList;
    private static List<ColoredShape> shapeList;

    private struct ColoredShape {
        public Shape shape;
        public Color color;
        public ColoredShape (Shape shape, Color color) {
            this.shape = shape;
            this.color = color;
        }

        public ColoredShape(Shape shape) {
            this.shape = shape;
            this.color = Color.White;
        }
    }

    private static Point center;

    private Instructions() {
        instance = this;
    }
    
    public void LoadContent(ContentManager content) {
    }

    public void Initialize(IGraphicsDeviceService graphicsDeviceService, uint width, uint height) {
        background = new Image(graphicsDeviceService.GraphicsDevice, width, height);
        textList = new List<Text>();
        shapeList = new List<ColoredShape>();
        center = new Point(width / 2.0, height / 2.0);

        textList.Add(new Text("PRESS A1 TO ROTATE LEFT", center + new Point(-200, -40), 16));
        textList.Add(new Text("PRESS A2 TO POWER THRUSTERS", center + new Point(-200, 0) , 16));
        textList.Add(new Text("PRESS A3 TO ROTATE RIGHT", center + new Point(-200, 40), 16));

        shapeList.Add(new ColoredShape(new Circle(center + new Point(-250, -25), 15), Color.Red));
        shapeList.Add(new ColoredShape(new Circle(center + new Point(-250, 15), 15), Color.Blue));
        shapeList.Add(new ColoredShape(new Circle(center + new Point(-250, 55), 15), Color.Green));
    }
    
    public void ReInitialize() {
    }

    public void Update(GameTime gameTime) {
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
        foreach (Line l in textList.SelectMany(text => text.Lines)) {
            Drawing.drawLine(background, l);
        }
        
        foreach (ColoredShape cs in shapeList) {
            Drawing.drawCircle(background, cs.shape.getCentroid(), 18, Color.Gray, 1, true);
            Drawing.drawCircle(background, (Circle)cs.shape, cs.color, 1, true);
        }

        spriteBatch.Draw(background.toTexture2D(), Vector2.Zero, Color.White);
    }

    public void Background(GameTime gameTime) {
    }
}
