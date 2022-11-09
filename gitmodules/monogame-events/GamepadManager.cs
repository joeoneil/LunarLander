using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MonoGameEvents;

public class GamepadManager {
    private readonly GamePadState[] prevState;
    private readonly int debounce;
    private int ptr = 0;
    private readonly List<ButtonEvent> handles;

    private struct ButtonEvent {
        public Buttons button;
        public PressType pressType;
        public Action callback;
        public bool invert;

        public ButtonEvent(Buttons button, Action callback, PressType pressType, bool invert) {
            this.button = button;
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

    public GamepadManager(int debounce = 6) {
        this.debounce = debounce;
        this.prevState = new GamePadState[debounce];
        this.handles = new List<ButtonEvent>();
    }

    public void Update(GamePadState newState) {
        ++ptr;
        ptr %= debounce;
        prevState[ptr] = newState;

        foreach (ButtonEvent handle in handles) {
            switch (handle.pressType) {
                case PressType.Held:
                    if (IsHeld(handle.button) ^ handle.invert) handle.callback();
                    break;
                case PressType.Pressed:
                    if (IsPressed(handle.button) ^ handle.invert) handle.callback();
                    break;
                case PressType.Released:
                    if (IsReleased(handle.button) ^ handle.invert) handle.callback();
                    break;
            }
        }
    }

    private int FramesPressed(Buttons button) {
        var acc = 0;
        for (var i = 0; i < debounce; i++) {
            if (prevState[i].IsButtonDown(button)) {
                acc++;
            }
        }

        return acc;
    }

    private GamePadState GetState() {
        return GetState(0);
    }

    private GamePadState GetState(int prev) {
        return prevState[(ptr - prev + debounce) % debounce];
    }

    private bool IsPressed(Buttons button) {
        return GetState().IsButtonDown(button) && FramesPressed(button) == 1;
    }

    private bool IsHeld(Buttons button, int threshold = 2) {
        if (GetState().IsButtonUp(button)) return false;
        for (var i = 0; i < debounce; i++) {
            if (i >= threshold) return true;
            if (GetState(i).IsButtonUp(button)) return false;
        }

        return false;
    }

    private bool IsReleased(Buttons button) {
        return GetState().IsButtonUp(button) && GetState(1).IsButtonDown(button);
    }

    public void OnPressed(Buttons button, Action callback, bool invert = false) {
        AddCallback(button, callback, PressType.Pressed, invert);
    }

    public void OnHeld(Buttons button, Action callback, bool invert = false) {
        AddCallback(button, callback, PressType.Held, invert);
    }

    public void OnReleased(Buttons button, Action callback, bool invert = false) {
        AddCallback(button, callback, PressType.Released, invert);
    }

    private void AddCallback(Buttons button, Action callback, PressType pressType, bool invert) {
        handles.Insert(0, new ButtonEvent(button, callback, pressType, invert));
    }
}