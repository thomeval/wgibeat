using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class SongSelectScreen : GameScreen
    {
        private List<SongListItem> SongList = new List<SongListItem>();
        private int _selectedIndex = 0;
        private Sprite _scoreBaseSprite;
        private Sprite _headerSprite;
        private SpriteMap _frameSpriteMap;
        private SpriteMap _iconSpriteMap;
        private Sprite _background;
        private SpriteMap _gradeSpriteMap;
       
        private BpmMeter _bpmMeter;
        private bool _resetSongTime = true;
        private double _songStartTime = 0;
        private const int LISTITEMS_DRAWN = 6;
        private const int NUM_EVALUATIONS = 19;

        private SineSwayParticleField _field = new SineSwayParticleField();

        private SongPreviewManager _songPreviewManager;
        public SongSelectScreen(GameCore core) : base(core)
        {
            
        }

        public override void Initialize()
        {
            if (SongList.Count == 0)
            {
                CreateSongList();
            }
            //TODO: Move to Core.Cookies
            if (Core.Settings.Exists("LastSongPlayed"))
            {
                var lastSongHash = Core.Settings.Get<int>("LastSongPlayed");
                var lastSong = (from e in SongList where e.Song.GetHashCode() == lastSongHash select e).FirstOrDefault();
                if (lastSong != null)
                {
                    _selectedIndex = SongList.IndexOf(lastSong);
                }
            }
            InitSprites();
            _songPreviewManager = new SongPreviewManager{SongManager = Core.Songs};
            _bpmMeter = new BpmMeter ();
            _bpmMeter.SetPosition(Core.Metrics["BPMMeter", 0]);

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
            _frameSpriteMap = new SpriteMap
            {
                Columns = 4,
                Rows = 1,
                SpriteTexture = TextureManager.Textures["playerDifficultiesFrame"]
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
                SongList.Add(new SongListItem {Height = 50, Song = song, Width = 380});
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawSongList(spriteBatch);
            DrawWaveForm(spriteBatch);
            DrawPlayerDifficulties(spriteBatch);
            DrawHighScoreFrame(spriteBatch);
            DrawBpmMeter(gameTime, spriteBatch);
            _headerSprite.SetPosition(Core.Metrics["SongSelectScreenHeader",0]);
            _headerSprite.Draw(spriteBatch);
            
            TextureManager.DrawString(spriteBatch,"Mode: " + Core.Settings.Get<GameType>("CurrentGameType"),"DefaultFont", Core.Metrics["SelectedMode", 0], Color.Black,FontAlign.CENTER);

        }

        private void DrawWaveForm(SpriteBatch spriteBatch)
        {
            if (_songPreviewManager.ChannelIndexCurrent != -1)
            {
                float[] levels = Core.Songs.GetChannelWaveform(_songPreviewManager.ChannelIndexCurrent);


                PrimitiveLine line = new PrimitiveLine(Core.GraphicsDevice);

                line.Colour = Color.Black;
                line.AddVector(new Vector2(200,200));
                line.AddVector(new Vector2(400,200));
                line.Render(spriteBatch);
                line.ClearVectors();
                line.Position.X = 200;
                line.Position.Y = 250;
                int posX = 0;

                float[] averageLevels = new float[levels.Count() / 32];

                for (int x = 0; x < averageLevels.Count(); x++ )
                {
                    averageLevels[x] = levels.Skip(32 * x).Take(32).Average();
                }
                    for (int x = 0; x < averageLevels.Count(); x++)
                    {

                        averageLevels[x] = Math.Min(1, averageLevels[x] * 10 * (x + 1));
                        line.AddVector(new Vector2(posX, 0));
                        line.AddVector(new Vector2(posX, -50 * averageLevels[x]));
                        line.AddVector(new Vector2(posX + 5, -50 * averageLevels[x]));
                        posX += 5;
                    }
                line.Render(spriteBatch);
            }
        }
        private void DrawBpmMeter(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_resetSongTime)
            {
                _resetSongTime = false;
                _songStartTime = gameTime.TotalRealTime.TotalMilliseconds;
            }
            _bpmMeter.SongTime = (gameTime.TotalRealTime.TotalMilliseconds - _songStartTime) / 1000 * (SongList[_selectedIndex].Song.Bpm / 60);

            _bpmMeter.Draw(spriteBatch);

            TextureManager.DrawString(spriteBatch, String.Format("{0:000.0}", SongList[_selectedIndex].Song.Bpm), "TwoTechLarge",
                                   Core.Metrics["SongBPMDisplay", 0], Color.Black, FontAlign.RIGHT);


        }

        private void DrawHighScoreFrame(SpriteBatch spriteBatch)
        {
            _scoreBaseSprite.SetPosition(Core.Metrics["SongHighScoreBase", 0]);
            _scoreBaseSprite.Draw(spriteBatch);
            var cgt = Core.Settings.Get<GameType>("CurrentGameType");
            var highScoreEntry = GetDisplayedHighScore();
            var displayedScore = (highScoreEntry == null) ? 0 : highScoreEntry.Scores[cgt];
            var displayedGrade = (highScoreEntry == null) ? -1 : highScoreEntry.Grades[cgt];
            var displayedDifficulty = (highScoreEntry == null) ? -1 : (int) highScoreEntry.Difficulties[cgt] + 1;

            TextureManager.DrawString(spriteBatch,"" + displayedScore, "DefaultFont",Core.Metrics["SongHighScore", 0], Color.Black,FontAlign.RIGHT);
            
            if (displayedGrade != -1)
            {
                _gradeSpriteMap.Draw(spriteBatch, displayedGrade, 68, 24, Core.Metrics["SongHighScoreGrade",0]);
            }
            if (displayedDifficulty != -1)
            {
                _iconSpriteMap.Draw(spriteBatch, displayedDifficulty, 24, 24, Core.Metrics["SongHighScoreDifficulty", 0]);               
            }
        }


        private HighScoreEntry GetDisplayedHighScore()
        {
            Core.HighScores.CurrentSong = SongList[_selectedIndex].Song;
            var highScoreEntry =
                Core.HighScores.GetHighScoreEntry(SongList[_selectedIndex].Song.GetHashCode());
            if (highScoreEntry == null)
            {
                return null;
            }
            if (!highScoreEntry.Scores.ContainsKey(Core.Settings.Get<GameType>("CurrentGameType")))
            {
                return null;
            }
            return highScoreEntry;
        }

        private void DrawPlayerDifficulties(SpriteBatch spriteBatch)
        {
            int playerCount = 0;
            for (int x = 0; x < 4; x++)
            {
                if (Core.Players[x].Playing)
                {
                    _frameSpriteMap.Draw(spriteBatch, x, 50, 100, Core.Metrics["PlayerDifficultiesFrame", playerCount]);
                    int idx = GetPlayerDifficulty(x);
                    _iconSpriteMap.Draw(spriteBatch, idx, 40, 40, Core.Metrics["PlayerDifficulties", playerCount]);
                    playerCount++;
                }
            }
        }

        private int GetPlayerDifficulty(int player)
        {
            if (!Core.Players[player].Playing)
            {
                return 0;
            }

            return 1 + (int) (Core.Players[player].PlayDifficulty);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {

            _background.Draw(spriteBatch);
            _field.Draw(spriteBatch);
        }

        private void DrawSongList(SpriteBatch spriteBatch)
        {

            DrawBackground(spriteBatch);



            var midpoint = Core.Metrics["SongListMidpoint", 0];
            SongList[_selectedIndex].SetPosition(midpoint);
            SongList[_selectedIndex].IsSelected = true;
            SongList[_selectedIndex].Draw(spriteBatch);

            //Draw SongListItems below (after) the selected one.
            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                midpoint.Y += 50;
                SongList[(_selectedIndex + x) % SongList.Count].IsSelected = false;
                SongList[(_selectedIndex + x) % SongList.Count].SetPosition(midpoint);
                SongList[(_selectedIndex + x) % SongList.Count].Draw(spriteBatch);
            }
         
            midpoint.Y -= 50 * LISTITEMS_DRAWN;
            int index = _selectedIndex;

            //Draw SongListItems above (before) the selected one.
            for (int x = 1; x <= LISTITEMS_DRAWN; x++)
            {
                index -= 1;
                if (index < 0)
                {
                    index = SongList.Count - 1;
                }
                midpoint.Y -= 50;
                SongList[index].IsSelected = false;
                SongList[index].SetPosition(midpoint);
                SongList[index].Draw(spriteBatch);
            }
        }


        public override void PerformAction(Action action)
        {
            switch (action)
            {
                case Action.P1_UP:
                case Action.P2_UP:
                case Action.P3_UP:
                case Action.P4_UP:
                    MoveSelectionUp();
                    break;
                case Action.P1_DOWN:
                case Action.P2_DOWN:
                case Action.P3_DOWN:
                case Action.P4_DOWN:
                    MoveSelectionDown();
                    break;
                case Action.P1_START:
                case Action.P2_START:
                case Action.P3_START:
                case Action.P4_START:
                    StartSong();
                    break;
                case Action.SYSTEM_BACK:
                    _songPreviewManager.Dispose();
                    Core.ScreenTransition("NewGame");
                    break;

            }
        }

        private void StartSong()
        {
            _songPreviewManager.Dispose();
            Core.Settings.Set("CurrentSong",SongList[_selectedIndex].Song);
            Core.Settings.Set("LastSongPlayed", SongList[_selectedIndex].Song.GetHashCode());
            Core.ScreenTransition("MainGame");
        }

        private void MoveSelectionUp()
        {
            _selectedIndex -= 1;
            if (_selectedIndex < 0)
            {
                _selectedIndex = SongList.Count - 1;
            }
            PlaySongPreview();
        }

        private void MoveSelectionDown()
        {
            _selectedIndex = (_selectedIndex + 1)%SongList.Count();
            PlaySongPreview();
        }

        private void PlaySongPreview()
        {
            bool previewsOn = false;

            _bpmMeter.Bpm = SongList[_selectedIndex].Song.Bpm;
            _resetSongTime = true;
            if (Core.Settings.Exists("SongPreview"))
            {
                previewsOn = Core.Settings.Get<bool>("SongPreview");
            }

            if (previewsOn)
            {

                _songPreviewManager.SetPreviewedSong(SongList[_selectedIndex].Song);
                if (Core.Cookies.ContainsKey("MenuMusicChannel"))
                {
                    _songPreviewManager.ChannelIndexPrevious = (int)Core.Cookies["MenuMusicChannel"];
                    Core.Cookies.Remove("MenuMusicChannel");
                }
            }
        }
    }
}
