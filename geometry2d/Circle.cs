using System;
using System.Linq;

namespace LunarLander.geometry2d; 

public class Circle : Shape{
    public Point center { get; private set; }
    public double radius { get; private set; }
    
    public Circle(Point center, double radius) {
        this.center = center;
        this.radius = radius;
    }
    
    public override bool contains(double x, double y) {
        return center.distance(x, y) <= radius;
    }

    public override void translate(double x, double y) {
        center.translate(x, y);
    }

    public override void rotate(double angle, Point p) {
        center.rotate(angle, p);
    }

    protected override void updateCentroid() {
        centroid = center;
    }
    
    protected override void updateBoundingBox() {
        boundingBox = new Bounds(center.x - radius, center.y - radius, radius * 2, radius * 2);
    }

    public override double squareDistance(double x, double y) {
            return Math.Abs(center.squareDistance(x, y) - radius * radius);
    }
    
    public override bool intersects(Shape s) {
        return s switch {
            Circle circle => center.distance(circle.center) <= radius + circle.radius,
            Rectangle rectangle => intersects(rectangle),
            Polygon polygon => intersects(polygon),
            _ => throw new Exception("Unknown shape type")
        };
    }

    public override Point contactNormal(Shape s) {
        throw new System.NotImplementedException();
    }

    private bool intersects(Rectangle r) {
        double dx = Math.Abs(center.x - r.getCentroid().x);
        double dy = Math.Abs(center.y - r.getCentroid().y);

        if (dx > r.width / 2 + radius) { return false; }
        if (dy > r.height / 2 + radius) { return false; }

        if (dx <= r.width / 2) { return true; }
        if (dy <= r.height / 2) { return true; }

        double cornerDistance_sq = Math.Pow(dx - r.width / 2, 2) +
                                   Math.Pow(dy - r.height / 2, 2);

        return cornerDistance_sq <= Math.Pow(radius, 2);
    }
    
    private bool intersects(Polygon p) {
        return p.lines.Any(intersects);
    }

    private bool intersects(Line l) {
        double dx = l.p2.x - l.p1.x;
        double dy = l.p2.y - l.p1.y;

        double a = dx * dx + dy * dy;
        double b = 2 * (dx * (l.p1.x - center.x) + dy * (l.p1.y - center.y));
        double c = (l.p1.x - center.x) * (l.p1.x - center.x) + (l.p1.y - center.y) * (l.p1.y - center.y) - radius * radius;

        double discriminant = b * b - 4 * a * c;
        if (discriminant < 0) {
            return false;
        } else {
            discriminant = Math.Sqrt(discriminant);

            double t1 = (-b - discriminant) / (2 * a);
            double t2 = (-b + discriminant) / (2 * a);

            if (t1 is >= 0 and <= 1) {
                return true;
            }

            return t2 is >= 0 and <= 1;
        }
    }
}