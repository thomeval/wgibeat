using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using LogLevel = WGiBeat.Managers.LogLevel;

namespace WGiBeat.Screens
{
    public class OptionScreen : GameScreen
    {
        private readonly Menu _optionsMenu;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private Sprite _background;
        private Sprite _header;
        private Sprite _optionBaseSprite;

        public OptionScreen(GameCore core)
            : base(core)
        {
            _optionsMenu = new Menu { Width = 700, MaxVisibleItems = 12 };
            BuildMenu();
            _optionsMenu.Position = (Core.Metrics["OptionsMenuStart", 0]);
        }

        private void BuildMenu()
        {
            var item = new MenuItem { ItemText = "Song Volume" };
            item.AddOption("0%", "0.00001");

            for (int x = 1; x < 11; x++)
            {
                item.AddOption(x + "0%", "" + x * 0.1);
            }
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Song Debugging" };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Song Previews" };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Song Audio Validation" };
            item.AddOption("Ignore", 0);
            item.AddOption("Warn only", 1);
            item.AddOption("Warn and exclude", 2);
            item.AddOption("Auto Correct", 3);

            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Save Game Log" };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Logging Level" };
            item.AddOption("Errors only", LogLevel.ERROR);
            item.AddOption("Warnings and errors", LogLevel.WARN);
            item.AddOption("Notes or above", LogLevel.NOTE);
            item.AddOption("Info or above", LogLevel.INFO);
            item.AddOption("Debug or above", LogLevel.DEBUG);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Theme" };
            foreach (string dir in System.IO.Directory.GetDirectories(Core.WgibeatRootFolder + "\\Content\\Textures"))
            {
                var dirname = dir.Substring(dir.LastIndexOf("\\") + 1);
                item.AddOption(dirname, dirname);
            }
            _optionsMenu.AddItem(item);

            item = new MenuItem {ItemText = "Allow Problematic Songs"};
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem {ItemText = "Convert Files to .sng"};
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem {ItemText = "Screen Resolution"};
            item.AddOption("800x600", "800x600");
            item.AddOption("1024x768", "1024x768");
            item.AddOption("1200x900","1200x900");
            item.AddOption("1280x1024", "1280x1024");
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Full screen" };
            item.AddOption("Off", false);
            item.AddOption("On", true);
            _optionsMenu.AddItem(item);

            item = new MenuItem { ItemText = "Save" };
            _optionsMenu.AddItem(item);
            item = new MenuItem { ItemText = "Cancel" };
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
            _background = new Sprite
            {
                Height = 600,
                Width = 800,
                SpriteTexture = TextureManager.Textures("AllBackground"),
            };
            _header = new Sprite
            {
                SpriteTexture = TextureManager.Textures("OptionsHeader"),
            };
            _optionBaseSprite = new Sprite
                                    {
                                        SpriteTexture = TextureManager.Textures("OptionDescriptionBase"),
                                        Position = Core.Metrics["OptionsDescriptionBase",0]
                                    };
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            _optionsMenu.Draw(spriteBatch);
            DrawOptionDescription(spriteBatch);
        }

        private void DrawOptionDescription(SpriteBatch spriteBatch)
        {
            _optionBaseSprite.Draw(spriteBatch);
            var optionText = Core.Text["Option" + _optionsMenu.SelectedIndex];
            TextureManager.DrawString(spriteBatch, optionText, "DefaultFont", Core.Metrics["OptionsDescription", 0], Color.White, FontAlign.CENTER);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {

            _background.Draw(spriteBatch);
            _field.Draw(spriteBatch);
            _header.Draw(spriteBatch);

        }

        public override void PerformAction(InputAction inputAction)
        {
            switch (inputAction.Action)
            {
                case "LEFT":
                    _optionsMenu.DecrementOption();
                    break;
                case "RIGHT":
                    _optionsMenu.IncrementOption();
                    break;
                case "UP":
                    _optionsMenu.DecrementSelected();
                    break;
                case "DOWN":
                    _optionsMenu.IncrementSelected();
                    break;
                case "BACK":
                    Core.ScreenTransition("MainMenu");
                    break;

                case "START":
                case "BEATLINE":
                    DoAction();
                    break;
            }
        }

        private void DoAction()
        {
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
                _optionsMenu.GetByItemText("Song Previews").SetSelectedByValue(Core.Settings.Get<object>("SongPreview"));
                _optionsMenu.GetByItemText("Full screen").SetSelectedByValue(Core.Settings.Get<object>("FullScreen"));
                _optionsMenu.GetByItemText("Song Audio Validation").SetSelectedByValue(Core.Settings.Get<object>("SongMD5Behaviour"));
                _optionsMenu.GetByItemText("Save Game Log").SetSelectedByValue(Core.Settings.Get<object>("SaveLog"));
                _optionsMenu.GetByItemText("Logging Level").SetSelectedByValue((LogLevel)(Core.Settings.Get<object>("LogLevel")));
                _optionsMenu.GetByItemText("Theme").SetSelectedByValue(Core.Settings.Get<object>("Theme"));
                _optionsMenu.GetByItemText("Allow Problematic Songs").SetSelectedByValue(Core.Settings.Get<object>("AllowProblematicSongs"));
                _optionsMenu.GetByItemText("Convert Files to .sng").SetSelectedByValue(Core.Settings.Get<object>("ConvertToSNG"));
                _optionsMenu.GetByItemText("Screen Resolution").SetSelectedByValue(Core.Settings.Get<object>("ScreenResolution"));

            }
            catch (Exception ex)
            {
                Core.Log.AddMessage("Failed to load options:" + ex.Message,LogLevel.WARN);
            }

        }
        private void SaveOptions()
        {

            Core.Settings.Set("SongVolume", (Convert.ToDouble(_optionsMenu.GetByItemText("Song Volume").SelectedValue(),CultureInfo.InvariantCulture.NumberFormat)));
            Core.Settings.Set("SongDebug", (_optionsMenu.GetByItemText("Song Debugging").SelectedValue()));
            Core.Settings.Set("SongPreview", (_optionsMenu.GetByItemText("Song Previews").SelectedValue()));
            Core.Settings.Set("FullScreen", _optionsMenu.GetByItemText("Full screen").SelectedValue());
            Core.Settings.Set("SongMD5Behaviour", _optionsMenu.GetByItemText("Song Audio Validation").SelectedValue());
            Core.Settings.Set("SaveLog", _optionsMenu.GetByItemText("Save Game Log").SelectedValue());
            Core.Settings.Set("LogLevel", (int)_optionsMenu.GetByItemText("Logging Level").SelectedValue());
            Core.Settings.Set("Theme", _optionsMenu.GetByItemText("Theme").SelectedValue());
            Core.Settings.Set("AllowProblematicSongs",_optionsMenu.GetByItemText("Allow Problematic Songs").SelectedValue());
            Core.Settings.Set("ConvertToSNG",_optionsMenu.GetByItemText("Convert Files to .sng").SelectedValue());
            Core.Settings.Set("ScreenResolution", _optionsMenu.GetByItemText("Screen Resolution").SelectedValue());
            Core.Audio.SetMasterVolume((float)Core.Settings.Get<double>("SongVolume"));
            Core.Log.LogLevel = (LogLevel)Core.Settings.Get<int>("LogLevel");
            Core.Log.SaveLog = Core.Settings.Get<bool>("SaveLog");

            Core.LoadCurrentTheme();
            Core.SetGraphicsSettings();

            Core.Settings.SaveToFile("settings.txt");

        }
    }
}
