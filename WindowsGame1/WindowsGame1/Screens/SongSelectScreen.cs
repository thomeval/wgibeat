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
        private Sprite3D _headerSprite;
        private Sprite3D _background;
        private Sprite3D _spectrumBackground;
        private Sprite3D _listBackend;
        private Sprite3D _songCountBase;
        private Sprite3D _songLockedSprite;

        private bool _previewStarted;
        private PreloadState _preloadState;

        private BpmMeter _bpmMeter;
        private SongSortDisplay _songSortDisplay;
        private SongTypeDisplay _songTypeDisplay;
        private SpectrumDrawer _spectrumDrawer;

        private HighScoreFrame _highScoreFrame;
        private PlayerOptionsSet _playerOptionsSet;

        private double _songListDrawOffset;
        private const int LISTITEMS_DRAWN = 13;
        private const double SONG_CHANGE_SPEED = 7;
        private const int SPECTRUM_POINTS = 64;
        private const int SPECTRUM_CLUSTER_SIZE = 2;
        private double _songListDrawOpacity;

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
            _songSortDisplay = new SongSortDisplay { Position = Core.Metrics["SongSortDisplay", 0], Size = Core.Metrics["SongSortDisplay.Size",0] };
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

            _playerOptionsSet = new PlayerOptionsSet { Players = Core.Players, Positions = Core.Metrics["PlayerOptionsFrame"], CurrentGameType = (GameType)Core.Cookies["CurrentGameType"], DrawAttract = true, StackableFrames = true };
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


            _songTypeDisplay = new SongTypeDisplay { Position = Core.Metrics["SongTypeDisplay", 0], Size = Core.Metrics["SongTypeDisplay.Size",0] };
            InitSprites();
            _spectrumDrawer = new SpectrumDrawer
                                  {
                                      LevelsCount = SPECTRUM_POINTS / SPECTRUM_CLUSTER_SIZE,
                                      Size = Core.Metrics["SelectedSongSpectrum.Size", 0],
                                      Position = Core.Metrics["SelectedSongSpectrum", 0],
                                      ColorShading = Color.White
                                  };
            _songLockedSprite = new Sprite3D { Position = Core.Metrics["BPMMeter", 0], Texture = TextureManager.Textures("SongLocked") };
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

            _headerSprite = new Sprite3D
                                {
                                    Texture = TextureManager.Textures("SongSelectHeader"),
                                    Position = Core.Metrics["ScreenHeader", 0],
                                    Size = Core.Metrics["ScreenHeader.Size", 0]
                                };

            _spectrumBackground = new Sprite3D
                                      {
                                          Texture = TextureManager.Textures("SpectrumBackground"),
                                          Position = Core.Metrics["SelectedSongSpectrum", 0]
                                      };
            _spectrumBackground.Y -= 65;
            _listBackend = new Sprite3D
                               {
                                   Texture = TextureManager.Textures("SongListBackend"),
                                   Height = 232,
                                   Width = 50,
                                   Position = (Core.Metrics["SongListBackend", 0])
                               };
            _songCountBase = new Sprite3D
            {
                Texture = TextureManager.Textures("SongCountBase"),
                Position = Core.Metrics["SongCountDisplay", 0],
                Size = Core.Metrics["SongCountDisplay.Size", 0]
            };
        }

        private const int FAIL_GRADE = 18;
        private void CreateSongList()
        {
            _songList.Clear();
            SongListItem.ClearIndicatorSize = Core.Metrics["SongListItem.ClearIndicatorSize", 0];
            foreach (var song in Core.Songs.Songs)
            {
                var hse = Core.HighScores.GetHighScoreEntry(song.GetHashCode(), HighScoreManager.TranslateGameType((GameType)Core.Cookies["CurrentGameType"]));
                var clearColour = (hse == null) || (hse.Grade == FAIL_GRADE) ? -1 : (int)hse.Difficulty;
                _songList.Add(new SongListItem
                {
                    Size = Core.Metrics["SongListItem.Size", 0],
                    Song = song,
                    TextMaxWidth = (int)Core.Metrics["SongListItem.MaxTextWidth", 0].X,
                    ClearColour = clearColour
                });
            }

            _songSortDisplay.SongList = _songList;
            _songSortDisplay.SongSortMode = Core.Settings.Get<SongSortMode>("LastSortMode");
        }

        private void SetCurrentLevelInSongList()
        {
            SongListItem.PlayerLevel = GetPlayerLevel();
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {


            DrawBackground(gameTime);
            DrawPlayerOptions();
            DrawHighScoreFrame();

            if (_preloadState == PreloadState.LOADING_STARTED)
            {
                FontManager.DrawString("Loading...", "LargeFont", Core.Metrics["SongSelectLoadingMessage", 0], Color.Black, FontAlign.Left);
            }

            DrawWaveForm();
            _songTypeDisplay.Draw();
            DrawBpmMeter();

            DrawSongList();
            _headerSprite.Draw();
            DrawSongCount();
            _songSortDisplay.Draw();

            //TODO: Debug info
            /*
            var scores = Core.HighScores.PrintHighScores();
            if (scores.LastIndexOf('\n') == -1)
            {
                return;
            }
            TextureManager.DrawString(scores.Substring(scores.LastIndexOf('\n')),"DefaultFont",new Vector2(30,120),Color.Black,FontAlign.LEFT );
            TextureManager.DrawString( "SongID: " + CurrentSong.GetHashCode(), "DefaultFont", new Vector2(30, 150), Color.Black, FontAlign.LEFT);
             */
        }

        private void DrawSongCount()
        {
            _songCountBase.Draw();
            FontManager.DrawString(_songList.Count + "", "TwoTech36", Core.Metrics["SongCountDisplay", 1], Color.Black, FontAlign.Center);

        }


        private void DrawWaveForm()
        {
            _spectrumBackground.Draw();

            if (Crossfader.ChannelIndexCurrent == -1)
                return;
            float[] levels = Core.Audio.GetChannelSpectrum(Crossfader.ChannelIndexCurrent, SPECTRUM_POINTS);
            var averageLevels = new float[SPECTRUM_POINTS / SPECTRUM_CLUSTER_SIZE];
            for (int x = 0; x < levels.Length; x += SPECTRUM_CLUSTER_SIZE)
            {
                averageLevels[x / SPECTRUM_CLUSTER_SIZE] = levels.Skip(x).Take(SPECTRUM_CLUSTER_SIZE).Average();
            }
            _spectrumDrawer.Draw(averageLevels);
        }
        private void DrawBpmMeter()
        {

            if (Crossfader.ChannelIndexCurrent > -1)
            {

                var actualTime = Core.Audio.GetChannelPosition(Crossfader.ChannelIndexCurrent);
                _bpmMeter.SongTime = CurrentSong.ConvertMSToPhrase(actualTime) * 4;
            }
            else
            {
                _bpmMeter.SongTime = 0;
            }

            _bpmMeter.Draw();

            if (CurrentSong.RequiredLevel <= GetPlayerLevel())
            {
                return;
            }
            _songLockedSprite.Draw();
            FontManager.DrawString("Unlocked at profile level: " + CurrentSong.RequiredLevel, "LargeFont", Core.Metrics["SongLockedRequirements", 0], Color.Black, FontAlign.Center);
        }

        private void DrawHighScoreFrame()
        {
            var cgt = HighScoreManager.TranslateGameType((GameType)Core.Cookies["CurrentGameType"]);
            _highScoreFrame.HighScoreEntry = GetDisplayedHighScore(cgt);
            _highScoreFrame.Draw();
        }

        private HighScoreEntry GetDisplayedHighScore(GameType gameType)
        {
            Core.HighScores.CurrentSong = _songList[_selectedIndex].Song;
            var highScoreEntry =
                Core.HighScores.GetHighScoreEntry(_songList[_selectedIndex].Song.GetHashCode(), gameType);
            return highScoreEntry;
        }

        private void DrawPlayerOptions()
        {
            _playerOptionsSet.Draw();
        }

        private void DrawBackground(GameTime gameTime)
        {
            _background.Draw();
            double phrase = 0.0;
            if (Crossfader.ChannelIndexCurrent > -1)
            {
                phrase = CurrentSong.ConvertMSToPhrase(Core.Audio.GetChannelPosition(Crossfader.ChannelIndexCurrent));
            }
            _field.Draw(gameTime, phrase);
        }

        private const int SONGLIST_FADEOUT_SPEED = 300;
        private void DrawSongList()
        {
            if (_preloadState == PreloadState.LOADING_STARTED)
            {
                _songListDrawOpacity = Math.Max(0, _songListDrawOpacity - TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * SONGLIST_FADEOUT_SPEED);
            }
            _listBackend.Draw();

            var midpoint = Core.Metrics["SongListMidpoint", 0];
            midpoint.Y += (int)_songListDrawOffset;

            SetCurrentLevelInSongList();

            _songList[_selectedIndex].Position = (midpoint);
            _songList[_selectedIndex].IsSelected = true;
            _songList[_selectedIndex].Opacity = 255;

            _songList[_selectedIndex].Draw();

            foreach (SongListItem sli in _songList)
            {
                sli.Opacity = Convert.ToByte(_songListDrawOpacity);
            }
            //Draw SongListItems below (after) the selected one.
            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                midpoint.Y += Core.Metrics["SongListItem.Size", 0].Y;
                _songList[(_selectedIndex + x) % _songList.Count].IsSelected = false;
                _songList[(_selectedIndex + x) % _songList.Count].Position = (midpoint);
                _songList[(_selectedIndex + x) % _songList.Count].Draw();
            }

            midpoint.Y -= Core.Metrics["SongListItem.Size", 0].Y * LISTITEMS_DRAWN;
            int index = _selectedIndex;

            //Draw SongListItems above (before) the selected one.
            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                index -= 1;
                if (index < 0)
                {
                    index = _songList.Count - 1;
                }
                midpoint.Y -= Core.Metrics["SongListItem.Size", 0].Y;
                _songList[index].IsSelected = false;
                _songList[index].Position = (midpoint);
                _songList[index].Draw();
            }

            midpoint.Y -= (int)_songListDrawOffset;
            var changeMx = Math.Min(0.5, SONG_CHANGE_SPEED * TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds);
            _songListDrawOffset -= (_songListDrawOffset * (changeMx));

        }

        private int GetPlayerLevel()
        {
            if (!(from e in Core.Players where e.Playing select e).Any())
            {
                return 1;
            }
            return (from e in Core.Players where e.Playing select e.GetLevel()).Max();
        }

        public override void Update(GameTime gameTime)
        {
            if (!_previewStarted)
            {
                _previewStarted = true;
                _bpmMeter.DisplayedSong = CurrentSong;
                _songTypeDisplay.Song = CurrentSong;
                Crossfader.PreviewDuration = 10;

                _spectrumDrawer.ResetMaxLevels();
                Crossfader.SetPreviewedSong(CurrentSong, true, GetPlayerLevel() < CurrentSong.RequiredLevel);
            }

            if (_preloadState == PreloadState.LOADING_FINISHED)
            {
                Crossfader.StopBoth();

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
                RaiseSoundTriggered(SoundEvent.PLAYER_OPTIONS_CHANGE);
                CheckCPUDifficulty();
                return;
            }

            pass = _songSortDisplay.PerformAction(inputAction);
            if (pass)
            {
                RaiseSoundTriggered(SoundEvent.SONG_SORT_CHANGE);
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
                    RaiseSoundTriggered(SoundEvent.SONG_SORT_DISPLAY);
                    break;
                case "SELECT":
                    _playerOptionsSet.SetChangeMode(inputAction.Player, true);
                    RaiseSoundTriggered(SoundEvent.PLAYER_OPTIONS_DISPLAY);
                    break;
                case "START":
                    StartSong();
                    break;
                case "BACK":
                    Core.ScreenTransition("ModeSelect");
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
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
            if (!(from e in Core.Players where e.IsHumanPlayer select e).Any())
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
                    RaiseSoundTriggered(SoundEvent.PLAYER_OPTIONS_HIDE);
                    break;
                case "BEATLINE":
                    _songSortDisplay.Active = false;
                    RaiseSoundTriggered(SoundEvent.SONG_SORT_HIDE);
                    break;
            }
        }

        private void StartSong()
        {
            if (GetPlayerLevel() < CurrentSong.RequiredLevel)
            {
                return;
            }
            Core.Cookies["CurrentSong"] = CurrentSong;
            var gameType = (GameType)Core.Cookies["CurrentGameType"];
            if (gameType == GameType.SYNC_PRO || gameType == GameType.SYNC_PLUS)
            {
                _playerOptionsSet.CheckSyncDifficulty();
            }


            _songLoadingThread = new Thread(StartSongLoading) { Name = "Song Loading Thread" };
            _songLoadingThread.Start();

            RaiseSoundTriggered(SoundEvent.SONG_DECIDE);
            Core.Settings.Set("LastSongPlayed", CurrentSong.GetHashCode());
            Core.Settings.Set("LastSortMode", (int)_songSortDisplay.SongSortMode);
            Core.Settings.SaveToFile("settings.txt");
        }

        private Thread _songLoadingThread;
        private void StartSongLoading(object state)
        {
            _preloadState = PreloadState.LOADING_STARTED;
            Core.Log.AddMessage("Song preload started...", LogLevel.DEBUG);
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
