using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework.Input;

namespace WGiBeat.Managers
{
    /// <summary>
    /// Maps keys and gamepad buttons to a set of predetermined Actions. Almost all GameScreens use Actions exclusively for input.
    /// Also allows the player to change these bindings. The bindings can be loaded from or saved to file.
    /// </summary>
    public class KeyMappings : Manager
    {
        private Dictionary<Keys, InputAction> _mappings = new Dictionary<Keys, InputAction>();
        private Dictionary<Buttons, InputAction>[] _buttonMappings = new Dictionary<Buttons, InputAction>[4];
        private static String DEFAULT_KEYFILE = "Keys.conf";

        public KeyMappings(LogManager log)
        {
            this.Log = log;
            for (int x =0; x < 4; x++)
            {
                _buttonMappings[x] = new Dictionary<Buttons, InputAction>();
            }
        }
        public void LoadDefault()
        {
            Log.AddMessage("Loading default key configuration.",LogLevel.INFO);
            _mappings.Clear(); //Assumes that loading default means overriding previous settings.
            for (int x = 0; x < 4; x++)
            {
                _buttonMappings[x].Clear(); //Assumes that loading default means overriding previous settings.
            }

            _mappings = new Dictionary<Keys, InputAction>();
            _mappings.Add(Keys.A, new InputAction {Player = 1, Action = "LEFT"});
            _mappings.Add(Keys.W, new InputAction { Player = 1, Action = "UP" });
            _mappings.Add(Keys.S, new InputAction { Player = 1, Action = "DOWN" });
            _mappings.Add(Keys.D, new InputAction { Player = 1, Action = "RIGHT" });
            _mappings.Add(Keys.Space, new InputAction { Player = 1, Action = "BEATLINE" });
            _mappings.Add(Keys.Q, new InputAction { Player = 1, Action = "START" });
            _mappings.Add(Keys.E, new InputAction { Player = 1, Action = "SELECT" });

            _mappings.Add(Keys.Left, new InputAction { Player = 2, Action = "LEFT" });
            _mappings.Add(Keys.Right, new InputAction { Player = 2, Action = "RIGHT" });
            _mappings.Add(Keys.Up, new InputAction { Player = 2, Action = "UP" });
            _mappings.Add(Keys.Down, new InputAction { Player = 2, Action = "DOWN" });
            _mappings.Add(Keys.NumPad0, new InputAction { Player = 2, Action = "BEATLINE" });
            _mappings.Add(Keys.NumPad1, new InputAction { Player = 2, Action = "START" });
            _mappings.Add(Keys.NumPad2, new InputAction { Player = 2, Action = "SELECT" });

            _mappings.Add(Keys.NumPad4, new InputAction { Player = 3, Action = "LEFT" });
            _mappings.Add(Keys.NumPad6, new InputAction { Player = 3, Action = "RIGHT" });
            _mappings.Add(Keys.NumPad8, new InputAction { Player = 3, Action = "UP" });
            _mappings.Add(Keys.NumPad5, new InputAction { Player = 3, Action = "DOWN" });
            _mappings.Add(Keys.Insert, new InputAction { Player = 3, Action = "BEATLINE" });
            _mappings.Add(Keys.PageDown, new InputAction { Player = 3, Action = "START" });
            _mappings.Add(Keys.PageUp, new InputAction { Player = 3, Action = "SELECT" });

            _mappings.Add(Keys.Add, new InputAction{Player = 4, Action = "START"});

            _mappings.Add(Keys.F5, new InputAction { Action = "BPM_DECREASE" });
            _mappings.Add(Keys.F6, new InputAction { Action = "BPM_INCREASE" });
            _mappings.Add(Keys.F7, new InputAction { Action = "OFFSET_DECREASE_BIG" });
            _mappings.Add(Keys.F8, new InputAction { Action = "OFFSET_INCREASE_BIG" });
            _mappings.Add(Keys.F9, new InputAction { Action = "OFFSET_DECREASE_SMALL" });
            _mappings.Add(Keys.F10, new InputAction { Action = "OFFSET_INCREASE_SMALL" });
            _mappings.Add(Keys.F11, new InputAction { Action = "LENGTH_DECREASE" });
            _mappings.Add(Keys.F12, new InputAction { Action = "LENGTH_INCREASE" });
            _mappings.Add(Keys.Escape, new InputAction{Action = "BACK"});

            _buttonMappings[0].Add(Buttons.X, new InputAction{ Player = 4, Action = "LEFT" });
            _buttonMappings[0].Add(Buttons.B, new InputAction { Player = 4, Action = "RIGHT" });
            _buttonMappings[0].Add(Buttons.Y, new InputAction { Player = 4, Action = "UP" });
            _buttonMappings[0].Add(Buttons.A, new InputAction { Player = 4, Action = "DOWN" });
            _buttonMappings[0].Add(Buttons.LeftShoulder, new InputAction { Player = 4, Action = "BEATLINE" });
            _buttonMappings[0].Add(Buttons.RightShoulder, new InputAction { Player = 4, Action = "BEATLINE" });
            _buttonMappings[0].Add(Buttons.Start, new InputAction { Player = 4, Action = "START" });
            _buttonMappings[0].Add(Buttons.Back, new InputAction { Player = 4, Action = "SELECT" });

        }

        public bool LoadFromFile(string filename)
        {
            if (File.Exists(filename))
            {
                FileStream fs = null;
                try
                {
                    Log.AddMessage("Loading saved key configuration from: " + filename, LogLevel.INFO);
                    fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    var bf = new BinaryFormatter();
                    _mappings = (Dictionary<Keys, InputAction>)bf.Deserialize(fs);
                    _buttonMappings = (Dictionary<Buttons, InputAction>[])bf.Deserialize(fs);
                    fs.Close();
                }
                catch (Exception ex)
                {
                    Log.AddMessage("Failed to load saved key configuration: " + ex.Message, LogLevel.WARN);
                    return false;
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }

                return true;
            }

                return false;
        }

        public void SaveToFile()
        {
            SaveToFile(DEFAULT_KEYFILE);
        }

        public void SaveToFile(string filename)
        {
            FileStream fs = null;
            BinaryFormatter bf = null;

            try
            {
                Log.AddMessage("Saving Key Configuration.", LogLevel.DEBUG);
                fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
                bf = new BinaryFormatter();
                bf.Serialize(fs, _mappings);
                bf.Serialize(fs, _buttonMappings);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            
        }

        public Keys GetKey(InputAction inputAction)
        {
            return (from e in _mappings.Keys where _mappings[e].Equals(inputAction) select e).SingleOrDefault();
        }

        public Keys[] GetKeys(InputAction inputAction)
        {
            return (from e in _mappings.Keys where _mappings[e].Equals(inputAction) select e).ToArray();
        }

        public Buttons[] GetButtons(InputAction inputAction, int controllerNumber)
        {
            if (!_buttonMappings[controllerNumber -1].ContainsValue(inputAction))
            {
                return new Buttons[0];
            }
            return
                (from e in _buttonMappings[controllerNumber - 1].Keys
                 where _buttonMappings[controllerNumber - 1][e].Equals(inputAction)
                 select e).ToArray();
        }
        public void SetKey(Keys key, int player, string command)
        {
            if (command == "NONE")
            {
                //Unassigns a key.
                _mappings.Remove(key);
            }
            else if (_mappings.ContainsKey(key))
            {
                //Changes a key already assigned to a different InputAction.
                _mappings[key].Action = command;
                _mappings[key].Player = player;
            }
            else
            {
                //Assigns an InputAction to a free key.
                _mappings.Add(key, new InputAction { Player = player, Action = command });
            }
        }

        public void SetButton(Buttons button, int controllerNumber, int player, string command)
        {
            if ((controllerNumber > 4) || (controllerNumber < 1))
            {
                throw new ArgumentException("Controller number must be between 1 and 4.");
            }
            if (command == "NONE")
            {
                _buttonMappings[controllerNumber - 1].Remove(button);
            }
            else if (_buttonMappings[controllerNumber - 1].ContainsKey(button))
            {
                _buttonMappings[controllerNumber - 1][button].Player = player;
                _buttonMappings[controllerNumber - 1][button].Action = command;
            }
            else
            {
                _buttonMappings[controllerNumber - 1].Add(button, new InputAction{Player = player, Action = command});
            }
        }
        public InputAction GetAction(Keys key)
        {
            if (!_mappings.ContainsKey(key))
            {
                return new InputAction { Action = "", Player = 0 };
            }
            return _mappings[key];
        }

        public InputAction GetAction(Buttons button, int controllerNumber)
        {
            if ((controllerNumber > 4) || (controllerNumber < 1))
            {
                throw new ArgumentException("Controller number must be between 1 and 4.");
            }
            if (!_buttonMappings[controllerNumber-1].ContainsKey(button))
            {
                return new InputAction {Action = "", Player = 0};
            }
            return _buttonMappings[controllerNumber - 1][button];
        }


        public void Unset(Keys key)
        {
            if (_mappings.ContainsKey(key))
            {
                _mappings.Remove(key);
            }
        }

        public void Unset(Buttons button, int number)
        {
            if (_buttonMappings[number - 1].ContainsKey(button))
            {
                _buttonMappings[number - 1].Remove(button);
            }
        }
    }

    /// <summary>
    /// All predetermined Actions that are valid inputs for the game.
    /// </summary>
   
}