using System;
using System.IO;
using System.Linq;
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
        private Sprite3D _baseSprite;
        private bool _autoScroll = true;

        private string _songFolderPath;

        public InitialLoadScreen(GameCore core) : base(core)
        {
        }

        public override void Initialize()
        {
            _songFolderPath = "" + Core.Settings["SongFolder"];
            _textPosition = Core.Metrics["SongLoadLog", 0].Clone();

            _baseSprite = new Sprite3D
                              {
                                  Texture = TextureManager.Textures("LoadingMessageBase"),
                                  Position = (Core.Metrics["LoadMessageBase", 0]),
                                  Size = Core.Metrics["LoadMessageBase.Size",0]
                              };
            var thread = new Thread(LoadSongs);
            thread.Start();
            base.Initialize();
        }

        public void LoadSongs()
        {
            string[] paths = _songFolderPath.Split('|');

            for (int x = 0; x < paths.Length; x++)
            {
                string path = paths[x];
                if (!Path.IsPathRooted(path))
                {
                    paths[x] = Core.WgibeatRootFolder + "\\" + path;

                }

            }
            Core.Songs.LoadFromFolder(paths);
            _doneLoading = true;
        }

        private const int MAX_VISIBLE_ENTRIES = 100;
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _maxY = 40;
            
            var currentPos = new Vector2(_textPosition.X, _textPosition.Y);
            var entries = GetOrReuseLogMessages();

            var startEntry = Math.Max(0, entries.Length - MAX_VISIBLE_ENTRIES);
            _minY = Math.Min((entries.Length - startEntry) * -12 + GameCore.INTERNAL_HEIGHT - 45, 40);

            if (_autoScroll)
            {
                 _textPosition.Y = _minY;
            }
            for (int i = startEntry; i < entries.Length; i++)
            {
                LogEntry entry = entries[i];
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
                        break;
                    case LogLevel.ERROR:
                        drawColor = Color.Red;
                        break;
                    default:
                        drawColor = Color.White;
                        break;
                }
                FontManager.DrawString(entry.ToString(), "LogFont",
                                          currentPos, drawColor, FontAlign.Left);
                currentPos.Y += 12;
            }

            _baseSprite.Draw();
            if (_doneLoading)
            {
                FontManager.DrawString("Loading complete. Press start.","LargeFont",Core.Metrics["LoadMessage",0],Color.White, FontAlign.Left);
            }
            else
            {
                FontManager.DrawString("Loading...", "LargeFont", Core.Metrics["LoadMessage", 0], Color.White, FontAlign.Left);
            }
            var errorCount = entries.Count(e => e.Level == LogLevel.ERROR);
            var warnCount = entries.Count(e => e.Level == LogLevel.WARN);
            FontManager.DrawString(String.Format("{0} songs, {1} errors, {2} warnings",Core.Songs.Songs.Count, errorCount,warnCount),"DefaultFont",Core.Metrics["LoadErrorCount",0],Color.White,FontAlign.Left);
            FontManager.DrawString("" + GameCore.VERSION_STRING, "DefaultFont", Core.Metrics["LoadVersion", 0], Color.White, FontAlign.Left);

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
