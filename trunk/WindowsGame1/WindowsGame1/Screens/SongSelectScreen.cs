using System;
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
        private Sprite _headerSprite;
        private Sprite _background;
        private Sprite _spectrumBackground;
        private Sprite _listBackend;
        private bool _previewStarted;
        
        private BpmMeter _bpmMeter;
        private SongSortDisplay _songSortDisplay;
        private HighScoreFrame _highScoreFrame;
        private readonly List<PlayerOptionsFrame> _playerOptions = new List<PlayerOptionsFrame>();

        private bool _resetSongTime = true;
        private double _songStartTime;
        private int _songListDrawOffset;
        private const int LISTITEMS_DRAWN = 7;
        private const double SONG_CHANGE_SPEED = 0.9;

        private SineSwayParticleField _field = new SineSwayParticleField();

        public CrossfaderManager Crossfader;

        public SongSelectScreen(GameCore core) : base(core)
        {           
        }

        public override void Initialize()
        {
            _songSortDisplay = new SongSortDisplay();
            _songSortDisplay.Position = (Core.Metrics["SongSortDisplay", 0]);
            _songSortDisplay.InitSprites();
            Crossfader = Core.Crossfader;
            _previewStarted = false;
            _highScoreFrame = new HighScoreFrame {EnableFadeout = false};
            _highScoreFrame.Position = (Core.Metrics["SongHighScoreFrame", 0]);
            _highScoreFrame.InitSprites();
            _bpmMeter = new BpmMeter();
            _bpmMeter.Position = (Core.Metrics["BPMMeter", 0]);
            _playerOptions.Clear();

            var frameCount = 0;
            for (int x = 3; x >= 0; x-- )
            {
                if (Core.Players[x].Playing)
                {
                    _playerOptions.Add(new PlayerOptionsFrame{Player = Core.Players[x], PlayerIndex = x});
                    _playerOptions[frameCount].Position = (Core.Metrics["PlayerOptionsFrame", frameCount]);
                    frameCount++;
                }
            }

                CreateSongList();

                var lastSongHash = Core.Settings.Get<int>("LastSongPlayed");
                var lastSong = (from e in _songList where e.Song.GetHashCode() == lastSongHash select e).FirstOrDefault();
                if (lastSong != null)
                {
                    _selectedIndex = _songList.IndexOf(lastSong);
                }

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

            _headerSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures["SongSelectHeader"]
            };
            _headerSprite.Position = (Core.Metrics["SongSelectScreenHeader", 0]);

            _spectrumBackground = new Sprite
            {
                SpriteTexture = TextureManager.Textures["SpectrumBackground"]
            };
            _spectrumBackground.Position = Core.Metrics["SelectedSongSpectrum", 0];
            _spectrumBackground.Y -= 70;
            _listBackend = new Sprite {SpriteTexture = TextureManager.Textures["SongListBackend"],Height=232,Width=50};
            _listBackend.Position = (Core.Metrics["SongListBackend", 0]);
        }

        private void CreateSongList()
        {
            _songList.Clear();
            foreach (GameSong song in Core.Songs.Songs)
            {
                _songList.Add(new SongListItem {Height = 50, Song = song, Width = 380});
            }
            SortSongList();
            _highScoreFrame.HighScoreEntry = GetDisplayedHighScore((GameType)Core.Cookies["CurrentGameType"]);
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
            DrawPlayerOptions(spriteBatch);
            DrawHighScoreFrame(spriteBatch);
            
            DrawWaveForm(spriteBatch);
            DrawBpmMeter(gameTime, spriteBatch);
            DrawSongList(spriteBatch);
            _headerSprite.Draw(spriteBatch);
            _songSortDisplay.Draw(spriteBatch);
     
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

                var line = new PrimitiveLine(Core.GraphicsDevice);
                line.Colour = Color.White;
                line.Position = Core.Metrics["SelectedSongSpectrum", 0];

                int posX = 0;

                var averageLevels = new float[levels.Count() / WAVEFORM_CLUSTER_SIZE];
      
                for (int x = 0; x < averageLevels.Count() - 4; x++ )
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
          
            if (_resetSongTime)
            {
                _resetSongTime = false;
                _songStartTime = gameTime.TotalRealTime.TotalMilliseconds;
            }
            _bpmMeter.SongTime = (gameTime.TotalRealTime.TotalMilliseconds - _songStartTime) / 1000 * (_songList[_selectedIndex].Song.Bpm / 60);

            _bpmMeter.Draw(spriteBatch);
        }

        private void DrawHighScoreFrame(SpriteBatch spriteBatch)
        {
            var cgt = HighScoreManager.TranslateGameType((GameType) Core.Cookies["CurrentGameType"]);
            _highScoreFrame.HighScoreEntry = GetDisplayedHighScore(cgt);
            _highScoreFrame.Draw(spriteBatch);
        }

        private HighScoreEntry GetDisplayedHighScore(GameType gameType)
        {
            Core.HighScores.CurrentSong = _songList[_selectedIndex].Song;
            var highScoreEntry =
                Core.HighScores.GetHighScoreEntry(_songList[_selectedIndex].Song.GetHashCode(),gameType);
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
            _listBackend.Draw(spriteBatch);

            var midpoint = Core.Metrics["SongListMidpoint", 0];
            midpoint.Y += _songListDrawOffset;
            _songList[_selectedIndex].Position = (midpoint);
           // 
            _songList[_selectedIndex].IsSelected = true;
            _songList[_selectedIndex].Draw(spriteBatch);

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
            _songListDrawOffset = (int) (_songListDrawOffset * SONG_CHANGE_SPEED);

        }

        public override void Update(GameTime gameTime)
        {
            if (!_previewStarted)
            {
                _previewStarted = true;
                PlaySongPreview();
            }
            base.Update(gameTime);
        }
        public override void PerformAction(Action action)
        {
            int player = -1;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            if ((player != -1) && (!Core.Players[player].IsHumanPlayer))
            {
                return;
            }
            var playerOptions = (from e in _playerOptions where e.PlayerIndex == player select e).SingleOrDefault();
            switch (paction)
            {
                case "UP":
                    if ((playerOptions != null) && (playerOptions.OptionChangeActive))
                    {
                        playerOptions.AdjustSpeed(1);
                    }
                    else
                    {
                        MoveSelectionUp();
                    }
                    break;
                case "DOWN":

                    if ((playerOptions != null) &&(playerOptions.OptionChangeActive))
                    {
                        playerOptions.AdjustSpeed(-1);
                    }
                    else
                    {
                        MoveSelectionDown();
                    }
                    break;

                case "LEFT":
                    if (playerOptions.OptionChangeActive)
                    {
                        playerOptions.AdjustDifficulty(-1);
                        CheckCPUDifficulty();
                    }
                    else if (_songSortDisplay.Active)
                    {
                        _songSortDisplay.DecrementSort();
                        SortSongList();
                    }
                    break;

                case "RIGHT":
                    if (playerOptions.OptionChangeActive)
                    {
                        playerOptions.AdjustDifficulty(1);
                        CheckCPUDifficulty();
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
                    playerOptions.OptionChangeActive = true;
                    break;
                case "START":
                    StartSong();
                    break;
                case "BACK":
                    Core.ScreenTransition("ModeSelect");
                    break;
            }
        }

        private void CheckCPUDifficulty()
        {
            var cpuDifficulty = (from e in Core.Players where e.IsHumanPlayer select e.PlayDifficulty).Max();

            foreach (Player player in (from e in Core.Players where e.IsCPUPlayer select e))
            {
                player.PlayDifficulty = cpuDifficulty;
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
                        playerOptions.OptionChangeActive = false;
                    }
                    break;
                case "BEATLINE":
                    _songSortDisplay.Active = false;
                    break;
            }
        }

        private void StartSong()
        {
            Crossfader.StopBoth();
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
            _bpmMeter.DisplayedSong = _songList[_selectedIndex].Song;
            _resetSongTime = true;
            Crossfader.PreviewDuration = 10;
            var previewsOn = Core.Settings.Get<bool>("SongPreview");

            if (previewsOn)
            {
                Crossfader.SetPreviewedSong(_songList[_selectedIndex].Song);
            }
        }
    } 
}
