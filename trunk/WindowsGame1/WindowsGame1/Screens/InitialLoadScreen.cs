﻿using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class InitialLoadScreen : GameScreen
    {

        private bool _doneLoading = false;
        private Vector2 _textPosition;
        private int _minY, _maxY;
        private Sprite _baseSprite;
        private bool _autoScroll = true;
        public string SongFolderPath { get; set; }

        public InitialLoadScreen(GameCore core) : base(core)
        {
        }

        public override void Initialize()
        {
            SongFolderPath = Directory.GetCurrentDirectory() + "\\" + Core.Settings["SongFolder"];
            _textPosition = new Vector2(Core.Metrics["SongLoadLog", 0].X, Core.Metrics["SongLoadLog", 0].Y);

            _baseSprite = new Sprite {SpriteTexture = TextureManager.Textures["LoadingMessageBase"]};
            _baseSprite.SetPosition(Core.Metrics["LoadMessageBase",0]);
            var thread = new Thread(LoadSongs);
            thread.Start();
            base.Initialize();
        }

        public void LoadSongs()
        {
            Core.Songs.LoadFromFolder(SongFolderPath);
            _doneLoading = true;
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _maxY = 40;
            
            var currentPos = new Vector2(_textPosition.X, _textPosition.Y);
            string[] splitChars = {"\n"};
            var lines = SongManager.GetLogMessages().Split(splitChars,StringSplitOptions.RemoveEmptyEntries);
            _minY = Math.Min(lines.Length * -12 + 560, 40);

            if (_autoScroll)
            {
                 _textPosition.Y = _minY;
            }
            foreach (string line in lines)
            {
                Color drawColor;
                switch (line.Substring(0,4))
                {
                    case "DEBU":
                        drawColor = Color.Cyan;
                        break;
                    case "WARN":
                        drawColor = Color.Yellow;
                        break;
                    case "ERRO":
                        drawColor = Color.Red;
                        break;
                     default:
                        drawColor = Color.White;
                        break;
                }
                TextureManager.DrawString(spriteBatch, line, "LogFont",
                                          currentPos, drawColor, FontAlign.LEFT);
                currentPos.Y += 12;

            }

            _baseSprite.Draw(spriteBatch);
            if (_doneLoading)
            {
                TextureManager.DrawString(spriteBatch,"Loading complete. Press start.","LargeFont",Core.Metrics["LoadMessage",0],Color.White, FontAlign.LEFT);
            }
            else
            {
                TextureManager.DrawString(spriteBatch, "Loading...", "LargeFont", Core.Metrics["LoadMessage", 0], Color.White, FontAlign.LEFT);
            }
        }

        public override void PerformAction(Action action)
        {
            int player;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            switch (paction)
            {
                case "START":
                    if (_doneLoading)
                    {
                        Core.ScreenTransition("MainMenu");
                    }
                    break;
                case "LEFT":
                    _textPosition.X += 54;
                    break;
                case "RIGHT":
                    _textPosition.X -= 54;
                    break;
                case "UP":
                    _textPosition.Y = Math.Min(_maxY, _textPosition.Y + 36);
                    _autoScroll = false;
                    break;
                case "DOWN":
                    _textPosition.Y = Math.Max(_minY, _textPosition.Y - 36);
                    _autoScroll = false;
                    break;
                    
                    
            }
  
        }
    }
}
