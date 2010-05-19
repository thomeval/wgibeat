﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;

namespace WGiBeat.Screens
{
    public class OptionScreen : GameScreen
    {
        private readonly Menu _optionsMenu;

        public OptionScreen(GameCore core) : base(core)
        {
            _optionsMenu = new Menu();
            BuildMenu();
            _optionsMenu.SetPosition(Core.Metrics["OptionsMenuStart",0]);
        }

        private void BuildMenu()
        {
            var item = new MenuItem {ItemText = "Song Volume"};
            for (int x = 0; x < 11; x++)
            {
                item.AddOption(x + "0%", x * 0.1);
            }
            _optionsMenu.AddItem(item);

            item = new MenuItem() {ItemText = "Song Debugging"};
            item.AddOption("Off", false);
            item.AddOption("On",true);
            _optionsMenu.AddItem(item);

            item = new MenuItem() {ItemText = "Save"};
            _optionsMenu.AddItem(item);
            item = new MenuItem() {ItemText = "Cancel"};
            _optionsMenu.AddItem(item);
        }

        public override void Initialize()
        {
            LoadOptions();
            base.Initialize();
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            var background = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["allBackground"],
                Width = Core.Window.ClientBounds.Width,
                X = 0,
                Y = 0
            };

            background.Draw(spriteBatch);
            _optionsMenu.Draw(spriteBatch);
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
            _optionsMenu.GetByItemText("Song Volume").SetSelectedByValue(Core.Settings.Get<object>("SongVolume"));
            _optionsMenu.GetByItemText("Song Debugging").SetSelectedByValue(Core.Settings.Get<object>("SongDebug"));
        }
        private void SaveOptions()
        {
            Core.Settings.Set("SongVolume",(_optionsMenu.GetByItemText("Song Volume").SelectedValue()));
            Core.Settings.Set("SongDebug", (_optionsMenu.GetByItemText("Song Debugging").SelectedValue()));

            Core.Settings.SaveToFile("settings.txt");
        }
    }
}