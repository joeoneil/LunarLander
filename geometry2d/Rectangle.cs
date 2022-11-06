using System;
using System.Collections.Generic;

namespace LunarLander.geometry2d; 

public class Rectangle : Polygon {
    public double width { get; private set; }
    public double height { get; private set; }
    
    public double left { get; private set; }
    public double right { get; private set; }
    public double top { get; private set; }
    public double bottom { get; private set; }

    public Rectangle(Point p1, Point p2) : base(
        new List<Point> { p1, new (p2.x, p1.y), p2, new (p1.x, p2.y) }) {
        width = Math.Abs(p2.x - p1.x);
        height = Math.Abs(p2.y - p1.y);
        left = Math.Min(p1.x, p2.x);
        right = Math.Max(p1.x, p2.x);
        top = Math.Min(p1.y, p2.y);
        bottom = Math.Max(p1.y, p2.y);
    }
    public Rectangle(double x1, double y1, double x2, double y2) : this(new Point(x1, y1), new Point(x2, y2)) { }
    public Rectangle(Point p, double width, double height) : this(p, new Point(p.x + width, p.y + height)) { }
    public Rectangle(double width, double height) : this(new Point(), width, height) { }

    public bool intersects(Rectangle other) {
        return !(other.points[0].x > points[2].x || other.points[2].x < points[0].x || other.points[0].y > points[2].y || other.points[2].y < points[0].y);
    }

    protected override void updateCentroid() 
    {
        // no point in doing calculation for a generic polygon when a rectangle is much easier.
        centroid = new Point(points[0].x + width / 2, points[0].y + height / 2);
    }

    public override Bounds getBoundingBox() {
        // this will return an upright rectangle even if the rectangle is rotated.
        this.boundingBoxCached = true;
        this.boundingBox = new Bounds(points[0], points[2]);
        return boundingBox;
    }
}
