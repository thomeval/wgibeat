using System.Collections.Generic;
using System.IO;
using System.Linq;
using WGiBeat.Managers;

namespace WGiBeat.AudioSystem
{
    public class SoundEffectManager : Manager
    {
        public AudioManager AudioManager { get; set; }
        public SettingsManager SettingsManager { get; set; }
        public readonly Dictionary<string, string> Sounds;
       
        public SoundEffectManager(LogManager log, AudioManager audioManager, SettingsManager settings)
        {
            Log = log;
            AudioManager = audioManager;
            SettingsManager = settings;
            Sounds = new Dictionary<string, string>();
            Log.AddMessage("Initializing Sound Effect Manager...", LogLevel.INFO);
        }

        public void LoadFromFolder(string foldername)
        {
            if (!Directory.Exists(foldername))
            {
                Log.AddMessage("SoundEffectManager: Folder doesn't exist: " + foldername, LogLevel.WARN);
                return;
            }

            if (!File.Exists(foldername + "\\MenuSounds.txt"))
            {
                Log.AddMessage("SoundEffectManager: Cannot find definition file: " + foldername + "\\MenuSounds.txt", LogLevel.WARN);
                return;
            }

            var filedata = File.ReadAllText(foldername + "\\MenuSounds.txt").Replace("\r", "").Replace("\n", "");
            var lines = filedata.Split(';');
            if (!lines[0].StartsWith("#MENUSOUNDS-1.0"))
            {
                Log.AddMessage("SoundEffectManager: Invalid definition file: " + foldername + "\\MenuSounds.txt", LogLevel.WARN);
            }

            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Count() != 2)
                {
                    continue;
                }

                var key = parts[0].ToUpper();
                var value = foldername + "\\" + parts[1];

                Sounds[key] = value;
            }
        }

        public void PlaySoundEffect(SoundEvent sevent)
        {
            if (!SettingsManager.Get<bool>("EnableMenuSounds"))
            {
                return;
            }

            var eventName = sevent.ToString().Replace("_", "");
            if (!Sounds.ContainsKey(eventName))
            {
                //This should happen if no corresponding definition is found for the event (such as a missing definition file).
                Log.AddMessage("SoundEffectManager doesn't contain an entry for: " + eventName,LogLevel.WARN);
                Sounds[eventName] = "";
                return;
            }
            if (string.IsNullOrEmpty(Sounds[eventName]))
            {
                return;
            }
            if (!File.Exists(Sounds[eventName]))
            {
                Log.AddMessage("SoundEffectManager: File specified doesn't exist: " + Sounds[eventName], LogLevel.WARN);
                return;
            }

            AudioManager.PlaySoundEffect(Sounds[eventName]);
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
        PLAYER_OPTIONS_CHANGE,
        SONG_DECIDE,
        SONG_SELECT_UP,
        SONG_SELECT_DOWN,
        SONG_SORT_DISPLAY,
        SONG_SORT_HIDE,
        SONG_SORT_CHANGE,

        TEAM_CHANGE,
        TEAM_DECIDE,
        KEY_CHANGE_COMPLETE,
        KEY_CHANGE_START
    }
}
