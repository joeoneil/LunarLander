# MonoGame-Events

This is a repo containing classes which will manage key, button, and mouse events for you! No more bullshit if statements and manual rising edge detection on your buttons and keys.
All the classes are designed with the callback event handling design pattern in mind. If you don't know what that is, don't worry, you don't need to, it just works.

## KeyboardManager

The KeyboardManager manages keypresses and calls functions on the rising and falling edge of keypresses or while they're held, here's how:

define a Keyboard Manager
```cs
private KeyboardManager keyManager = new KeyboardManager();
```

In your initialize method, define which key should call which function, and provide a callback function with what code should execute whenever the event happens

```cs
protected override void Initialize() 
{
	keyManager.OnPressed(Keys.A, () => {
		Console.WriteLine("You pressed the A key");
	});

	keyManager.OnPressed(Keys.S, foo);

	keyManager.OnHeld(Keys.D, () => {
		Console.Write("D");
	})

	keyManager.OnReleased(Keys.F, () => {
		Console.Write("F is no longer being held");
	})
}

private void foo() 
{
	Console.WriteLine("You pressed the S key");
}
```
The callback function can be an anonymous function defined locally, or a defined function within Game.cs

You will also need to update the state of the keyManager every frame with the new state of the keyboard within your update function

```cs
protected override void Update(GameTime gameTime) {
	keyManager.Update(Keyboard.GetState());

	// ... some other stuff
}
```

## GamepadManager

The GamepadManager functions almost identically to the KeyboardManager, but with gamepad buttons instead of keys. The only difference you'll notice in using it is that you'll use 'Buttons.{button}' instead of 'Keys.{key}' for referring to your button.

```cs
protected override void Initialize() {
	gamepadManager.OnPressed(Buttons.DPadUp () => {
		Console.Writeline("Pressed DPad up");	
	});
}
```

In addition, you'll need to specify which player you're referring to when getting the gamepad's state

```cs
protected override void Update(GameTime gameTime) {
	gamepadManager1.Update(GamePad.GetState(PlayerIndex.one));
	gamepadManager2.Update(GamePad.GetState(PlayerIndex.two));
}
```
This will mean that you'll need multiple GamepadManagers if you require multiple players.

## ButtonManager

The Button manager manages physical buttons on the screen. The Button class contains only a rectangle and a callback, and will need to be created in Initialize and added to a ButtonManager with ButtonManager.Add(button). As ButtonManager is much simpler, it is a static class so it doesn't need to be constructed. Like the other two event handlers you'll need to pass in the Mouse's state every frame for it to handle events. Don't use this class though, as arcade cabinets typically do not have mouse pointers.
