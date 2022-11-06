using System;

namespace LunarLander.geometry2d; 

public class Line : Shape {
    public Point p1 { get; }
    public Point p2 { get; }
    public double m { get; }
    public double length { get; }
    
    public Line(Point p1, Point p2) {
        this.p1 = p1;
        this.p2 = p2;
        this.m = (p2.y - p1.y) / (p2.x - p1.x);
        this.length = Math.Sqrt(Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2));
    }
    
    public Line(double x1, double y1, double x2, double y2) : this(new Point(x1, y1), new Point(x2, y2)) { }

    public Line() : this(new Point(), new Point()) { }

    public override double squareDistance(double x, double y) {
        double l2 = Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2. y - p1.y, 2);
        if (l2 == 0.0) return p1.squareDistance(x, y);
        double t = ((x - p1.x) * (p2.x - p1.x) + (y - p1.y) * (p2.y - p1.y)) / l2;
        return t switch {
            < 0.0 => p1.squareDistance(x, y),
            > 1.0 => p2.squareDistance(x, y),
            _ => Math.Pow(p1.x + t * (p2.x - p1.x) - x, 2) + Math.Pow(p1.y + t * (p2.y - p1.y) - y, 2)
        };
    }

    public int lerp(double x) {
            return (int) (m * x + p1.y - m * p1.x);
    }

    public override void translate(double x, double y) {
        p1.translate(x, y);
        p2.translate(x, y);
        boundingBoxCached = false;
        centroidCached = false;
    }
    
    public override void rotate(double angle, Point p) {
        p1.rotate(angle, p);
        p2.rotate(angle, p);
        boundingBoxCached = false;
        centroidCached = false;
    }

    public override bool intersects(Shape s) {
        return s switch {
            Line l => intersects(l),
            Circle c => c.intersects(this),
            Rectangle r => r.intersects(this),
            Polygon p => p.intersects(this),
            _ => false
        };
    }

    public override bool contains(double x, double y) {
        return squareDistance(x, y) == 0;
    }
    
    private bool intersects(Line l) {
        return Point.ccw(this.p1, this.p2, l.p2) != Point.ccw(this.p2, l.p1, l.p2) && Point.ccw(this.p1, this.p2, l.p1) != Point.ccw(this.p1, this.p2, l.p2);
    }

    protected override void updateCentroid() {
            centroid = new Point((p1.x + p2.x) / 2, (p1.y + p2.y) / 2);
    }

    protected override void updateBoundingBox() {
        boundingBox = new Bounds(p1, p2);
    }
}