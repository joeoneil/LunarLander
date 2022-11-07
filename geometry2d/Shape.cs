using System;

namespace LunarLander.geometry2d; 

public abstract class Shape {
    protected Point centroid;
    protected Bounds boundingBox;

    protected bool centroidCached;
    protected bool boundingBoxCached;
    
    protected Shape() {
        centroidCached = false;
        boundingBoxCached = false;
    }
    
    public Point getCentroid() {
        if (centroidCached) return centroid;
        updateCentroid();
        centroidCached = true;
        return centroid;
    }
    
    public virtual Bounds getBoundingBox() {
        if (boundingBoxCached) return boundingBox;
        updateBoundingBox();
        boundingBoxCached = true;
        return boundingBox;
    }

    public abstract void translate(double x, double y);

    public void translate(Point p) {
        translate(p.x, p.y);
    }

    public abstract void rotate(double angle, Point p);

    public void rotate(double angle) {
        rotate(angle, getCentroid());
    }
    
    protected abstract void updateCentroid();
    
    protected abstract void updateBoundingBox();

    public abstract bool intersects(Shape other);

    public bool contains(Point p) {
        return contains(p.x, p.y);
    }

    public abstract bool contains(double x, double y);

    public double squareDistance(Point p) {
        return squareDistance(p.x, p.y);
    }

    public abstract double squareDistance(double x, double y);

    public double distance(Point p) {
            return Math.Sqrt(squareDistance(p.x, p.y));
    }

    public double distance(double x, double y) {
        return Math.Sqrt(squareDistance(x, y));
    }

    public abstract Point contactNormal(Shape other);
}