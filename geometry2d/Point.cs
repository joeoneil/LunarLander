using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LunarLander.geometry2d;
    
public class Point
{
    public double x { get; private set; }
    public double y { get; private set; }

    public Point(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public Point() : this(0, 0) { }

    // I love operator overloading
    public static Point operator +(Point a, Point b) => new (a.x + b.x, a.y + b.y);
    public static Point operator -(Point a, Point b) => a + -b;
    public static Point operator -(Point a) => new (-a.x, -a.y);
    public static Point operator *(Point a, Point b) => new (a.x * b.x, a.y * b.y);
    public static Point operator *(Point a, double s) => new (a.x * s, a.y * s);
    public static Point operator *(double s, Point a) => a * s;
    public static Point operator /(Point a, Point b) => new (a.x / b.x, a.y / b.y);
    public static Point operator /(Point a, double s) => a * (1 / s);
    public static Point operator /(double s, Point a) => new (s / a.x, s / a.y);
    public static double operator %(Point a, Point b) => a.cross(b);
    public static double operator ^(Point a, Point b) => a.dot(b);
    
    
    public double dot(Point other) => x * other.x + y * other.y;
    public double cross(Point other) => x * other.y - y * other.x;
    
    public double squareDistance(Point p) {
        double dx = this.x - p.x;
        double dy = this.y - p.y;
        return dx * dx + dy * dy;
    }

    public double squareDistance(double x, double y) {
        double dx = this.x - x;
        double dy = this.y - y;
        return dx * dx + dy * dy;
    }
    
    public double distance(double x, double y) {
        return Math.Sqrt(squareDistance(x, y));
    }
    
    public double distance(Point p) => Math.Sqrt(squareDistance(p.x, p.y));
    
    public static bool ccw(Point a, Point b, Point c)
    {
        return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x);
    }
    
    public static bool ccw(List<Point> points)
    {
        bool result = true;
        for (int i = 0; i < points.Count; i++) {
            result &= ccw(points[i], points[(i + 1) % points.Count], points[(i + 2) % points.Count]);
        }
        return result;
    }
    
    public void translate(Point p) {
        translate(p.x, p.y);
    }

    public void translate(double x, double y) {
        this.x += x;
        this.y += y;
    }

    public void rotate(double angle, Point center) {
        // rotate point around center
        double s = Math.Sin(angle);
        double c = Math.Cos(angle);
        double dx = x - center.x;
        double dy = y - center.y;
        x = center.x + dx * c - dy * s;
        y = center.y + dx * s + dy * c;
    }

    public Point normalize() {
        double length = this.magnitude();
        x /= length;
        y /= length;
        return this;
    }
    
    public Point normalized() {
        return new Point(x, y).normalize();
    }

    public Point project(Point p) {
        double dot = this.dot(p);
        return this.normalized() * dot;
    }

    public double magnitude() {
        return this.distance(0, 0);
    }

    public Point clone() {
        return new Point(x, y);
    }
    
    public Vector2 toVector2() {
        return new Vector2((float)x, (float)y);
    }
}