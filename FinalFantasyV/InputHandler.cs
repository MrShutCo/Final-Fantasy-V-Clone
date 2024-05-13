using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV
{
    public class InputHandler
    {

        Dictionary<Keys, Action> KeyMappings = new Dictionary<Keys, Action>();

        static KeyboardState keyboardState;
        static KeyboardState previousKeyboardState;

        public static KeyboardState GetState()
        {
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            return keyboardState;
        }

        public static bool KeyPressed(Keys key)
        {
            return (keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key));
        }

        public static bool KeyReleased(Keys key)
        {
            return (keyboardState.IsKeyUp(key) && previousKeyboardState.IsKeyDown(key));
        }

        public void Reset()
        {
            keyboardState = new KeyboardState();
            previousKeyboardState = new KeyboardState();
        }

        

        public void RegisterKey(Keys key, Action action)
        {
            KeyMappings[key] = action;
        }

        //This will be filled with the proper Key events and actions for each state
        public void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<Keys, Action> mapping in KeyMappings)
            {
                if (KeyPressed(mapping.Key))
                    mapping.Value();
            }
            
        }

    }
}

