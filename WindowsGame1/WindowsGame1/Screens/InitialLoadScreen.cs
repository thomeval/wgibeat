using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using WGiBeat.Drawing;
using WGiBeat.Managers;

namespace WGiBeat.Screens
{
    public class InitialLoadScreen : GameScreen
    {

        private bool _doneLoading;
        private Vector2 _textPosition;
        private int _minY, _maxY;
        private Sprite _baseSprite;
        private bool _autoScroll = true;

        private string _songFolderPath;

        public InitialLoadScreen(GameCore core) : base(core)
        {
        }

        public override void Initialize()
        {
            _songFolderPath = "" + Core.Settings["SongFolder"];
            _textPosition = Core.Metrics["SongLoadLog", 0].Clone();

            _baseSprite = new Sprite
                              {
                                  SpriteTexture = TextureManager.Textures("LoadingMessageBase"),
                                  Position = (Core.Metrics["LoadMessageBase", 0])
                              };
            var thread = new Thread(LoadSongs);
            thread.Start();
            base.Initialize();
        }

        public void LoadSongs()
        {
            string[] paths = _songFolderPath.Split('|');

            foreach (string path in paths)
            {
                if (!Path.IsPathRooted(path))
                {
                   Core.Songs.LoadFromFolder(Core.WgibeatRootFolder + "\\" + path);
                }
                else
                {
                    Core.Songs.LoadFromFolder(path);
                }
               
            }
            _doneLoading = true;
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _maxY = 40;
            int errorCount = 0, warnCount = 0;
            var currentPos = new Vector2(_textPosition.X, _textPosition.Y);
            var entries = GetOrReuseLogMessages();
            _minY = Math.Min(entries.Length * -12 + 555, 40);

            if (_autoScroll)
            {
                 _textPosition.Y = _minY;
            }
            foreach (LogEntry entry in entries)
            {
                Color drawColor;
                switch (entry.Level)
                {
                    case LogLevel.DEBUG:
                        drawColor = Color.Gray;
                        break;
                    case LogLevel.NOTE:
                        drawColor = Color.Cyan;
                        break;
                    case LogLevel.WARN:
                        drawColor = Color.Yellow;
                        warnCount++;
                        break;
                    case LogLevel.ERROR:
                        drawColor = Color.Red;
                        errorCount++;
                        break;
                     default:
                        drawColor = Color.White;
                        break;
                }
                TextureManager.DrawString(spriteBatch, entry.ToString(), "LogFont",
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
            TextureManager.DrawString(spriteBatch,String.Format("{0} errors, {1} warnings",errorCount,warnCount),"DefaultFont",Core.Metrics["LoadErrorCount",0],Color.White,FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch,"" + GameCore.VERSION_STRING, "DefaultFont", Core.Metrics["LoadVersion", 0], Color.White, FontAlign.LEFT);

        }

        private int _lastMessageCount = -1;
        private LogEntry[] _lastMessages = new LogEntry[1];
        private LogEntry[] GetOrReuseLogMessages()
        {
            if (_lastMessageCount != Core.Log.MessageCount())
            {
                _lastMessageCount = Core.Log.MessageCount();
                _lastMessages = Core.Log.GetMessages();
            }
            return _lastMessages;
        }

        public override void PerformAction(InputAction inputAction)
        {

            switch (inputAction.Action)
            {
                case "START":
                    if (_doneLoading)
                    {
                        Core.ScreenTransition("MainMenu");
                    }
                    break;
                case "LEFT":
                    _textPosition.X = Math.Min(Core.Metrics["SongLoadLog",0].X,_textPosition.X + 54);
                    break;
                case "RIGHT":
                    _textPosition.X = Math.Max(-3000, _textPosition.X - 54);
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
