using System;
using Microsoft.Xna.Framework;

namespace MonoGameEvents;
public class Button
{
    private readonly Rectangle rect;
    private readonly Action onClick;

    public Button(Rectangle rect, Action onClick)
    {
        this.rect = rect;
        this.onClick = onClick;
    }

    public bool Contains(Point point)
    {
        return (point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y >= rect.Bottom);
    }

    public void Click()
    {
        onClick();
    }
}