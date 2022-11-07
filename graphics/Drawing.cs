using System;
using LunarLander.graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LunarLander.geometry2d; 

public static class Drawing {
    
    public static readonly Color defaultBG = Color.Black;
    public static readonly Color defaultFG = Color.White;
    public static void drawShape(Image image, Shape shape, Color color, int thickness = 1, bool filled = false) {
        image.texture_changed = true;
        switch (shape) {
            case Circle circle:
                drawCircle(image, circle, color, thickness, filled);
                break;
            case Polygon polygon:
                drawPolygon(image, polygon, color, thickness, filled);
                break;
            case Line line:
                drawLine(image, line, color, thickness);
                break;
        }
    }

    public static void drawShape(Image image, Shape shape, int thickness = 1, bool filled = false) {
        drawShape(image, shape, defaultFG, thickness, filled);
    }
    
    public static void drawLine(Image image, Line line, Color color, int thickness = 1) {
        image.texture_changed = true;
        // if the line is outside the image, don't draw it
        if (line.p1.x < 0 && line.p2.x < 0) return;
        if (line.p1.y < 0 && line.p2.y < 0) return;
        if (line.p1.x > image.width && line.p2.x > image.width) return;
        if (line.p1.y > image.height && line.p2.y > image.height) return;
        
        // if the thickness is 0 or less, don't draw it
        if (thickness <= 0) return;
        
        // draw a circle at each end of the line if the thickness is greater than 1
        if (thickness > 1) {
            drawCircle(image, new Circle(line.p1, thickness / 2.0), color, 1, true);
            drawCircle(image, new Circle(line.p2, thickness / 2.0), color, 1, true);
        }
        // get the unit vector of the line
        Point unit = line.parallel().normalized();
        Point normal = line.normal().normalized();
        Point scaledNormal = normal * thickness / 2;
        Point p = line.p1;
        double x_ = p.x;
        double y_ = p.y;
        int width = (int)image.width;
        int height = (int)image.height;
        double ll = line.length;
        // draw a pixel at each point along the line
        for (int i = 0; i < ll; i++) {
            x_ += unit.x;
            y_ += unit.y;
            x_ -= scaledNormal.x;
            y_ -= scaledNormal.y;
            for (int j = 0; j < thickness; j++) {
                x_ += normal.x;
                y_ += normal.y;
                // if the pixel is outside the image, don't draw it
                if (x_ < 0 || y_ < 0 || x_ + 0.5 >= width || y_ + 0.5 >= height) continue;
                image.setPixel((uint) Math.Round(x_), (uint) Math.Round(y_), color);
            }
            x_ -= scaledNormal.x;
            y_ -= scaledNormal.y;
        }
    }

    public static void drawLine(Image image, Line line, int thickness = 1) {
        // white is apparently not a compile time constant
        drawLine(image, line, defaultFG, thickness);
    }

    public static void drawPolygon(Image image, Polygon polygon, Color color, int thickness = 1, bool filled = false) {
        image.texture_changed = true;
        if (filled) {
            Bounds bounds = polygon.getBoundingBox();
            // check each pixel that is both in the image and the polygon's bounding box
            int right = (int) Math.Min(image.width, bounds.right + thickness);
            int bottom = (int) Math.Min(image.height, bounds.bottom + thickness);
            for (uint x = (uint) Math.Max(0, bounds.left - thickness); x < right; x++) {
                for (uint y = (uint) Math.Max(0, bounds.top - thickness); y < bottom; y++) {
                    // if the pixel is in the polygon, draw it
                    if (!polygon.contains(new Point(x, y))) continue;
                    image.setPixel(x, y, color);
                }
            }
        }
        // draw each line in the polygon
        foreach (Line line in polygon.getLines()) {
            drawLine(image, line, color, thickness);
        }
    }
    
    public static void drawPolygon(Image image, Polygon polygon, int thickness = 1) {
        // white is apparently not a compile time constant
        drawPolygon(image, polygon, defaultFG, thickness);
    }
    
    public static void drawCircle(Image image, Circle circle, Color color, int thickness = 1, bool fill = false) {
        image.texture_changed = true;
        drawCircle(image, circle.center, circle.radius, color, thickness, fill);
    }
    public static void drawCircle(Image image, Point center, double radius, Color color, int thickness = 1, bool fill = false) {
        image.texture_changed = true;
        // if the circle is outside the image, don't draw it
        if (center.x + radius < 0) return;
        if (center.y + radius < 0) return;
        if (center.x - radius > image.width) return;
        if (center.y - radius > image.height) return;
        
        // if the thickness is 0 or less, don't draw it
        if (thickness <= 0) return;
        
        // check each pixel that is both in the image and the circle's bounding box
        int right = (int) Math.Min(image.width, center.x + radius + thickness);
        int bottom = (int) Math.Min(image.height, center.y + radius + thickness);
        Bounds bounds = new Bounds(center.x - radius, center.y - radius, radius * 2, radius * 2);
        for (uint x = (uint) Math.Max(0, bounds.left - thickness); x < right; x++) {
            for (uint y = (uint) Math.Max(0, bounds.top - thickness); y < bottom; y++) {
                // if fill is true and the pixel is in the circle, draw it
                if (fill && center.squareDistance(x, y) <= radius * radius) {
                    image.setPixel(x, y, color);
                }
                else {
                    // if the pixel is on the circle, draw it
                    // (this seems to draw a circle with a thickness thinner than the specified thickness, but I don't know why)
                    if (Math.Abs(center.squareDistance(x, y) - radius * radius) <= thickness * thickness) {
                        image.setPixel(x, y, color);
                    }
                }
            }
        }
    }
    public static void drawCircle(Image image, Circle circle, int thicknesss = 1, bool fill = false) {
        drawCircle(image, circle.center, circle.radius, defaultFG, thicknesss, fill);
    }
    public static void drawCirlce(Image image, Point center, double radius, int thickness = 1, bool fill = false) {
        drawCircle(image, center, radius, defaultFG, thickness, fill);
    }

    public static void drawBounds(Image image, Bounds bounds, Color color, int thickness = 1, bool fill = false) {
            // if the bounds are outside the image, don't draw them
        if (bounds.right < 0) return;
        if (bounds.bottom < 0) return;
        if (bounds.left > image.width) return;
        if (bounds.top > image.height) return;
        
        // if the thickness is 0 or less, don't draw them
        if (thickness <= 0) return;
        
        // if fill is true, draw a rectangle
        if (fill) {
            for (uint x = (uint) Math.Max(0, bounds.left - thickness); x < Math.Min(bounds.right + thickness, image.width); x++) {
                for (uint y = (uint) Math.Max(0, bounds.top - thickness); y < Math.Min(bounds.bottom + thickness, image.height); y++) {
                    image.setPixel(x, y, color);
                }
            }
        }
        // otherwise, draw four lines
        else {
            drawLine(image, new Line(bounds.left, bounds.top, bounds.right, bounds.top), color, thickness);
            drawLine(image, new Line(bounds.right, bounds.top, bounds.right, bounds.bottom), color, thickness);
            drawLine(image, new Line(bounds.right, bounds.bottom, bounds.left, bounds.bottom), color, thickness);
            drawLine(image, new Line(bounds.left, bounds.bottom, bounds.left, bounds.top), color, thickness);
        }
    }

    public static void fill(Image image, Color color) {
        image.texture_changed = true;
        for (uint x = 0; x < image.width; x++) {
            for (uint y = 0; y < image.height; y++) {
                image.setPixel(x, y, color);
            }
        }
    }
    
    public static void fill(Image image) {
        fill(image, defaultBG);
    }
}