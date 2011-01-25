using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Action = WGiBeat.Managers.Action;
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
            _optionsMenu = new Menu { Width = 700 };
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

            item = new MenuItem { ItemText = "Full screen" };
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
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["AllBackground"],
                Width = Core.Window.ClientBounds.Width,
            };
            _header = new Sprite
            {
                SpriteTexture = TextureManager.Textures["OptionsHeader"],
            };
            _optionBaseSprite = new Sprite
                                    {
                                        SpriteTexture = TextureManager.Textures["OptionDescriptionBase"],
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

        public override void PerformAction(Action action)
        {
            switch (action)
            {
                case Action.P1_LEFT:
                case Action.P2_LEFT:
                case Action.P3_LEFT:
                case Action.P4_LEFT:
                    _optionsMenu.DecrementOption();
                    break;
                case Action.P1_RIGHT:
                case Action.P2_RIGHT:
                case Action.P3_RIGHT:
                case Action.P4_RIGHT:
                    _optionsMenu.IncrementOption();
                    break;
                case Action.P1_UP:
                case Action.P2_UP:
                case Action.P3_UP:
                case Action.P4_UP:
                    _optionsMenu.DecrementSelected();
                    break;
                case Action.P1_DOWN:
                case Action.P2_DOWN:
                case Action.P3_DOWN:
                case Action.P4_DOWN:
                    _optionsMenu.IncrementSelected();
                    break;
                case (Action.SYSTEM_BACK):
                    Core.ScreenTransition("MainMenu");
                    break;

                case Action.P1_START:
                case Action.P2_START:
                case Action.P3_START:
                case Action.P4_START:
                case Action.P1_BEATLINE:
                case Action.P2_BEATLINE:
                case Action.P3_BEATLINE:
                case Action.P4_BEATLINE:
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load options:" + ex.Message);
            }

        }
        private void SaveOptions()
        {

            Core.Settings.Set("SongVolume", (Convert.ToDouble(_optionsMenu.GetByItemText("Song Volume").SelectedValue())));
            Core.Settings.Set("SongDebug", (_optionsMenu.GetByItemText("Song Debugging").SelectedValue()));
            Core.Settings.Set("SongPreview", (_optionsMenu.GetByItemText("Song Previews").SelectedValue()));
            Core.Settings.Set("FullScreen", _optionsMenu.GetByItemText("Full screen").SelectedValue());
            Core.Settings.Set("SongMD5Behaviour", _optionsMenu.GetByItemText("Song Audio Validation").SelectedValue());
            Core.Settings.Set("SaveLog", _optionsMenu.GetByItemText("Save Game Log").SelectedValue());
            Core.Settings.Set("LogLevel", (int)_optionsMenu.GetByItemText("Logging Level").SelectedValue());
            Core.Settings.Set("Theme", _optionsMenu.GetByItemText("Theme").SelectedValue());
            Core.Audio.SetMasterVolume((float)Core.Settings.Get<double>("SongVolume"));
            Core.Log.LogLevel = (LogLevel)Core.Settings.Get<int>("LogLevel");
            Core.Log.SaveLog = Core.Settings.Get<bool>("SaveLog");

            Core.LoadCurrentTheme();

            Core.GraphicsManager.IsFullScreen = Core.Settings.Get<bool>("FullScreen");
            Core.GraphicsManager.ApplyChanges();
            Core.Settings.SaveToFile("settings.txt");

        }
    }
}
