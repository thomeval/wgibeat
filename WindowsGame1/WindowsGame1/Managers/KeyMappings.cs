using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;

namespace WindowsGame1
{
    public class KeyMappings
    {
        private Dictionary<Keys, Action> _mappings = new Dictionary<Keys, Action>();
        private Dictionary<Buttons, Action>[] _buttonMappings = new Dictionary<Buttons, Action>[4];
 
        public KeyMappings()
        {
            for (int x =0; x < 4; x++)
            {
                _buttonMappings[x] = new Dictionary<Buttons, Action>();
            }
        }
        public void LoadDefault()
        {
            _mappings = new Dictionary<Keys, Action>();
            _mappings.Add(Keys.A,Action.P1_LEFT);
            _mappings.Add(Keys.W, Action.P1_UP);
            _mappings.Add(Keys.S, Action.P1_DOWN);
            _mappings.Add(Keys.D, Action.P1_RIGHT);
            _mappings.Add(Keys.Space, Action.P1_BEATLINE);
            _mappings.Add(Keys.Q, Action.P1_START);
            _mappings.Add(Keys.E, Action.P1_SELECT);

            _mappings.Add(Keys.Left, Action.P2_LEFT);
            _mappings.Add(Keys.Right, Action.P2_RIGHT);
            _mappings.Add(Keys.Up, Action.P2_UP);
            _mappings.Add(Keys.Down, Action.P2_DOWN);
            _mappings.Add(Keys.NumPad0, Action.P2_BEATLINE);
            _mappings.Add(Keys.NumPad1, Action.P2_START);
            _mappings.Add(Keys.NumPad2, Action.P2_SELECT);

            _mappings.Add(Keys.NumPad4, Action.P3_LEFT);
            _mappings.Add(Keys.NumPad6, Action.P3_RIGHT);
            _mappings.Add(Keys.NumPad8, Action.P3_UP);
            _mappings.Add(Keys.NumPad5, Action.P3_DOWN);
            _mappings.Add(Keys.Insert, Action.P3_BEATLINE);
            _mappings.Add(Keys.PageDown, Action.P3_START);
            _mappings.Add(Keys.PageUp, Action.P3_SELECT);

            _mappings.Add(Keys.Add, Action.P4_START);

            _mappings.Add(Keys.F5, Action.SYSTEM_BPM_DECREASE);
            _mappings.Add(Keys.F6, Action.SYSTEM_BPM_INCREASE);
            _mappings.Add(Keys.F7, Action.SYSTEM_OFFSET_DECREASE_BIG);
            _mappings.Add(Keys.F8, Action.SYSTEM_OFFSET_INCREASE_BIG);
            _mappings.Add(Keys.F9, Action.SYSTEM_OFFSET_DECREASE_SMALL);
            _mappings.Add(Keys.F10, Action.SYSTEM_OFFSET_INCREASE_SMALL);

            _buttonMappings[0].Add(Buttons.X, Action.P4_LEFT);
            _buttonMappings[0].Add(Buttons.B, Action.P4_RIGHT);
            _buttonMappings[0].Add(Buttons.Y, Action.P4_UP);
            _buttonMappings[0].Add(Buttons.A, Action.P4_DOWN);
            _buttonMappings[0].Add(Buttons.LeftShoulder, Action.P4_BEATLINE);
            _buttonMappings[0].Add(Buttons.RightShoulder, Action.P4_BEATLINE);
            _buttonMappings[0].Add(Buttons.Start, Action.P4_START);
            _buttonMappings[0].Add(Buttons.Back, Action.P4_SELECT);

            _mappings.Add(Keys.Escape, Action.P1_ESCAPE);
        }

        public Boolean LoadFromFile(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
                    var bf = new BinaryFormatter();
                    _mappings = (Dictionary<Keys, Action>)bf.Deserialize(fs);
                    _buttonMappings = (Dictionary<Buttons, Action>[])bf.Deserialize(fs);
                    fs.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading Keys.conf file");
                    return false;
                }

                return true;
            }
            else
                return false;
        }

        public void SaveToFile(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            var bf = new BinaryFormatter();
            bf.Serialize(fs, _mappings);
            bf.Serialize(fs,_buttonMappings);
            fs.Close();
        }

        public Keys GetKey(Action action)
        {
            return (from e in _mappings.Keys where _mappings[e] == action select e).SingleOrDefault();
        }

        public void SetKey(Keys key, Action action)
        {
            if (action == Action.NONE)
            {
                _mappings.Remove(key);
            }
            else if (_mappings.ContainsKey(key))
            {
                _mappings[key] = action;
            }
            else
            {
                _mappings.Add(key, action); //One key maps to many actions? Not handled in GameCore or GetAction(Keys key).
            }
        }

        public void SetButton(Buttons button, int controllerNumber, Action action)
        {
            if ((controllerNumber > 4) || (controllerNumber < 1))
            {
                throw new ArgumentException("Controller number must be between 1 and 4.");
            }
            if (action == Action.NONE)
            {
                _buttonMappings[controllerNumber-1].Remove(button);
            }
            else if (_buttonMappings[controllerNumber-1].ContainsKey(button))
            {
                _buttonMappings[controllerNumber - 1][button] = action;
            }
            else
            {
                _buttonMappings[controllerNumber - 1].Add(button, action);
            }
        }
        public Action GetAction(Keys key)
        {
            if (!_mappings.ContainsKey(key))
            {
                return Action.NONE;
            }
            return _mappings[key];
        }

        public Action GetAction(Buttons button, int controllerNumber)
        {
            if ((controllerNumber > 4) || (controllerNumber < 1))
            {
                throw new ArgumentException("Controller number must be between 1 and 4.");
            }
            if (!_buttonMappings[controllerNumber-1].ContainsKey(button))
            {
                return Action.NONE;
            }
            return _buttonMappings[controllerNumber - 1][button];
        }
    }

    public enum Action
    {
        P1_LEFT,
        P1_RIGHT,
        P1_UP,
        P1_DOWN,
        P1_BEATLINE,
        P1_START,
        P1_SELECT,
        P1_ESCAPE,

        
        P2_LEFT,
        P2_RIGHT,
        P2_UP,
        P2_DOWN,
        P2_BEATLINE,
        P2_START,
        P2_SELECT,

        P3_LEFT,
        P3_RIGHT,
        P3_UP,
        P3_DOWN,
        P3_BEATLINE,
        P3_START,
        P3_SELECT,

        P4_LEFT,
        P4_RIGHT,
        P4_UP,
        P4_DOWN,
        P4_BEATLINE,
        P4_START,
        P4_SELECT,

        SYSTEM_BPM_INCREASE,
        SYSTEM_BPM_DECREASE,
        SYSTEM_OFFSET_INCREASE_BIG,
        SYSTEM_OFFSET_INCREASE_SMALL,
        SYSTEM_OFFSET_DECREASE_BIG,
        SYSTEM_OFFSET_DECREASE_SMALL,

        NONE
        
    }
}
