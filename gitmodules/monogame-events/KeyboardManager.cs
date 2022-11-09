using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MonoGameEvents;

public class KeyboardManager {
    private readonly KeyboardState[] prevState;
    private readonly int debounce;
    private int ptr;
    private readonly List<ButtonEvent> handles;

    private struct ButtonEvent {
        public readonly Keys key;
        public readonly PressType pressType;
        public readonly Action callback;
        public readonly bool invert;

        public ButtonEvent(Keys key, Action callback, PressType pressType, bool invert) {
            this.key = key;
            this.callback = callback;
            this.pressType = pressType;
            this.invert = invert;
        }
    }

    private enum PressType {
        Pressed,
        Held,
        Released,
    }

    public KeyboardManager(int debounce = 6) {
        this.debounce = debounce;
        this.prevState = new KeyboardState[debounce];
        this.handles = new List<ButtonEvent>();
    }

    public void Update(KeyboardState newState) {
        ++ptr;
        ptr %= debounce;
        prevState[ptr] = newState;

        foreach (ButtonEvent handle in handles) {
            switch (handle.pressType) {
                case PressType.Pressed:
                    if (IsPressed(handle.key) ^ handle.invert) handle.callback();
                    break;
                case PressType.Held:
                    if (IsHeld(handle.key) ^ handle.invert) handle.callback();
                    break;
                case PressType.Released:
                    if (IsReleased(handle.key) ^ handle.invert) handle.callback();
                    break;
            }
        }
    }

    private int FramesPressed(Keys key) {
        var acc = 0;
        for (var i = 0; i < debounce; i++) {
            if (prevState[i].IsKeyDown(key)) {
                acc++;
            }
        }

        return acc;
    }

    private KeyboardState GetState(int prev) {
        return prevState[(ptr - prev + debounce) % debounce];
    }

    private KeyboardState GetState() {
        return GetState(0);
    }

    private bool IsPressed(Keys key) {
        return GetState().IsKeyDown(key) && FramesPressed(key) == 1;
    }

    private bool IsHeld(Keys key, int threshold = 2) {
        if (GetState().IsKeyUp(key)) return false;
        for (int i = 1; i < debounce; i++) {
            if (i >= threshold) {
                return true;
            }

            if (GetState(i).IsKeyUp(key)) {
                return false;
            }
        }

        return false;
    }

    private bool IsReleased(Keys key) {
        return GetState().IsKeyUp(key) && GetState(1).IsKeyDown(key);
    }

    public void OnPressed(Keys key, Action callback, bool invert = false) {
        AddCallback(key, callback, PressType.Pressed, invert);
    }

    public void OnHeld(Keys key, Action callback, bool invert = false) {
        AddCallback(key, callback, PressType.Held, invert);
    }

    public void OnReleased(Keys key, Action callback, bool invert = false) {
        AddCallback(key, callback, PressType.Released, invert);
    }

    private void AddCallback(Keys key, Action callback, PressType pressType, bool invert = false) {
        handles.Insert(0, new ButtonEvent(key, callback, pressType, invert));
    }
}