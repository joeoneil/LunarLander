using System;
using System.Collections.Generic;

namespace LunarLander.geometry2d; 

public class Line : Shape {
    public Point p1 { get; private set; }
    public Point p2 { get; private set; }
    public double m { get; private set; }
    public double length { get; private set; }

    private readonly bool greebleable; // this is not a word, but it's fun to say. Try it.
    
    public Line(Point p1, Point p2, bool greebleable = true) {
        this.p1 = p1;
        this.p2 = p2;
        this.m = (p2.y - p1.y) / (p2.x - p1.x);
        this.length = Math.Sqrt(Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2));
        this.greebleable = greebleable;
    }
    
    public Line(double x1, double y1, double x2, double y2) : this(new Point(x1, y1), new Point(x2, y2)) { }

    public Line() : this(new Point(), new Point()) { }

    public override double squareDistance(double x, double y) {
        double l2 = Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2. y - p1.y, 2);
        if (Math.Abs(l2) < 0.0001) {
            return p1.squareDistance(x, y);
        }
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

    public override bool intersects(Shape other) {
        return other switch {
            Line l => intersects(l),
            Circle c => c.intersects(this),
            Rectangle r => r.intersects(this),
            Polygon p => p.intersects(this),
            _ => false
        };
    }

    public override bool contains(double x, double y) {
        return Math.Abs(squareDistance(x, y)) < 0.0001;
    }
    
    private bool intersects(Line l) {
        // https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
        double s1_x = p2.x - p1.x;
        double s1_y = p2.y - p1.y;
        double s2_x = l.p2.x - l.p1.x;
        double s2_y = l.p2.y - l.p1.y;
        double s = (-s1_y * (p1.x - l.p1.x) + s1_x * (p1.y - l.p1.y)) / (-s2_x * s1_y + s1_x * s2_y);
        double t = (s2_x * (p1.y - l.p1.y) - s2_y * (p1.x - l.p1.x)) / (-s2_x * s1_y + s1_x * s2_y);
        return s is >= 0 and <= 1 && t is >= 0 and <= 1;
    }

    protected override void updateCentroid() {
            centroid = new Point((p1.x + p2.x) / 2, (p1.y + p2.y) / 2);
    }

    protected override void updateBoundingBox() {
        boundingBox = new Bounds(p1, p2);
    }

    public override Point contactNormal(Shape other) {
        return other switch {
            Polygon p => contactNormal(p),
            Circle c => contactNormal(c),
            Line => contactNormal(),
            _ => throw new ArgumentOutOfRangeException(nameof(other), other, null)
        };
    }
    
    private Point contactNormal(Polygon p) {
        return p.contactNormal(this);
    }

    private Point contactNormal(Circle c) {
        return c.contactNormal(this);
    }

    private Point contactNormal() {
        return new Point(p2.y - p1.y, p1.x - p2.x);
    }

    public void setPoints(Point p1, Point p2) {
        this.p1 = p1;
        this.p2 = p2;
        this.length = p1.distance(p2);
        this.m = (p2.y - p1.y) / (p2.x - p1.x);
        boundingBoxCached = false;
        centroidCached = false;
    }

    public Point intersectionPoint(Line l) {
        double x = ((p1.x * p2.y - p1.y * p2.x) * (l.p1.x - l.p2.x) - (p1.x - p2.x) * (l.p1.x * l.p2.y - l.p1.y * l.p2.x)) / ((p1.x - p2.x) * (l.p1.y - l.p2.y) - (p1.y - p2.y) * (l.p1.x - l.p2.x));
        double y = ((p1.x * p2.y - p1.y * p2.x) * (l.p1.y - l.p2.y) - (p1.y - p2.y) * (l.p1.x * l.p2.y - l.p1.y * l.p2.x)) / ((p1.x - p2.x) * (l.p1.y - l.p2.y) - (p1.y - p2.y) * (l.p1.x - l.p2.x));
        return new Point(x, y);
    }

    public int side(double x, double y) {
        return Math.Sign((p2.x - p1.x) * (y - p1.y) - (p2.y - p1.y) * (x - p1.x));
    }

    public int side(Point p) {
        return side(p.x, p.y);
    }
    
    public Point normal() {
        return -new Point(p2.y - p1.y, p1.x - p2.x);
    }

    public Point parallel() {
        return new Point(p2.x - p1.x, p2.y - p1.y);
    }

    public IEnumerable<Line> greeble(int numSplits, double maxDeviation) {
        if (!greebleable) {
            return new List<Line> {
                this.clone()
            };
        }
        List<Point> points = new () { p1 };
        Point normal = this.normal().normalized();
        for (int i = 0; i < numSplits; i++) {
            double t = (i + 1) / (double) (numSplits + 1);
            double x = p1.x + t * (p2.x - p1.x);
            double y = p1.y + t * (p2.y - p1.y);
            double deviation = maxDeviation * (2 * LunarLander.rng.NextDouble() - 1);
            points.Add(new Point(x, y) + normal * deviation);
        }
        points.Add(p2);
        List<Line> lines = new ();
        for (int i = 0; i < points.Count - 1; i++) {
            lines.Add(new Line(points[i].clone(), points[i + 1].clone()));
        }
        return lines;
    }

    public void scale(double factor) {
        this.p1 = new Point(p1.x * factor, p1.y * factor);
        this.p2 = new Point(p2.x * factor, p2.y * factor);
        this.m = (p2.y - p1.y) / (p2.x - p1.x);
        this.length = p1.distance(p2);
        boundingBoxCached = false;
        centroidCached = false;
        updateBoundingBox();
        updateCentroid();
    }

    public Line clone() {
        return new Line(p1.clone(), p2.clone());
    }
}
