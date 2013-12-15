using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using LogLevel = WGiBeat.Managers.LogLevel;

namespace WGiBeat.Screens
{
    public class OptionScreen : GameScreen
    {
        private readonly Menu _optionsMenu;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private Sprite3D _background;
        private Sprite3D _header;
        private Sprite3D _optionBaseSprite;

        public OptionScreen(GameCore core)
            : base(core)
        {
            _optionsMenu = new Menu { Width = Core.Metrics["OptionsMenuStart.Size", 0].X, MaxVisibleItems = (int) Core.Metrics["OptionsMenuStart.Size",0].Y };
            BuildMenu();
            _optionsMenu.Position = (Core.Metrics["OptionsMenuStart", 0]);
        }

        private void BuildMenu()
        {
            var item = new MenuItem { ItemText = "Song Volume", IsSelectable = false};
            item.AddOption("0%", "" + 0.0001);

            for (int x = 1; x < 11; x++)
            {
                item.AddOption(x + "0%", "" + x * 0.1);
            }
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Song Debugging", IsSelectable = false };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);


            item = new MenuItem { ItemText = "Song Audio Validation", IsSelectable = false };
            item.AddOption("Ignore", 0);
            item.AddOption("Warn only", 1);
            item.AddOption("Warn and exclude", 2);
            item.AddOption("Auto Correct", 3);

            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Save Game Log", IsSelectable = false };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Logging Level", IsSelectable = false };
            item.AddOption("Errors only", LogLevel.ERROR);
            item.AddOption("Warnings and errors", LogLevel.WARN);
            item.AddOption("Notes or above", LogLevel.NOTE);
            item.AddOption("Info or above", LogLevel.INFO);
            item.AddOption("Debug or above", LogLevel.DEBUG);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Theme", IsSelectable = false };
            foreach (string dir in System.IO.Directory.GetDirectories(Core.WgibeatRootFolder + "\\Content\\Textures"))
            {
                var dirname = dir.Substring(dir.LastIndexOf("\\") + 1);
                item.AddOption(dirname, dirname);
            }
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Allow Problematic Songs", IsSelectable = false };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Convert Files to .sng", IsSelectable = false };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Screen Resolution", IsSelectable = false };
            item.AddOption("640x480 (4:3)", "640x480");
            item.AddOption("800x600 (4:3)", "800x600");
            item.AddOption("1024x768 (4:3)", "1024x768");
            item.AddOption("1280x1024 (4:3)", "1280x1024");

            item.AddOption("1280x720 (16:9)", "1280x720");
            item.AddOption("1600x900 (16:9)", "1600x900");
            item.AddOption("1920x1080 (16:9)", "1920x1080");
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Full screen", IsSelectable = false };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "V-Sync", IsSelectable = false };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Check For Updates", IsSelectable = false };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Background Animation", IsSelectable = false };
            item.AddOption("Off", 0);
            item.AddOption("Normal", 128);
            item.AddOption("ARGH! MY EYES!", 255);

            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Blazing Bass Boost", IsSelectable = false };
            item.AddOption("Off", 1);
            item.AddOption("Light", 1.25);
            item.AddOption("Mild", 1.5);
            item.AddOption("Max", 2);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Save" };
            _optionsMenu.AddItem(item);
            item = new MenuItem { ItemText = "Cancel", IsCancel = true };
            _optionsMenu.AddItem(item);
        }

        public override void Initialize()
        {
            LoadOptions();
            InitSprites();
            base.Initialize();
        }

        private void InitSprites()
        {
            _background = new Sprite3D
            {
                Size = Core.Metrics["ScreenBackground.Size", 0],
                Position = Core.Metrics["ScreenBackground", 0],
                Texture = TextureManager.Textures("AllBackground"),
            };
            _header = new Sprite3D
            {
                Texture = TextureManager.Textures("OptionsHeader"),
                Position = Core.Metrics["ScreenHeader", 0],
                Size = Core.Metrics["ScreenHeader.Size", 0]
            };
            _optionBaseSprite = new Sprite3D
                                    {
                                        Texture = TextureManager.Textures("OptionDescriptionBase"),
                                        Position = Core.Metrics["OptionsDescriptionBase",0],
                                        Size = Core.Metrics["OptionsDescriptionBase.Size",0]
                                    };
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(gameTime);
            _optionsMenu.Draw();
            DrawOptionDescription();
        }

        private void DrawOptionDescription()
        {
            _optionBaseSprite.Draw();
            var optionText = Core.Text["Option" + _optionsMenu.SelectedIndex];
            FontManager.DrawString(optionText, "DefaultFont", Core.Metrics["OptionsDescription", 0], Color.White, FontAlign.Center);
        }

        private void DrawBackground(GameTime gameTime)
        {
            _background.Draw();
            _field.Draw(gameTime);
            _header.Draw();

        }

        public override void PerformAction(InputAction inputAction)
        {
            switch (inputAction.Action)
            {
                case "LEFT":
                case "RIGHT":
                case "UP":
                case "DOWN":
                    _optionsMenu.HandleAction(inputAction);
                    break;
                case "BACK":
                    Core.ScreenTransition("MainMenu");
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;

                case "START":
                case "BEATLINE":
                    DoAction(inputAction);
                    break;
            }
        }

        private void DoAction(InputAction action)
        {
            _optionsMenu.HandleAction(action);
            switch (_optionsMenu.SelectedItem().ItemText)
            {
                case "Save":
                    SaveOptions();
                    Core.ScreenTransition("MainMenu");
                    break;
                case "Cancel":
                    Core.ScreenTransition("MainMenu");

                    break;
            }
        }

        private void LoadOptions()
        {
            try
            {
                _optionsMenu.GetByItemText("Song Volume").SetSelectedByValue("" + Core.Settings.Get<object>("SongVolume"));
                _optionsMenu.GetByItemText("Song Debugging").SetSelectedByValue(Core.Settings.Get<object>("SongDebug"));
                _optionsMenu.GetByItemText("Full screen").SetSelectedByValue(Core.Settings.Get<object>("FullScreen"));
                _optionsMenu.GetByItemText("V-Sync").SetSelectedByValue(Core.Settings.Get<object>("VSync"));
                _optionsMenu.GetByItemText("Song Audio Validation").SetSelectedByValue(Core.Settings.Get<object>("SongMD5Behaviour"));
                _optionsMenu.GetByItemText("Save Game Log").SetSelectedByValue(Core.Settings.Get<object>("SaveLog"));
                _optionsMenu.GetByItemText("Logging Level").SetSelectedByValue((LogLevel)(Core.Settings.Get<object>("LogLevel")));
                _optionsMenu.GetByItemText("Theme").SetSelectedByValue(Core.Settings.Get<object>("Theme"));
                _optionsMenu.GetByItemText("Allow Problematic Songs").SetSelectedByValue(Core.Settings.Get<object>("AllowProblematicSongs"));
                _optionsMenu.GetByItemText("Convert Files to .sng").SetSelectedByValue(Core.Settings.Get<object>("ConvertToSNG"));
                _optionsMenu.GetByItemText("Screen Resolution").SetSelectedByValue(Core.Settings.Get<object>("ScreenResolution"));
                _optionsMenu.GetByItemText("Check For Updates").SetSelectedByValue(Core.Settings.Get<object>("CheckForUpdates"));
                _optionsMenu.GetByItemText("Blazing Bass Boost").SetSelectedByValue(Core.Settings.Get<object>("BlazingBassBoost"));
                _optionsMenu.GetByItemText("Background Animation").SetSelectedByValue(Core.Settings.Get<object>("BackgroundAnimation"));
            }
            catch (Exception ex)
            {
                Core.Log.AddMessage("Failed to load options:" + ex.Message,LogLevel.WARN);
            }

        }
        private void SaveOptions()
        {

            Core.Settings.Set("SongVolume", (Convert.ToDouble(_optionsMenu.GetByItemText("Song Volume").SelectedValue())));
            Core.Settings.Set("SongDebug", (_optionsMenu.GetByItemText("Song Debugging").SelectedValue()));
            Core.Settings.Set("FullScreen", _optionsMenu.GetByItemText("Full screen").SelectedValue());
            Core.Settings.Set("SongMD5Behaviour", _optionsMenu.GetByItemText("Song Audio Validation").SelectedValue());
            Core.Settings.Set("SaveLog", _optionsMenu.GetByItemText("Save Game Log").SelectedValue());
            Core.Settings.Set("LogLevel", (int)_optionsMenu.GetByItemText("Logging Level").SelectedValue());
            Core.Settings.Set("Theme", _optionsMenu.GetByItemText("Theme").SelectedValue());
            Core.Settings.Set("AllowProblematicSongs",_optionsMenu.GetByItemText("Allow Problematic Songs").SelectedValue());
            Core.Settings.Set("ConvertToSNG",_optionsMenu.GetByItemText("Convert Files to .sng").SelectedValue());
            Core.Settings.Set("ScreenResolution", _optionsMenu.GetByItemText("Screen Resolution").SelectedValue());
            Core.Settings.Set("CheckForUpdates", _optionsMenu.GetByItemText("Check For Updates").SelectedValue());
            Core.Settings.Set("VSync", _optionsMenu.GetByItemText("V-Sync").SelectedValue());
            Core.Settings.Set("BlazingBassBoost", _optionsMenu.GetByItemText("Blazing Bass Boost").SelectedValue());
            Core.Settings.Set("BackgroundAnimation", _optionsMenu.GetByItemText("Background Animation").SelectedValue());
            Core.Audio.SetMasterVolume((float)Core.Settings.Get<double>("SongVolume"));
            Core.Log.LogLevel = (LogLevel)Core.Settings.Get<int>("LogLevel");
            Core.Log.SaveLog = Core.Settings.Get<bool>("SaveLog");

            Core.LoadCurrentTheme();
            Core.SetGraphicsSettings();

            Core.Settings.SaveToFile("settings.txt");

        }
    }
}
