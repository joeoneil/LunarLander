using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using LunarLander.geometry2d;

namespace LunarLander.data; 

public class Level {
    private readonly List<Shape> baseShapes;
    public List<Shape> shapes { get; private set; }
    public Rectangle goal { get; private set; }
    public Line flagpole { get; private set; }
    public Polygon flag { get; private set; }
    public Point start { get; private set; }
    public int fuel { get; private set; }
    
    public Rectangle meatball { get; set; }

    private readonly Point amountMoved = new ();

    private Level(List<Shape> shapes, Rectangle goal, Point start, int fuel) {
        this.goal = goal;
        this.start = start;
        this.fuel = fuel;
        // create flagpole as a line 70 pixels tall from the bottom center of the goal
        flagpole = new Line(goal.left + goal.width / 2, goal.bottom, goal.left + goal.width / 2, goal.bottom - 45);
        // create flag as a triangle 20 pixels tall and 30 pixels wide with the top point at the top of the flagpole
        flag = new Polygon(new List<Point>(new [] {
            new Point(flagpole.p2.x, flagpole.p2.y),
            new Point(flagpole.p2.x, flagpole.p2.y + 12),
            new Point(flagpole.p2.x + 20, flagpole.p2.y + 6)
        }));
        this.baseShapes = shapes;
        this.shapes = getShapes();
    }

    public void translate(double x, double y) {
        shapes.ForEach(s => s.translate(x, y));
        goal.translate(x, y);
        flagpole.translate(x, y);
        flag.translate(x, y);
        amountMoved.translate(x, y);
        if (this.meatball != null) {
            meatball.translate(x, y);
        }
    }

    public void translate(Point p) {
        translate(p.x, p.y);
    }
    
    public void reset() {
        translate(-amountMoved);
        this.shapes = getShapes();
    }
    
    public List<Shape> getShapes() {
        List<Shape> processed = new ();
        foreach(Shape s in this.baseShapes) {
            if (s is not Line l) {
                processed.Add(s);
                continue;
            }
            processed.AddRange(l.greeble((int) (l.length / 100), 20));
        }
        flagpole = new Line(goal.left + goal.width / 2, goal.bottom, goal.left + goal.width / 2, goal.bottom - 45);
        flag = new Polygon(new List<Point>(new [] {
            new Point(flagpole.p2.x, flagpole.p2.y),
            new Point(flagpole.p2.x, flagpole.p2.y + 12),
            new Point(flagpole.p2.x + 20, flagpole.p2.y + 6)
        }));
        int i = 0;
        flagpole.translate(0, 25);
        flag.translate(0, 25);
        while (processed.Any(flagpole.intersects)) {
            flagpole.translate(0, -1);
            flag.translate(0, -1);
            if (i++ == 50) {
                break;
            }
        }
        return processed;
    }

    private Level addMeatball(Point p) {
        Point offset = new Point(25, 25);
        this.meatball = new Rectangle(p - offset, p + offset);
        return this;
    }

    public static readonly Level level1 = new Level(
        /*
         *      |------------0------------|
         *      |                         |
         *      |                         |
         *      |               |>        1
         *      7             _4|__       |
         *      |             |   3     * |
         *      |             |   |___2___|
         *      |             5              
         *      |          S  |              
         *      |______6______|
         */

        // shapes
        new List<Shape>(
            // a set of lines drawn in the shape of the above comment with the top corner at 0,0
            new[] {
                new Line(0, 0, 1500, 0), // 0
                new Line(1500, 0, 1500, 1000), // 1
                new Line(1500, 1000, 1000, 1000), // 2
                new Line(1000, 1000, 1000, 800), // 3
                new Line(new Point(1000, 800), new Point(700, 800), false), // 4
                new Line(700, 800, 700, 2000), // 5
                new Line(700, 2000, 0, 2000), // 6
                new Line(0, 2000, 0, 0) // 7
            }),
        // goal
        new Rectangle(1000, 800, 700, 750),
        // start
        new Point(500, 1950),
        // fuel
        20
    ).addMeatball(new Point(1250, 900));

    public static readonly Level level2 = new Level(
        /*
         *     -------------0-------------
         *     |                         |
         *     |            S            |
         *     |       ______4____       |
         *     |       |         |       |
         *     |       7         5       |
         *     |       |____6____|       |
         *     3                         1
         *     |            |>           |
         *     |       _____|8____       |
         *     |       |         |       |
         *     |   *   11        9       |
         *     |       |____10___|       |
         *     |                         |
         *     |                         |
         *     |____________2____________|
         */

        // shapes
        new List<Shape>(
            // a set of lines drawn in the shape of the above comment with the top corner at 0,0
            new[] {
                new Line(0, 0, 2400, 0), // 0
                new Line(2400, 0, 2400, 4000), // 1
                new Line(2400, 4000, 0, 4000), // 2
                new Line(0, 4000, 0, 0), // 3
                new Line(800, 800, 1600, 800), // 4
                new Line(1600, 800, 1600, 1600), // 5
                new Line(1600, 1600, 800, 1600), // 6
                new Line(800, 1600, 800, 800), // 7
                new Line(800, 2400, 1100, 2400), // 8
                new Line(new Point(1100, 2400), new Point(1300, 2400), false), // 8
                new Line(1300, 2400, 1600, 2400), // 8
                new Line(1600, 2400, 1600, 3200), // 9
                new Line(1600, 3200, 800, 3200), // 10
                new Line(800, 3200, 800, 2400), // 11
            }),
        // goal
        new Rectangle(1125, 2400, 1275, 2350),
        // start
        new Point(1400, 780),
        // fuel
        35
    ).addMeatball(new Point(400, 2800));

    public static readonly Level level3 = new Level(
        /*
         *      ____________________________
         *      | S                        |
         *      |__                        |
         *        /                        |
         *       /                         |
         *      |                          |
         *      |______  __________  ______|
         *            |  |   __   |  |      
         *            |  |  |* |  |  |      
         *            |  |  |  |  |  |      
         *            |  |   ||   |  |      
         *       _____|  |___||___|  |_____ 
         *      |                          |
         *      |                          |
         *       \            |>          / 
         *        \  /\  /\  _|  /\  /\  /  
         *         \/  \/  \/  \/  \/  \/   
         */

        //shapes       
        new List<Shape>( new [] {
            // upper atrium
            new Line(new Point(0, 0), new Point(4000, 0)),
            new Line(new Point(4000, 0), new Point(4000, 1500)),
            new Line(new Point(4000, 1500), new Point(3000, 1500)),
            new Line(new Point(2800, 1500), new Point(1200, 1500)),
            new Line(new Point(1000, 1500), new Point(0, 1500)),
            new Line(new Point(0, 1500), new Point(0, 700)),
            new Line(new Point(0, 700), new Point(200, 400)),
            new Line(new Point(200, 400), new Point(0, 400)),
            new Line(new Point(0, 400), new Point(0, 0)),
            
            // a series of tubes
            new Line(new Point(1000, 1500), new Point(1000, 3500)),
            new Line(new Point(1200, 1500), new Point(1200, 3500)),
            new Line(new Point(2800, 1500), new Point(2800, 3500)),
            new Line(new Point(3000, 1500), new Point(3000, 3500)),
            
            // inner box thing with the meatball in it
            new Line(new Point(1200, 3500), new Point(1950, 3500)),
            new Line(new Point(1950, 3500), new Point(1950, 2125)),
            new Line(new Point(1950, 2125), new Point(1875, 2125)),
            new Line(new Point(1875, 2125), new Point(1875, 1875)),
            new Line(new Point(1875, 1875), new Point(2125, 1875)),
            new Line(new Point(2125, 1875), new Point(2125, 2125)),
            new Line(new Point(2125, 2125), new Point(2050, 2125)),
            new Line(new Point(2050, 2125), new Point(2050, 3500)),
            new Line(new Point(2050, 3500), new Point(2800, 3500)),
            
            // lower atrium
            new Line(new Point(1000, 3500), new Point(0, 3500)),
            new Line(new Point(0, 3500), new Point(0, 4000)),
            new Line(new Point(0, 4000), new Point(1000, 5000)), // First diagonal line in the whole game lmao
            new Line(new Point(1000, 5000), new Point(1333, 4667)),
            new Line(new Point(1333, 4667), new Point(1667, 5000)),
            new Line(new Point(1667, 5000), new Point(1750, 4917)),
            new Line(new Point(1750, 4917), new Point(2250, 4917), false), // this is the flat bit with the goal on it
            new Line(new Point(2250, 4917), new Point(2333, 5000)),
            new Line(new Point(2333, 5000), new Point(2667, 4667)),
            new Line(new Point(2667, 4667), new Point(3000, 5000)),
            new Line(new Point(3000, 5000), new Point(4000, 4000)),
            new Line(new Point(4000, 4000), new Point(4000, 3500)),
            new Line(new Point(4000, 3500), new Point(3000, 3500)),

        }),
        // goal
        new Rectangle(1950, 4917, 2050, 4817),
        // start
        new Point(170, 370),
        // fuel
        0
    ).addMeatball(new Point(2000, 2000));
}