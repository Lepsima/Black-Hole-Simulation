using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace BlackHoles;

public class InputHandler {
	public static InputHandler Instance { get; private set; }

	public InputHandler() {
		Instance = this;
	}
	
	private class KeyHandler(Keys key) {
		public Action<bool> OnKeyEvent;
		private bool wasPressed;

		public void Update(ref KeyboardState keyboard) {
			bool isPressed = keyboard.IsKeyDown(key);

			if (isPressed != wasPressed) {
				OnKeyEvent?.Invoke(isPressed);
			}
			
			wasPressed = isPressed;
		}
	}
	
	private readonly Dictionary<Keys, KeyHandler> keyHandlers = new();

	public ref Action<bool> GetAction(Keys key) {
		if (!keyHandlers.ContainsKey(key)) AddKey(key);
		return ref keyHandlers[key].OnKeyEvent;
	}

	private void AddKey(Keys key) {
		keyHandlers.Add(key, new KeyHandler(key));
	}
	
	public void Update() {
		KeyboardState keyboard = Keyboard.GetState();

		foreach (KeyValuePair<Keys, KeyHandler> handlerEntry in keyHandlers) {
			handlerEntry.Value.Update(ref keyboard);
		}
	}
}