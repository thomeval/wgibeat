using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WGiBeat.Managers;

namespace WGiBeat.AudioSystem
{
    public class SoundEffectManager : Manager
    {
        public AudioManager AudioManager { get; set; }
        public SettingsManager SettingsManager { get; set; }
        public Dictionary<string, string> Sounds;

        public SoundEffectManager(LogManager log, AudioManager audioManager, SettingsManager settings)
        {
            Log = log;
            AudioManager = audioManager;
            SettingsManager = settings;
            Sounds = new Dictionary<string, string>();
            Log.AddMessage("Initializing Song Manager...", LogLevel.INFO);
        }

        public void LoadFromFile(string filename)
        {
            //TODO: Define file format.
        }
    }

    public enum SoundEvent
    {
        MAIN_MENU_DECIDE,
        MAIN_MENU_BACK,
        MAIN_MENU_SELECT_UP,
        MAIN_MENU_SELECT_DOWN,
        MENU_DECIDE,
        MENU_BACK,
        MENU_SELECT_UP,
        MENU_SELECT_DOWN,
        MENU_OPTION_SELECT_LEFT,
        MENU_OPTION_SELECT_RIGHT,
        PLAYER_OPTIONS_DISPLAY,
        PLAYER_OPTIONS_HIDE,
        PLAYER_OPTIONS_UP,
        PLAYER_OPTIONS_DOWN,
        SONG_DECIDE,
        SONG_SELECT_UP,
        SONG_SELECT_DOWN,
        SONG_SORT_SELECT_UP,
        SONG_SORT_SELECT_DOWN,
        SONG_SORT_SELECT_LEFT,
        SONG_SORT_SELECT_RIGHT,
        SONG_SORT_DISPLAY,
        SONG_SORT_HIDE
    }
}
