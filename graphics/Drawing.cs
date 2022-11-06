using System;
using LunarLander.graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LunarLander.geometry2d; 

public static class Drawing {
    
    public static readonly Color defaultBG = Color.Black;
    public static readonly Color defaultFG = Color.White;
    public static void drawShape(Image image, Shape shape, Color color, int thickness = 1, bool filled = false) {
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
        // if the line is outside the image, don't draw it
        if (line.p1.x < 0 && line.p2.x < 0) return;
        if (line.p1.y < 0 && line.p2.y < 0) return;
        if (line.p1.x > image.width && line.p2.x > image.width) return;
        if (line.p1.y > image.height && line.p2.y > image.height) return;
        
        // if the thickness is 0 or less, don't draw it
        if (thickness <= 0) return;
        
        // check each pixel that is both in the image and the line's bounding box
        Bounds bounds = line.getBoundingBox();
        for (uint x = (uint) Math.Max(0, bounds.left - thickness); x < Math.Min(bounds.right + thickness, image.width); x++) {
            for (uint y = (uint) Math.Max(0, bounds.top - thickness); y < Math.Min(bounds.bottom + thickness, image.height); y++) {
                // if the pixel is in the line, draw it
                if (line.squareDistance(x, y) <= thickness * thickness) {
                    image.setPixel(x, y, color);
                }
            }
        }
    }

    public static void drawLine(Image image, Line line, int thickness = 1) {
        // white is apparently not a compile time constant
        drawLine(image, line, defaultFG, thickness);
    }
    
    public static void drawPolygon(Image image, Polygon polygon, Color color, int thickness = 1, bool filled = false) {
        if (filled) {
            Bounds bounds = polygon.getBoundingBox();
            // check each pixel that is both in the image and the polygon's bounding box
            for (uint x = (uint) Math.Max(0, bounds.left - thickness); x < Math.Min(bounds.right + thickness, image.width); x++) {
                for (uint y = (uint) Math.Max(0, bounds.top - thickness); y < Math.Min(bounds.bottom + thickness, image.height); y++) {
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
        // if the circle is outside the image, don't draw it
        if (circle.center.x + circle.radius < 0) return;
        if (circle.center.y + circle.radius < 0) return;
        if (circle.center.x - circle.radius > image.width) return;
        if (circle.center.y - circle.radius > image.height) return;
        
        // if the thickness is 0 or less, don't draw it
        if (thickness <= 0) return;
        
        // check each pixel that is both in the image and the circle's bounding box
        Bounds bounds = circle.getBoundingBox();
        for (uint x = (uint) Math.Max(0, bounds.left - thickness); x < Math.Min(bounds.right + thickness, image.width); x++) {
            for (uint y = (uint) Math.Max(0, bounds.top - thickness); y < Math.Min(bounds.bottom + thickness, image.height); y++) {
                // if fill is true and the pixel is in the circle, draw it
                if (fill && circle.contains(new Point(x, y))) {
                    image.setPixel(x, y, color);
                }
                else {
                    // if the pixel is on the circle, draw it
                    // (this seems to draw a circle with a thickness thinner than the specified thickness, but I don't know why)
                    if (circle.squareDistance(x, y) <= thickness * thickness) {
                        image.setPixel(x, y, color);
                    }
                }
            }
        }
    }

    public static void fill(Image image, Color color) {
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