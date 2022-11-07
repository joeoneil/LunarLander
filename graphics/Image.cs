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
        this.texture = new Texture2D(g, (int) width, (int) height);
        this.buffer = new Color[width * height];
        this.width = width;
        this.height = height;
        this.reset();
    }

    public void reset() {
        Drawing.fill(this, Color.Black);
        this.texture_changed = true;
    }
    
    public void setPixel(uint x, uint y, Color color) {
        buffer[y * width + x] = color;
    }
    
    public Color getPixel(uint x, uint y) {
        return buffer[y * width + x];
    }
    
    public Texture2D toTexture2D() {
        if (!texture_changed) return texture;
        texture.SetData(buffer);
        texture_changed = false;
        return texture;
    }
}