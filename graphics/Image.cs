using System.Collections.Generic;
using LunarLander.geometry2d;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LunarLander.graphics; 

public class Image {
    public uint width { get; private set; }
    public uint height { get; private set; }
    
    public Color[] buffer { get; private set; }
    public bool texture_changed { get; set; }
    
    public Texture2D texture { get; private set; }
    
    public Image(GraphicsDevice g, uint width, uint height) {
        texture = new Texture2D(g, (int)width, (int)height);
        this.buffer = new Color[width * height];
        this.width = width;
        this.height = height;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                buffer[x + y * width] = Color.Black;
            }
        }
        texture.SetData(buffer); // initialize texture buffer
    }

    public void reset() {
        Drawing.fill(this, Color.Black);
        this.texture_changed = true;
    }

    public void render(IReadOnlyList<IColoredShape> shapes) {
        Drawing.fill(this, Color.Black);
    }

    public Texture2D toTexture2D() {
        if (!texture_changed) return texture;
        texture.SetData(buffer);
        texture_changed = false;
        return texture;
    }
}