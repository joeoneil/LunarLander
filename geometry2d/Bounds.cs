using System;

namespace LunarLander.geometry2d; 

public struct Bounds {
    // This class is essentially a rectangle, but it is immutable and always upright.
    // It is used to represent the bounds of a shape.
    //
    // A rectangle cannot be used to represent the bounds of the shape as a rectangle
    // is also a shape, which results in a circular definition and causes a stack overflow.

    public double x { get; private set; }
    public double y { get; private set; }
    public double width { get; private set; }
    public double height { get; private set; }
    public double left { get; private set; }
    public double right { get; private set; }
    public double top { get; private set; }
    public double bottom { get; private set; }
    
    public Bounds(double x, double y, double width, double height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.left = x;
        this.right = x + width;
        this.top = y;
        this.bottom = y + height;
    }

    public Bounds(Point p1, Point p2) : this(Math.Min(p1.x, p2.x), Math.Min(p1.y, p2.y), Math.Abs(p1.x - p2.x), Math.Abs(p1.y - p2.y)) { }

    public bool intersects(Bounds other) {
        return !(other.left > this.right || other.right < this.left || other.top > this.bottom || other.bottom < this.top);
    }
    
    public bool contains(Bounds other) {
        return (other.left >= this.left && other.right <= this.right && other.top >= this.top && other.bottom <= this.bottom);
    }

    public bool contains(Point p) {
        return (p.x >= this.left && p.x <= this.right && p.y >= this.top && p.y <= this.bottom);
    }
    
    public bool contains(double x, double y) {
        return (x >= this.left && x <= this.right && y >= this.top && y <= this.bottom);
    }

    public Bounds translate(double x, double y) {
        return new Bounds(this.x + x, this.y + y, this.width, this.height);
    }

    public Bounds translate(Point p) {
        return translate(p.x, p.y);
    }
}
