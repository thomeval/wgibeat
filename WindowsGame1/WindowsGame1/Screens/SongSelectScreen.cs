﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class SongSelectScreen : GameScreen
    {
        private readonly List<SongListItem> _songList = new List<SongListItem>();
        private int _selectedIndex;
        private Sprite _scoreBaseSprite;
        private Sprite _headerSprite;
        private SpriteMap _iconSpriteMap;
        private Sprite _background;
        private SpriteMap _gradeSpriteMap;
       
        private BpmMeter _bpmMeter;
        private SongSortDisplay _songSortDisplay;
        private readonly List<PlayerOptionsFrame> _playerOptions = new List<PlayerOptionsFrame>();

        private bool _resetSongTime = true;
        private double _songStartTime;
        private int _songListDrawOffset;
        private const int LISTITEMS_DRAWN = 7;
        private const int NUM_EVALUATIONS = 19;
        private const double SONG_CHANGE_SPEED = 0.9;

        private SineSwayParticleField _field = new SineSwayParticleField();

        private SongPreviewManager _songPreviewManager;

        public SongSelectScreen(GameCore core) : base(core)
        {
            
        }

        public override void Initialize()
        {

            _songSortDisplay = new SongSortDisplay();
            _songSortDisplay.SetPosition(Core.Metrics["SongSortDisplay", 0]);
            _songSortDisplay.InitSprites();
            _songPreviewManager = new SongPreviewManager { SongManager = Core.Songs };
            _bpmMeter = new BpmMeter();
            _bpmMeter.SetPosition(Core.Metrics["BPMMeter", 0]);
            _playerOptions.Clear();

            var frameCount = 0;
            for (int x = 3; x >= 0; x-- )
            {
                if (Core.Players[x].Playing)
                {
                    _playerOptions.Add(new PlayerOptionsFrame{Player = Core.Players[x], PlayerIndex = x});
                    _playerOptions[frameCount].SetPosition(Core.Metrics["PlayerOptionsFrame",frameCount]);
                    frameCount++;
                }
            }

                if (_songList.Count == 0)
                {
                    CreateSongList();
                }

            if (Core.Settings.Exists("LastSongPlayed"))
            {
                var lastSongHash = Core.Settings.Get<int>("LastSongPlayed");
                var lastSong = (from e in _songList where e.Song.GetHashCode() == lastSongHash select e).FirstOrDefault();
                if (lastSong != null)
                {
                    _selectedIndex = _songList.IndexOf(lastSong);
                }
            }
            InitSprites();

            base.Initialize();
            PlaySongPreview();
        }

        private void InitSprites()
        {
            _background = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["allBackground"],
                Width = Core.Window.ClientBounds.Width,
                X = 0,
                Y = 0
            };
            _scoreBaseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures["songHighScoreBase"]
            };

            _headerSprite = new Sprite
            {
             //   Height = 80,
             //   Width = 800,
                SpriteTexture = TextureManager.Textures["songSelectHeader"]
            };
            _iconSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = (int)Difficulty.COUNT + 1,
                SpriteTexture = TextureManager.Textures["playerDifficulties"]
            };
            _gradeSpriteMap = new SpriteMap
                                  {
                                      Columns = 1,
                                      Rows = NUM_EVALUATIONS,
                                      SpriteTexture = TextureManager.Textures["evaluationGrades"]
                                  };
        }

        private void CreateSongList()
        {

            foreach (GameSong song in Core.Songs.AllSongs())
            {
                _songList.Add(new SongListItem {Height = 50, Song = song, Width = 380});
            }
            SortSongList();
        }

        private void SortSongList()
        {
            int currentSelection = _songList[_selectedIndex].Song.GetHashCode();
            switch (_songSortDisplay.SongSortMode)
            {
                case SongSortMode.TITLE:
                    _songList.Sort(SortByName);
                    break;
                case SongSortMode.ARTIST:
                    _songList.Sort(SortByArtist);
                    break;
                    case SongSortMode.BPM:
                    _songList.Sort(SortByBpm);
                    break;
            }
            var lastSong = (from e in _songList where e.Song.GetHashCode() == currentSelection select e).FirstOrDefault();
            if (lastSong != null)
            {
                _selectedIndex = _songList.IndexOf(lastSong);
            }
        }

        private int SortByName(SongListItem first, SongListItem second)
        {
            return first.Song.Title.CompareTo(second.Song.Title);
        }
        private int SortByArtist(SongListItem first, SongListItem second)
        {
            return first.Song.Artist.CompareTo(second.Song.Artist);
        }
        private int SortByBpm(SongListItem first, SongListItem second)
        {
            return first.Song.Bpm.CompareTo(second.Song.Bpm);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawSongList(spriteBatch);
            _headerSprite.SetPosition(Core.Metrics["SongSelectScreenHeader", 0]);
            _headerSprite.Draw(spriteBatch);
            DrawWaveForm(spriteBatch);
            DrawPlayerOptions(spriteBatch);
            DrawHighScoreFrame(spriteBatch);
            DrawBpmMeter(gameTime, spriteBatch);
            DrawSongText(spriteBatch);
            _songSortDisplay.Draw(spriteBatch);
      
            TextureManager.DrawString(spriteBatch,"Mode: " + Core.Cookies["CurrentGameType"],"DefaultFont", Core.Metrics["SelectedMode", 0], Color.Black,FontAlign.CENTER);

        }

        private void DrawSongText(SpriteBatch spriteBatch)
        {
            var currentSong = _songList[_selectedIndex].Song;

            if (!String.IsNullOrEmpty(currentSong.Title))
            {
                TextureManager.DrawString(spriteBatch, currentSong.Title, "DefaultFont",
                                          Core.Metrics["SelectedSongTitle", 0], Color.Black, FontAlign.CENTER);
            }
            if (!String.IsNullOrEmpty(currentSong.Subtitle))
            {
                TextureManager.DrawString(spriteBatch, currentSong.Subtitle, "DefaultFont",
                                          Core.Metrics["SelectedSongSubtitle", 0], Color.Black, FontAlign.CENTER);
            }
        }

        private const int WAVEFORM_POINTS = 512;
        private const int WAVEFORM_CLUSTER_SIZE = 16;
        private readonly float[] _maxLevels = new float[WAVEFORM_POINTS / WAVEFORM_CLUSTER_SIZE];
        private readonly float[] _dropSpeed = new float[WAVEFORM_POINTS / WAVEFORM_CLUSTER_SIZE];
        private double _displayedBpm;

        private void DrawWaveForm(SpriteBatch spriteBatch)
        {
            if (_songPreviewManager.ChannelIndexCurrent != -1)
            {
                float[] levels = Core.Songs.GetChannelWaveform(_songPreviewManager.ChannelIndexCurrent, WAVEFORM_POINTS);

                var line = new PrimitiveLine(Core.GraphicsDevice);
                line.Colour = Color.Black;
                line.Position = Core.Metrics["SelectedSongSpectrum", 0];

                line.AddVector(new Vector2(0, -70));
                line.AddVector(new Vector2(200, -70));
                line.Render(spriteBatch);
                line.ClearVectors();
                int posX = 0;

                var averageLevels = new float[levels.Count() / WAVEFORM_CLUSTER_SIZE];
      
                for (int x = 0; x < averageLevels.Count(); x++ )
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
                        line.AddVector(new Vector2(posX, -70 * averageLevels[x]));
                        line.AddVector(new Vector2(posX + 6, -70 * averageLevels[x]));
                        posX += 6;
                    }
                line.Render(spriteBatch);

                posX = 0;
                for (int x = 0; x < _maxLevels.Count(); x++)
                {
                    line.ClearVectors();
 
                    line.AddVector(new Vector2(posX, -70 * _maxLevels[x]));
                    line.AddVector(new Vector2(posX + 6, -70 * _maxLevels[x]));
                    line.Render(spriteBatch);
                    posX += 6; 
                    
                }
            }
        }
        private void DrawBpmMeter(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_displayedBpm == 0.0)
            {
                _displayedBpm = _songList[_selectedIndex].Song.Bpm;
            }
            else
            {
                var diff = _displayedBpm - _songList[_selectedIndex].Song.Bpm;
                _displayedBpm -= diff* 0.2;
            }
            if (_resetSongTime)
            {
                _resetSongTime = false;
                _songStartTime = gameTime.TotalRealTime.TotalMilliseconds;
            }
            _bpmMeter.SongTime = (gameTime.TotalRealTime.TotalMilliseconds - _songStartTime) / 1000 * (_songList[_selectedIndex].Song.Bpm / 60);

            _bpmMeter.Draw(spriteBatch);

            TextureManager.DrawString(spriteBatch, String.Format("{0:000.0}", _displayedBpm), "TwoTechLarge",
                                   Core.Metrics["SelectedSongBPMDisplay", 0], Color.Black, FontAlign.RIGHT);
        }

        private void DrawHighScoreFrame(SpriteBatch spriteBatch)
        {
            _scoreBaseSprite.SetPosition(Core.Metrics["SongHighScoreBase", 0]);
          //  _scoreBaseSprite.Draw(spriteBatch);
            var cgt = HighScoreManager.TranslateGameType((GameType) Core.Cookies["CurrentGameType"]);
            var highScoreEntry = GetDisplayedHighScore(cgt);
            var displayedScore = (highScoreEntry == null) ? 0 : highScoreEntry.Scores[cgt];
            var displayedGrade = (highScoreEntry == null) ? -1 : highScoreEntry.Grades[cgt];
            var displayedDifficulty = (highScoreEntry == null) ? -1 : (int) highScoreEntry.Difficulties[cgt] + 1;

          //  TextureManager.DrawString(spriteBatch,"" + displayedScore, "DefaultFont",Core.Metrics["SongHighScore", 0], Color.Black,FontAlign.RIGHT);
            
            if (displayedGrade != -1)
            {
          //      _gradeSpriteMap.Draw(spriteBatch, displayedGrade, 68, 24, Core.Metrics["SongHighScoreGrade",0]);
            }
            if (displayedDifficulty != -1)
            {
         //       _iconSpriteMap.Draw(spriteBatch, displayedDifficulty, 24, 24, Core.Metrics["SongHighScoreDifficulty", 0]);               
            }
        }

        private HighScoreEntry GetDisplayedHighScore(GameType gameType)
        {
            Core.HighScores.CurrentSong = _songList[_selectedIndex].Song;
            var highScoreEntry =
                Core.HighScores.GetHighScoreEntry(_songList[_selectedIndex].Song.GetHashCode());
            if (highScoreEntry == null)
            {
                return null;
            }
            if (!highScoreEntry.Scores.ContainsKey(gameType))
            {
                return null;
            }
            return highScoreEntry;
        }

        private void DrawPlayerOptions(SpriteBatch spriteBatch)
        {
            foreach (PlayerOptionsFrame pof in _playerOptions)
            {
                pof.Draw(spriteBatch);
            }
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {

            _background.Draw(spriteBatch);
            _field.Draw(spriteBatch);
        }

        private void DrawSongList(SpriteBatch spriteBatch)
        {

            var midpoint = Core.Metrics["SongListMidpoint", 0];
            midpoint.Y += _songListDrawOffset;
            _songList[_selectedIndex].SetPosition(midpoint);
           // 
            _songList[_selectedIndex].IsSelected = true;
            _songList[_selectedIndex].Draw(spriteBatch);

            //Draw SongListItems below (after) the selected one.
            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                midpoint.Y += 50;
                _songList[(_selectedIndex + x) % _songList.Count].IsSelected = false;
                _songList[(_selectedIndex + x) % _songList.Count].SetPosition(midpoint);
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
                _songList[index].SetPosition(midpoint);
                _songList[index].Draw(spriteBatch);
            }

            midpoint.Y -= _songListDrawOffset;
            _songListDrawOffset = (int) (_songListDrawOffset * SONG_CHANGE_SPEED);

        }

        public override void PerformAction(Action action)
        {
            int player;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            var playerOptions = (from e in _playerOptions where e.PlayerIndex == player select e).SingleOrDefault();
            switch (paction)
            {
                case "UP":
                    if ((playerOptions != null) && (playerOptions.SpeedChangeActive))
                    {
                        playerOptions.AdjustSpeed(1);
                    }
                    else
                    {
                        MoveSelectionUp();
                    }
                    break;
                case "DOWN":

                    if ((playerOptions != null) &&(playerOptions.SpeedChangeActive))
                    {
                        playerOptions.AdjustSpeed(-1);
                    }
                    else
                    {
                        MoveSelectionDown();
                    }
                    break;

                case "LEFT":
                    if (playerOptions.SpeedChangeActive)
                    {
                        playerOptions.AdjustDifficulty(-1);
                    }
                    else if (_songSortDisplay.Active)
                    {
                        _songSortDisplay.DecrementSort();
                        SortSongList();
                    }
                    break;

                case "RIGHT":
                    if (playerOptions.SpeedChangeActive)
                    {
                        playerOptions.AdjustDifficulty(1);
                    }
                    else if (_songSortDisplay.Active)
                    {
                        _songSortDisplay.IncrementSort();
                        SortSongList();
                    }
                    break;

                case "BEATLINE":
                    _songSortDisplay.Active = true;
                    break;
                case "SELECT":
                    playerOptions.SpeedChangeActive = true;
                    break;
                case "START":
                    StartSong();
                    break;
                case "BACK":
                    _songPreviewManager.Dispose();
                    Core.ScreenTransition("NewGame");
                    break;

            }
        }

        public override void PerformActionReleased(Action action)
        {
            int player;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            var playerOptions = (from e in _playerOptions where e.PlayerIndex == player select e).SingleOrDefault();
            switch (paction)
            {
                case "SELECT":
                    if (playerOptions != null)
                    {
                        playerOptions.SpeedChangeActive = false;
                    }
                    break;
                case "BEATLINE":
                    _songSortDisplay.Active = false;
                    break;
            }
        }

        private void StartSong()
        {
            _songPreviewManager.Dispose();
            Core.Cookies["CurrentSong"] = _songList[_selectedIndex].Song;
            Core.Settings.Set("LastSongPlayed", _songList[_selectedIndex].Song.GetHashCode());
            Core.ScreenTransition("MainGame");
        }

        private void MoveSelectionUp()
        {
            _selectedIndex -= 1;
            if (_selectedIndex < 0)
            {
                _selectedIndex = _songList.Count - 1;
            }
            PlaySongPreview();
            _songListDrawOffset -= 50;
        }

        private void MoveSelectionDown()
        {
            _selectedIndex = (_selectedIndex + 1)%_songList.Count();
            PlaySongPreview();
            _songListDrawOffset += 50;
        }

        private void PlaySongPreview()
        {
            bool previewsOn = false;

            _bpmMeter.Bpm = _songList[_selectedIndex].Song.Bpm;
            _resetSongTime = true;
            if (Core.Settings.Exists("SongPreview"))
            {
                previewsOn = Core.Settings.Get<bool>("SongPreview");
            }

            if (previewsOn)
            {

                _songPreviewManager.SetPreviewedSong(_songList[_selectedIndex].Song);
                if (Core.Cookies.ContainsKey("MenuMusicChannel"))
                {
                    _songPreviewManager.ChannelIndexPrevious = (int)Core.Cookies["MenuMusicChannel"];
                    Core.Cookies.Remove("MenuMusicChannel");
                }
            }
        }
    } 
}
