# Devcade-library
A monogame library for allowing games to interact with cabinet functions.

---

- [Input](#input-wrapping)
  - [ArcadeButtons enum](#arcadebuttons-enum)
  - [Get methods](#get-methods)
  
---
---

## Input wrapping
### ArcadeButtons enum
`Input.ArcadeButtons` is an enum with values 
- A1 through A4
- B1 through B4
- Menu
- StickUp, StickDown, StickRight, and StickLeft

These values are equivelant to values of the `Buttons` enum and can be used in place of them when explicitly cast to a `Buttons`. This allows existing controller input code to be easily adapted to the Devcade control scheme.

Example:
`gamePadState.IsButtonDown((Buttons)Devcade.Input.ArcadeButtons.Menu)`

---
### Get methods

In order to use these methods `Input.Initialize()` must be called once before using them and `Input.Update()` must be called once each frame.

#### `GetButton(int playerNum, ArcadeButtons button)`

Given the player and button to check, it will return true if the button is down. This will return true on the initial press and for the duration that the button is held.

#### `GetButtonDown(int playerNum, ArcadeButtons button)`

Given the player and button to check, it will return true if the button is pressed down during the current frame. This only returns true on the initial press from up to down and will not trigger repeatedly while the button is held.

#### `GetButtonUp(int playerNum, ArcadeButtons button)`

Given the player and button to check, it will return true if the button is released during the current frame. This only returns true on the initial release from down to up and will not trigger repeatedly while the button is up.

#### `GetButtonHeld(int playerNum, ArcadeButtons button)`

Given the player and button to check, it will return true if the button is being held down. This will not return true for the initial press or the release.

#### `GetStick(int playerNum)`

Given the player it returns a `Vector2` representing the stick direction.

---
---