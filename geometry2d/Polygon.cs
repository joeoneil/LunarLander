using System;
using System.Collections.Generic;
using System.Linq;

namespace LunarLander.geometry2d;

public class Polygon : Shape {
    public List<Point> points { get; }
    public List<Line> lines;
    private bool linesCached;

    public Polygon(List<Point> points) {
        // if the list of points is too small, throw an exception
        if (points.Count <= 2) {
            throw new ArgumentException("Polygon must have at least 3 points");
        }
        
        // if the points are defined in a clockwise order, reverse the list
        if (!ccw(points)) {
            points.Reverse();
        }

        this.points = points;
        this.lines = new List<Line>();
        // initialize the lines list
        for (int i = 0; i < points.Count; i++) {
            lines.Add(new Line(points[i], points[(i + 1) % points.Count]));
        }
        linesCached = true;
        updateCentroid();
        updateLines();
        updateBoundingBox();
    }

    public override double squareDistance(double x, double y) {
        // if the point is inside the polygon, return 0
        if (contains(x, y)) {
            return 0;
        }

        // otherwise, return the square distance to the closest edge
        return lines.Min(l => l.squareDistance(x, y));
    }

    public override bool contains(double x, double y) {
        int i, j;
        bool c = false;
        for (i = 0, j = points.Count - 1; i < points.Count; j = i++) {
            if (points[i].y > y != points[j].y > y &&
                x < (points[j].x - points[i].x) * (y - points[i].y) / (points[j].y - points[i].y) + points[i].x)
                c = !c;
        }
        return c;
    }
    
    public override bool intersects(Shape s) {
        if (!this.getBoundingBox().intersects(s.getBoundingBox())) {
            return false;
        }
        return s switch {
            Polygon polygon => intersects(polygon),
            Circle circle => intersects(circle),
            _ => throw new ArgumentException("Unknown shape type")
        };
    }
    
    private bool intersects(Polygon p) {
        return p.lines.Any(intersects);
    }

    private bool intersects(Circle c) {
            return lines.Any(c.intersects);
    }
    
    private bool intersects(Line l) {
        return lines.Any(l.intersects);
    }

    public override void translate(double x, double y) {
        foreach (Point p in points) {
            p.translate(x, y);
        }
        centroidCached = false;
        linesCached = false;
        boundingBoxCached = false;
    }

    public override void rotate(double angle, Point center) {
        foreach (Point point in points) {
            point.rotate(angle, center);
        }
        centroidCached = false;
        linesCached = false;
        boundingBoxCached = false;
    }
    
    protected override void updateCentroid() {
        // https://stackoverflow.com/questions/2792443/finding-the-centroid-of-a-polygon
        centroid = new Point();
        double signedArea = 0.0;

        for (int i = 0; i < points.Count; i++) {
            double x0 = points[i].x;
            double y0 = points[i].y;
            double x1 = points[(i + 1) % points.Count].x;
            double y1 = points[(i + 1) % points.Count].y;
            double a = x0 * y1 - x1 * y0;
            signedArea += a;
            centroid.translate((x0 + x1) * a, (y0 + y1) * a);
        }
        
        signedArea *= 0.5;
        centroid /= 6.0 * signedArea;

        centroidCached = true;
    }

    protected override void updateBoundingBox() {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;
        foreach (Point p in points) {
            minX = Math.Min(minX, p.x);
            minY = Math.Min(minY, p.y);
            maxX = Math.Max(maxX, p.x);
            maxY = Math.Max(maxY, p.y);
        }
        boundingBox = new Bounds(minX, minY, maxX - minX, maxY - minY);
        boundingBoxCached = true;
    }

    public List<Line> getLines() {
        if (linesCached) return lines;
        updateLines();
        linesCached = true;
        return lines;
    }
    
    private void updateLines() {
        for (int i = 0; i < points.Count; i++) {
            lines[i] = new Line(points[i], points[(i + 1) % points.Count]);
        }
        linesCached = true;
    }
    
    public static bool ccw(List<Point> points) {
        // check if the polygon is counter clockwise
        // https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
        double sum = 0;
        for (int i = 0; i < points.Count; i++) {
            int j = (i + 1) % points.Count;
            sum += (points[j].x - points[i].x) * (points[j].y + points[i].y);
        }
        return sum < 0;
    }
}