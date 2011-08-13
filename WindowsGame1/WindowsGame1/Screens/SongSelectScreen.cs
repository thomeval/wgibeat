using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Screens
{
    public class SongSelectScreen : GameScreen
    {
        private readonly List<SongListItem> _songList = new List<SongListItem>();
        private int _selectedIndex;
        private Sprite _headerSprite;
        private Sprite _background;
        private Sprite _spectrumBackground;
        private Sprite _listBackend;
        private Sprite _songCountBase;
        private bool _previewStarted;
        private PreloadState _preloadState;

        private BpmMeter _bpmMeter;
        private SongSortDisplay _songSortDisplay;
        private SongTypeDisplay _songTypeDisplay;

        private HighScoreFrame _highScoreFrame;
        private PlayerOptionsSet _playerOptionsSet;

        private int _songListDrawOffset;
        private const int LISTITEMS_DRAWN = 13;
        private const double SONG_CHANGE_SPEED = 0.9;
        private byte _songListDrawOpacity;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();

        public CrossfaderManager Crossfader;

        protected GameSong CurrentSong
        {
            get { return _songList[_selectedIndex].Song; }
        }

        public SongSelectScreen(GameCore core)
            : base(core)
        {
        }

        public override void Initialize()
        {
            _songSortDisplay = new SongSortDisplay { Position = (Core.Metrics["SongSortDisplay", 0]) };
            _songSortDisplay.InitSprites();
            _songListDrawOpacity = 255;
            Crossfader = Core.Crossfader;
            _preloadState = PreloadState.NOT_LOADING;
            _previewStarted = false;

            _highScoreFrame = new HighScoreFrame
                                  {
                                      EnableFadeout = false,
                                      Position = (Core.Metrics["SongHighScoreFrame", 0])
                                  };

            _highScoreFrame.InitSprites();
            _bpmMeter = new BpmMeter { Position = (Core.Metrics["BPMMeter", 0]) };

            _playerOptionsSet = new PlayerOptionsSet { Players = Core.Players, Positions = Core.Metrics["PlayerOptionsFrame"], CurrentGameType = (GameType)Core.Cookies["CurrentGameType"] };
            _playerOptionsSet.GameTypeInvalidated += delegate
                                                         { Core.ScreenTransition("MainMenu"); };
            _playerOptionsSet.CreatePlayerOptionsFrames();

            CreateSongList();

            var lastSongHash = Core.Settings.Get<int>("LastSongPlayed");
            var lastSong = (from e in _songList where e.Song.GetHashCode() == lastSongHash select e).FirstOrDefault();
            if (lastSong != null)
            {
                _selectedIndex = _songList.IndexOf(lastSong);
                _songSortDisplay.SelectedSongIndex = _selectedIndex;
            }


            _songTypeDisplay = new SongTypeDisplay { Position = Core.Metrics["SongTypeDisplay", 0], Width = 112, Height = 42 };
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

            _headerSprite = new Sprite
                                {
                                    SpriteTexture = TextureManager.Textures("SongSelectHeader"),
                                    Position = (Core.Metrics["SongSelectScreenHeader", 0])
                                };

            _spectrumBackground = new Sprite
                                      {
                                          SpriteTexture = TextureManager.Textures("SpectrumBackground"),
                                          Position = Core.Metrics["SelectedSongSpectrum", 0]
                                      };
            _spectrumBackground.Y -= 65;
            _listBackend = new Sprite
                               {
                                   SpriteTexture = TextureManager.Textures("SongListBackend"),
                                   Height = 232,
                                   Width = 50,
                                   Position = (Core.Metrics["SongListBackend", 0])
                               };
            _songCountBase = new Sprite { SpriteTexture = TextureManager.Textures("SongCountBase"), Position = Core.Metrics["SongCountDisplay", 0] };
        }

        private void CreateSongList()
        {
            _songList.Clear();
            foreach (GameSong song in Core.Songs.Songs)
            {
                _songList.Add(new SongListItem { Height = 50, Song = song, Width = 380, TextMaxWidth = 325 });
            }
            _songSortDisplay.SongList = _songList;
            _songSortDisplay.SongSortMode = SongSortMode.TITLE;
            _highScoreFrame.HighScoreEntry = GetDisplayedHighScore((GameType)Core.Cookies["CurrentGameType"]);
        }



        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {


            DrawBackground(spriteBatch);
            DrawPlayerOptions(spriteBatch);
            DrawHighScoreFrame(spriteBatch);

            if (_preloadState == PreloadState.LOADING_STARTED)
            {
                TextureManager.DrawString(spriteBatch, "Loading...", "LargeFont", Core.Metrics["SongSelectLoadingMessage", 0], Color.Black, FontAlign.LEFT);
            }

            DrawWaveForm(spriteBatch);
            _songTypeDisplay.Draw(spriteBatch);
            DrawBpmMeter(spriteBatch);

            DrawSongList(spriteBatch);
            _headerSprite.Draw(spriteBatch);
            DrawSongCount(spriteBatch);
            _songSortDisplay.Draw(spriteBatch);

        }

        private void DrawSongCount(SpriteBatch spriteBatch)
        {
            _songCountBase.Draw(spriteBatch);
            TextureManager.DrawString(spriteBatch, _songList.Count + "", "TwoTech36", Core.Metrics["SongCountDisplay", 1], Color.Black, FontAlign.CENTER);

        }

        private const int WAVEFORM_POINTS = 512;
        private const int WAVEFORM_CLUSTER_SIZE = 16;
        private readonly float[] _maxLevels = new float[WAVEFORM_POINTS / WAVEFORM_CLUSTER_SIZE];
        private readonly float[] _dropSpeed = new float[WAVEFORM_POINTS / WAVEFORM_CLUSTER_SIZE];

        private void DrawWaveForm(SpriteBatch spriteBatch)
        {
            _spectrumBackground.Draw(spriteBatch);

            if (Crossfader.ChannelIndexCurrent != -1)
            {
                float[] levels = Core.Audio.GetChannelSpectrum(Crossfader.ChannelIndexCurrent, WAVEFORM_POINTS);

                var line = new PrimitiveLine(Core.GraphicsDevice)
                               {
                                   Colour = Color.White,
                                   Position = Core.Metrics["SelectedSongSpectrum", 0]
                               };

                int posX = 0;

                var averageLevels = new float[levels.Count() / WAVEFORM_CLUSTER_SIZE];

                for (int x = 0; x < averageLevels.Count() - 4; x++)
                {
                    averageLevels[x] = levels.Skip(WAVEFORM_CLUSTER_SIZE * x).Take(WAVEFORM_CLUSTER_SIZE).Average();
                    // averageLevels[x] = averageLevels[x]* 2 * (float) Math.Pow(x, 1.5);  
                    averageLevels[x] = Math.Min(1, averageLevels[x] * 8 * (x + 1));

                    if (averageLevels[x] >= _maxLevels[x])
                    {
                        _dropSpeed[x] = 0.0f;
                    }
                    else
                    {
                        _dropSpeed[x] += 0.0005f;
                    }
                    _maxLevels[x] = Math.Max(averageLevels[x], _maxLevels[x] - _dropSpeed[x]);

                    line.AddVector(new Vector2(posX, 0));
                    line.AddVector(new Vector2(posX, -65 * averageLevels[x]));
                    line.AddVector(new Vector2(posX + 6, -65 * averageLevels[x]));
                    posX += 6;
                }

                line.Render(spriteBatch);

                posX = 0;
                for (int x = 0; x < _maxLevels.Count(); x++)
                {
                    line.ClearVectors();

                    line.AddVector(new Vector2(posX, -65 * _maxLevels[x]));
                    line.AddVector(new Vector2(posX + 6, -65 * _maxLevels[x]));
                    line.Render(spriteBatch);
                    posX += 6;

                }
            }
        }
        private void DrawBpmMeter(SpriteBatch spriteBatch)
        {

            if (Core.Settings.Get<bool>("SongPreview") && Crossfader.ChannelIndexCurrent > -1)
            {

                var actualTime = Core.Audio.GetChannelPosition(Crossfader.ChannelIndexCurrent);
                _bpmMeter.SongTime = CurrentSong.ConvertMSToPhrase(actualTime) * 4;
                //   TextureManager.DrawString(spriteBatch,String.Format("{0:F3}",actualTime),"DefaultFont",new Vector2(20,140),Color.Black,FontAlign.LEFT);
                //    TextureManager.DrawString(spriteBatch, String.Format("{0:F3}", CurrentSong.ConvertMSToPhrase(actualTime) * 1), "DefaultFont", new Vector2(50, 120), Color.Black, FontAlign.LEFT);

            }
            else
            {
                _bpmMeter.SongTime = 0;
            }

            _bpmMeter.Draw(spriteBatch);
        }

        private void DrawHighScoreFrame(SpriteBatch spriteBatch)
        {
            var cgt = HighScoreManager.TranslateGameType((GameType)Core.Cookies["CurrentGameType"]);
            _highScoreFrame.HighScoreEntry = GetDisplayedHighScore(cgt);
            _highScoreFrame.Draw(spriteBatch);
        }

        private HighScoreEntry GetDisplayedHighScore(GameType gameType)
        {
            Core.HighScores.CurrentSong = _songList[_selectedIndex].Song;
            var highScoreEntry =
                Core.HighScores.GetHighScoreEntry(_songList[_selectedIndex].Song.GetHashCode(), gameType);
            return highScoreEntry;
        }

        private void DrawPlayerOptions(SpriteBatch spriteBatch)
        {
            _playerOptionsSet.Draw(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {

            _background.Draw(spriteBatch);
            _field.Draw(spriteBatch);
        }

        private void DrawSongList(SpriteBatch spriteBatch)
        {
            if (_preloadState == PreloadState.LOADING_STARTED)
            {
                _songListDrawOpacity = (byte) Math.Max(0, _songListDrawOpacity - 5);
            }
            _listBackend.Draw(spriteBatch);

            var midpoint = Core.Metrics["SongListMidpoint", 0];
            midpoint.Y += _songListDrawOffset;

 
            _songList[_selectedIndex].Position = (midpoint);
            _songList[_selectedIndex].IsSelected = true;
            _songList[_selectedIndex].Opacity = 255;
            _songList[_selectedIndex].Draw(spriteBatch);

            foreach (SongListItem sli in _songList)
            {
                sli.Opacity = _songListDrawOpacity;
            }
            //Draw SongListItems below (after) the selected one.
            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                midpoint.Y += 50;
                _songList[(_selectedIndex + x) % _songList.Count].IsSelected = false;
                _songList[(_selectedIndex + x) % _songList.Count].Position = (midpoint);
                _songList[(_selectedIndex + x) % _songList.Count].Draw(spriteBatch);
            }

            midpoint.Y -= 50 * LISTITEMS_DRAWN;
            int index = _selectedIndex;

            //Draw SongListItems above (before) the selected one.
            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                index -= 1;
                if (index < 0)
                {
                    index = _songList.Count - 1;
                }
                midpoint.Y -= 50;
                _songList[index].IsSelected = false;
                _songList[index].Position = (midpoint);
                _songList[index].Draw(spriteBatch);
            }

            midpoint.Y -= _songListDrawOffset;
            _songListDrawOffset = (int)(_songListDrawOffset * SONG_CHANGE_SPEED);

        }

        public override void Update(GameTime gameTime)
        {
            if (!_previewStarted)
            {
                _previewStarted = true;
                _bpmMeter.DisplayedSong = CurrentSong;
                _songTypeDisplay.Song = CurrentSong;
                Crossfader.PreviewDuration = 10;
                var previewsOn = Core.Settings.Get<bool>("SongPreview");

                if (previewsOn)
                {
                    Crossfader.SetPreviewedSong(CurrentSong, true);
                }
            }

            if (_preloadState == PreloadState.LOADING_FINISHED)
            {
                Crossfader.StopBoth();
                Core.Settings.Set("LastSongPlayed", CurrentSong.GetHashCode());
                Core.ScreenTransition("MainGame");
            }
            base.Update(gameTime);
        }

        public override void PerformAction(InputAction inputAction)
        {
            if (_preloadState == PreloadState.LOADING_STARTED)
            {
                return;
            }

            var pass = _playerOptionsSet.PerformAction(inputAction);
            if (pass)
            {
                CheckCPUDifficulty();
                return;
            }

            pass = _songSortDisplay.PerformAction(inputAction);
            if (pass)
            {

                JumpToBookmark();
                return;
            }


            //Ignore inputs from players not playing EXCEPT for system keys.
            if ((inputAction.Player > 0) && (!Core.Players[inputAction.Player - 1].IsHumanPlayer))
            {
                return;
            }

            switch (inputAction.Action)
            {
                case "UP":
                    MoveSelectionUp();
                    
                    break;
                case "DOWN":
                    MoveSelectionDown();
                    break;
                case "BEATLINE":
                    _songSortDisplay.Active = true;
                    _songSortDisplay.SelectedSongIndex = _selectedIndex;
                    break;
                case "SELECT":
                    _playerOptionsSet.SetChangeMode(inputAction.Player, true);
                    break;
                case "START":
                    StartSong();
                    break;
                case "BACK":
                    Core.ScreenTransition("ModeSelect");
                    break;
            }
        }

        private void JumpToBookmark()
        {
            _selectedIndex = _songSortDisplay.SelectedSongIndex;
            _previewStarted = false;
        }

        private void CheckCPUDifficulty()
        {
            if (((GameType)Core.Cookies["CurrentGameType"]) != GameType.VS_CPU)
            {
                return;
            }
            if ((from e in Core.Players where e.IsHumanPlayer select e).Count() == 0)
            {
                return;
            }
            var cpuDifficulty = (from e in Core.Players where e.IsHumanPlayer select e.PlayerOptions.PlayDifficulty).Max();

            foreach (Player player in (from e in Core.Players where e.IsCPUPlayer select e))
            {
                player.PlayerOptions.PlayDifficulty = cpuDifficulty;
            }
        }


        public override void PerformActionReleased(InputAction inputAction)
        {

            switch (inputAction.Action)
            {
                case "SELECT":
                    _playerOptionsSet.SetChangeMode(inputAction.Player, false);
                    break;
                case "BEATLINE":
                    _songSortDisplay.Active = false;
                    break;
            }
        }

        private void StartSong()
        {

            Core.Cookies["CurrentSong"] = CurrentSong;
            if (((GameType)Core.Cookies["CurrentGameType"]) == GameType.SYNC)
            {
                _playerOptionsSet.CheckSyncDifficulty();
            }


                _songLoadingThread = new Thread(StartSongLoading) { Name = "Song Loading Thread" };
                _songLoadingThread.Start();
            RaiseSoundTriggered(SoundEvent.SONG_DECIDE);

        }

        private Thread _songLoadingThread;
        private void StartSongLoading(object state)
        {
            _preloadState = PreloadState.LOADING_STARTED;
            Core.Log.AddMessage("Song preload started...",LogLevel.DEBUG);
            Core.Cookies["GameSongChannel"] = Core.Songs.PreloadSong(CurrentSong);
            SetPanicState();
            Core.Log.AddMessage("Song preload done.", LogLevel.DEBUG);
            _preloadState = PreloadState.LOADING_FINISHED;
        }

        private void SetPanicState()
        {
            if (Core.Cookies.ContainsKey("Panic"))
            {
                Core.Cookies.Remove("Panic");
            }
            if (!File.Exists(CurrentSong.Path + "\\" + CurrentSong.AudioFile))
            {
                Core.Cookies.Add("Panic", true);
            }

        }

        private void MoveSelectionUp()
        {
            _selectedIndex -= 1;
            if (_selectedIndex < 0)
            {
                _selectedIndex = _songList.Count - 1;
            }
            _previewStarted = false;
            _songListDrawOffset -= 50;
            RaiseSoundTriggered(SoundEvent.SONG_SELECT_UP);
        }

        private void MoveSelectionDown()
        {
            _selectedIndex = (_selectedIndex + 1) % _songList.Count();
            _previewStarted = false;
            _songListDrawOffset += 50;
            RaiseSoundTriggered(SoundEvent.SONG_SELECT_DOWN);

        }

    }

    public enum PreloadState
    {
        NOT_LOADING,
        LOADING_STARTED,
        LOADING_FINISHED
    }
}
