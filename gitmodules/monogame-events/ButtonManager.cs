using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MonoGameEvents;

public class ButtonManager
{
    private static readonly List<Button> buttons = new List<Button>();

    public static void AddButton(Button b)
    {
        buttons.Add(b);
    }

    public static void Update(MouseState state)
    {
        foreach (var b in buttons)
        {
            if (b.Contains(state.Position))
            {
                b.Click();
            }
        }
    }
}