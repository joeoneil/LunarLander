using System;
using System.Collections.Generic;
using Devcade;
using Microsoft.Xna.Framework.Input;

namespace MonoGameEvents;

public class ArcadeManager {
    private readonly GamePadState[] prevState;
    private readonly int debounce;
    private int ptr = 0;
    private readonly List<ArcadeEvent> handles;

    private struct ArcadeEvent {
        public Input.ArcadeButtons Button;
        public PressType pressType;
        public Action callback;
        public bool invert;
        
        public ArcadeEvent(Input.ArcadeButtons button, PressType pressType, Action callback, bool invert) {
            Button = button;
            this.pressType = pressType;
            this.callback = callback;
            this.invert = invert;
        }
    }

    private enum PressType {
        Pressed,
        Held,
        Released,
    }
    
    public ArcadeManager(int debounce = 6) {
        this.debounce = debounce;
        this.prevState = new GamePadState[debounce];
        this.handles = new List<ArcadeEvent>();
    }

    public void Update(GamePadState newState) {
        ++ptr;
        ptr %= debounce;
        prevState[ptr] = newState;

        foreach (ArcadeEvent handle in handles) {
            switch (handle.pressType) {
                case PressType.Held:
                    if (IsHeld(handle.Button) ^ handle.invert) handle.callback();
                    break;
                case PressType.Pressed:
                    if (IsPressed(handle.Button) ^ handle.invert) handle.callback();
                    break;
                case PressType.Released:
                    if (IsReleased(handle.Button) ^ handle.invert) handle.callback();
                    break;
            }
        }
    }

    private int FramesPressed(Input.ArcadeButtons button) {
        var acc = 0;
        for (var i = 0; i < debounce; ++i) {
            if (prevState[i].IsButtonDown((Buttons)button)) {
                ++acc;
            }
        }

        return acc;
    }

    public GamePadState GetState(int prev = 0) {
        return prevState[(ptr - prev + debounce) % debounce];
    }

    private bool IsPressed(Input.ArcadeButtons button) {
        return GetState().IsButtonDown((Buttons)button) && FramesPressed(button) == 1;
    }
    
    private bool IsHeld(Input.ArcadeButtons button, int threshold = 2) {
        if (GetState().IsButtonUp((Buttons)button)) return false;
        for (var i = 0; i < debounce; i++) {
            if (i >= threshold) return true;
            if (GetState(i).IsButtonDown((Buttons)button)) return false;
        }
        return false;
    }
    
    private bool IsReleased(Input.ArcadeButtons button) {
        return GetState().IsButtonUp((Buttons)button) && GetState(1).IsButtonDown((Buttons)button);
    }

    public void OnPressed(Input.ArcadeButtons button, Action callback, bool invert = false) {
            handles.Add(new ArcadeEvent(button, PressType.Pressed, callback, invert));
    }
    
    public void OnHeld(Input.ArcadeButtons button, Action callback, bool invert = false) {
            handles.Add(new ArcadeEvent(button, PressType.Held, callback, invert));
    }
    
    public void OnReleased(Input.ArcadeButtons button, Action callback, bool invert = false) {
            handles.Add(new ArcadeEvent(button, PressType.Released, callback, invert));
    }
}