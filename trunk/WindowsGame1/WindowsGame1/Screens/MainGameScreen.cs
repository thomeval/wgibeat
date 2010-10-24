using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Drawing.Sets;
using WGiBeat.Managers;
using WGiBeat.Notes;
using Action = WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class MainGameScreen : GameScreen
    {

        private double _phraseNumber;
        private double _debugLastHitOffset;
        private double _songLoadDelay;

        private LifeBarSet _lifeBarSet;
        private LevelBarSet _levelbarSet;
        private HitsBarSet _hitsbarSet;
        private ScoreSet _scoreSet;
        private NoteJudgementSet _noteJudgementSet;
        private BeatlineSet _beatlineSet;
        private NoteBar[] _notebars;
        private int _playerCount;
        private GameSong _gameSong;
        private TimeSpan? _startTime;
        private CPUManager _cpuManager;
        private int _displayState;
        private double _transitionTime;
        private double _lastBlazeCheck;
        private double _lastLifeRecord;
        public MainGameScreen(GameCore core)
            : base(core)
        {
        }

        public override void Initialize()
        {
            _playerCount = 4;

            _notebars = new NoteBar[_playerCount];
            _lifeBarSet = new LifeBarSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _levelbarSet = new LevelBarSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _hitsbarSet = new HitsBarSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _scoreSet = new ScoreSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _noteJudgementSet = new NoteJudgementSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _beatlineSet = new BeatlineSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);

            _beatlineSet.NoteMissed += BeatlineNoteMissed;
            _beatlineSet.CPUNoteHit += BeatlineNoteCPUHit;
            _beatlineSet.Large = LargeBeatlinesSuitable();

            _displayState = 0;
            _songLoadDelay = 0.0;
            _confidence = 0;
            _lastBlazeCheck = 0;
            _lastLifeRecord = -0.5;
            for (int x = 0; x < _playerCount; x++)
            {

                if (Core.Players[x] == null)
                {
                    Core.Players[x] = new Player
                                          {
                                              Hits = 0,
                                              Momentum = 0,
                                              Life = 50,
                                              Score = 0,
                                              PlayDifficulty =
                                                  (Difficulty)Core.Settings.Get<int>("P" + (x + 1) + "Difficulty"),
                                              Streak = 0
                                          };
                }
                else
                {
                    _lifeBarSet.Reset();
                    Core.Players[x].ResetStats();

                }

                _notebars[x] = NoteBar.CreateNoteBar((int)Core.Players[x].Level, 0);
                _notebars[x].SetPosition(Core.Metrics["NoteBar", x]);

            }

            _gameSong = (GameSong)Core.Cookies["CurrentSong"];

            _startTime = null;
            _beatlineSet.EndingPhrase = GetEndingTimeInPhrase();
            _beatlineSet.Bpm = _gameSong.Bpm;
            _beatlineSet.SetSpeeds();

            if (Core.Cookies.ContainsKey("MenuMusicChannel"))
            {
                Core.Audio.StopChannel((int)Core.Cookies["MenuMusicChannel"]);
                Core.Cookies.Remove("MenuMusicChannel");
            }
            base.Initialize();
        }


        private bool LargeBeatlinesSuitable()
        {
            var result = !(Core.Players[0].Playing && Core.Players[1].Playing);
            result = result && !(Core.Players[2].Playing && Core.Players[3].Playing);
            return result;
        }

        #region Updating, Beatline maintenance

        private int _timeCheck;

        public override void Update(GameTime gameTime)
        {

            if (_startTime == null)
            {
                Core.Songs.PlaySong(_gameSong);
                _startTime = new TimeSpan(gameTime.TotalRealTime.Ticks);
            }

            CheckForEndings(gameTime);
            SyncSong();
            _phraseNumber = 1.0 * (gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay - _gameSong.Offset * 1000) / 1000 * (_gameSong.Bpm / 240);
            _timeCheck = (int)(gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay);
            _beatlineSet.MaintainBeatlineNotes(_phraseNumber);
            MaintainBlazings();
            RecordPlayerLife();
            base.Update(gameTime);
        }

        private void RecordPlayerLife()
        {
            if (_phraseNumber + 1 > _lastLifeRecord)
            {
                foreach (Player player in Core.Players)
                {
                    player.RecordCurrentLife();
                }
                _lastLifeRecord++;
            }
        }

        private void MaintainBlazings()
        {
            if (_phraseNumber - _lastBlazeCheck > 0.25)
            {
                _lastBlazeCheck += 0.25;
                for (int x = 0; x < 4; x++)
                {
                    if ((Core.Players[x].IsBlazing))
                    {
                        Core.Players[x].Life--;
                        if (Core.Players[x].Life < 100)
                        {
                            Core.Players[x].Life -= 25;
                            Core.Players[x].IsBlazing = false;
                            _notebars[x].CancelReverse();
                        }
                    }
                }
            }
        }

        private void CheckForEndings(GameTime gameTime)
        {
            if ((SongPassed() ||AllPlayersKOed()) && (_displayState == 0))
            {
                _displayState = 1;
                _transitionTime = gameTime.TotalRealTime.TotalSeconds + 3;
            }
            if ((_displayState == 1) && (gameTime.TotalRealTime.TotalSeconds >= _transitionTime))
            {
                if (Core.Settings.Get<bool>("SongDebug"))
                {
                    Core.Songs.SaveToFile(_gameSong);
                }

                Core.ScreenTransition("Evaluation");
            }
        }

        private bool AllPlayersKOed()
        {
            return (from e in Core.Players select e).All(e => (!e.Playing || e.KO));
        }

        private int _confidence;

        private void SyncSong()
        {

            if (_displayState != 0)
            {
                return;
            }

            //FMOD cannot reliably determine the position of the song. Using GetCurrentSongProgress()
            //as the default timing mechanism makes it jerky and slows the game down, so we attempt
            //to match current time with the song time by periodically sampling it. A hill climbing method
            // is used here.
            var delay = Core.Songs.GetCurrentSongProgress() - _timeCheck;
            if ((_confidence < 15) && (Math.Abs(delay) > 25))
            {
                _confidence = 0;
                _songLoadDelay += delay / 2.0;
            }
            else if (_confidence < 15)
            {
                _confidence++;
            }
        }

        #endregion

        #region Actions/Input
        /// <summary>
        /// Executes whenever the key has been pressed that corresponds to some action in the game.
        /// </summary>
        /// <param name="action">The Action that the user has requested.</param>
        public override void PerformAction(Action action)
        {
            var songDebug = Core.Settings.Get<bool>("SongDebug");
            int player = -1;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            if ((player > -1) && (Core.Players[player].CPU))
            {
                return;
            }

            switch (paction)
            {
                case "LEFT":
                case "RIGHT":
                case "UP":
                case "DOWN":
                    HitArrow(action, player);
                    break;

                case "BEATLINE":
                    HitBeatline(player);
                    break;
                case "SELECT":
                    ToggleBlazing(player);
                    break;
                case  "BPM_DECREASE":
                    if (songDebug)
                    {
                        _gameSong.Bpm -= 0.1;
                    }
                    break;
                case "BPM_INCREASE":
                    if (songDebug)
                    {
                        _gameSong.Bpm += 0.1;
                    }
                    break;
                case "OFFSET_DECREASE_BIG":
                    if (songDebug)
                    {
                        _gameSong.Offset -= 0.1;
                    }
                    break;
                case "OFFSET_INCREASE_BIG":
                    if (songDebug)
                    {
                        _gameSong.Offset += 0.1;
                    }
                    break;
                case "OFFSET_DECREASE_SMALL":
                    if (songDebug)
                    {
                        _gameSong.Offset -= 0.01;
                    }
                    break;
                case "OFFSET_INCREASE_SMALL":
                    if (songDebug)
                    {
                        _gameSong.Offset += 0.01;
                    }
                    break;
                case "LENGTH_DECREASE":
                    if (songDebug)
                    {
                        _gameSong.Length -= 0.1;
                    }
                    break;
                case "LENGTH_INCREASE":
                    if (songDebug)
                    {
                        _gameSong.Length += 0.1;
                    }
                    break;
                case "BACK":
                    Core.Songs.StopCurrentSong();
                    Core.ScreenTransition("SongSelect");
                    break;
            }

        }

        private void ToggleBlazing(int player)
        {
            switch ((GameType)Core.Cookies["CurrentGameType"])
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                case GameType.COOPERATIVE:
                    if (Core.Players[player].Life > 100)
                    {
                        Core.Players[player].IsBlazing = true;
                    }
                    break;
            }

        }

        private void HitArrow(Action action, int player)
        {
            if (player >= _playerCount)
            {
                return;
            }
            if ((Core.Players[player].KO) || (!Core.Players[player].Playing))
            {
                return;
            }

            if ((_notebars[player].CurrentNote() != null) && (Note.ActionToDirection(action) == _notebars[player].CurrentNote().Direction))
            {
                _notebars[player].MarkCurrentCompleted();
                Core.Players[player].Hits++;
            }
            else if (_notebars[player].CurrentNote() != null)
            {
                _notebars[player].ResetAll();

                _lifeBarSet.AdjustLife(Core.Players[player].MissedArrow(), player);
            }
        }

        private void HitBeatline(int player)
        {

            if ((Core.Players[player].KO) || (!Core.Players[player].Playing))
            {
                return;
            }

            _debugLastHitOffset = _beatlineSet.CalculateHitOffset(player, _phraseNumber);

            var complete = _notebars[player].AllCompleted();
            var judgement = _beatlineSet.AwardJudgement(_phraseNumber, player, complete);
            if (judgement == BeatlineNoteJudgement.COUNT)
            {
                return;
            }

            //Check if all notes in the notebar have been hit and act accordingly.
            if (complete)
            {
                //Increment Player Level
                Core.Players[player].Momentum += MomentumIncreaseByDifficulty(Core.Players[player].PlayDifficulty);
            }
            //Award Score
            ApplyJudgement(judgement, player);
            CreateNextNoteBar(player);


        }

        private void CreateNextNoteBar(int player)
        {
            //Create next note bar.
            var numArrow = (int)Core.Players[player].Level;
            var numReverse = (Core.Players[player].IsBlazing) ? (int)Core.Players[player].Level / 2 : 0;
            _notebars[player] = NoteBar.CreateNoteBar(numArrow, numReverse, Core.Metrics["NoteBar", player]);
        }

        #endregion

        #region Helper Methods

        private void ApplyJudgement(BeatlineNoteJudgement judgement, int player)
        {
            var lifeAdjust = _noteJudgementSet.AwardJudgement(judgement, player, _notebars[player].NumberCompleted() + _notebars[player].NumberReverse(), _notebars[player].Notes.Count - _notebars[player].NumberCompleted());
            _lifeBarSet.AdjustLife(lifeAdjust, player);
        }

        private static long MomentumIncreaseByDifficulty(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.BEGINNER:
                    return 15;
                case Difficulty.EASY:
                    return 40;
                case Difficulty.MEDIUM:
                    return 70;
                case Difficulty.HARD:
                    return 150;
                case Difficulty.INSANE:
                    return 200;
                default:
                    return 0;
            }
        }

        private string CalculateTimeLeft()
        {

           // var timeElapsed = gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds;
           // var timeLeft = _gameSong.Length * 1000 - timeElapsed;
            var timeLeft = _gameSong.Length*1000 - _timeCheck;
            var ts = new TimeSpan(0, 0, 0, 0, (int)timeLeft);

            return ts.Minutes + ":" + String.Format("{0:D2}", Math.Max(0, ts.Seconds));
        }

        private bool SongPassed()
        {
            //var timeElapsed = gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds;
           // var timeLeft = _gameSong.Length * 1000 - timeElapsed;
            var timeLeft = _gameSong.Length * 1000 - _timeCheck;
            return timeLeft <= 0.0;
        }


        private double GetEndingTimeInPhrase()
        {
            //Removed _songLoadDelay here
            return ((_gameSong.Length - _gameSong.Offset) * 1000) / 1000 * (_gameSong.Bpm / 240);
        }

        private void BeatlineNoteMissed(object sender, EventArgs e)
        {
            var player = ((Beatline)sender).Id;
            _lifeBarSet.AdjustLife(
                _noteJudgementSet.AwardJudgement(BeatlineNoteJudgement.MISS, player, 0, 0), player);
        }

        private void BeatlineNoteCPUHit(object sender, EventArgs e)
        {
            var player = ((Beatline)sender).Id;
            var judgement = Core.CPUManager.GetNextJudgement(2, Core.Players[player].Streak);

            _notebars[player].MarkAllCompleted();

            if ((judgement != BeatlineNoteJudgement.MISS) && (judgement != BeatlineNoteJudgement.FAIL))
            {
                Core.Players[player].Momentum += MomentumIncreaseByDifficulty(Core.Players[player].PlayDifficulty);
            }
            //Award Score
            ApplyJudgement(judgement, player);
            CreateNextNoteBar(player);
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawBorders(spriteBatch);

            //Draw the notebars.
            for (int x = 0; x < 4; x++)
            {
                if (Core.Players[x].Playing)
                {
                    _notebars[x].Draw(spriteBatch);
                }
            }

            //Draw the component sets.
            _lifeBarSet.Draw(spriteBatch, _phraseNumber);
            _levelbarSet.Draw(spriteBatch);
            _hitsbarSet.Draw(spriteBatch);
            _scoreSet.Draw(spriteBatch);
            _noteJudgementSet.Draw(spriteBatch, _phraseNumber);
            _beatlineSet.Draw(spriteBatch, _phraseNumber);

            if (_phraseNumber < 0)
            {
                DrawCountdowns(spriteBatch);
            }

            DrawKOIndicators(spriteBatch);
            DrawSongInfo(spriteBatch);
            DrawClearIndicators(spriteBatch);
            DrawText(spriteBatch);

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
        }

        private readonly double[] _threshholds = { -1.00, -0.75, -0.5, -0.25, 0.0 };

        private void DrawCountdowns(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < Core.Players.Count(); x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                var countdownSpriteMap = new SpriteMap
                                             {
                                                 Columns = 1,
                                                 Rows = 5,
                                                 SpriteTexture = TextureManager.Textures["countdown"]
                                             };

                for (int y = 0; y < _threshholds.Count(); y++)
                {
                    if (_phraseNumber < _threshholds[y])
                    {
                        countdownSpriteMap.ColorShading.A = (byte)Math.Min(255, (_threshholds[y] - _phraseNumber) * 255 * 4);
                        countdownSpriteMap.Draw(spriteBatch, y, 200, 60, Core.Metrics["Countdown", x]);
                        break;
                    }
                }

            }
        }

        private void DrawClearIndicators(SpriteBatch spriteBatch)
        {
            if (_displayState != 1)
            {
                return;
            }
            for (int x = 0; x < _playerCount; x++)
            {
                if ((!Core.Players[x].KO) && (Core.Players[x].Playing))
                {
                    var koSprite = new Sprite
                    {
                        Height = 150,
                        Width = 250,
                        SpriteTexture = TextureManager.Textures["stageClearIndicator"]
                    };
                    koSprite.SetPosition(Core.Metrics["KOIndicator", x]);
                    koSprite.Draw(spriteBatch);
                }
            }
        }


        private void DrawSongInfo(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + CalculateTimeLeft()
, Core.Metrics["SongTimeLeft", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + _gameSong.Title
, Core.Metrics["SongTitle", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + _gameSong.Artist
, Core.Metrics["SongArtist", 0], Color.Black);
        }

        private void DrawKOIndicators(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _playerCount; x++)
            {
                if (Core.Players[x].KO)
                {
                    var koSprite = new Sprite
                    {
                        Height = 150,
                        Width = 250,
                        SpriteTexture = TextureManager.Textures["koIndicator"]
                    };
                    koSprite.SetPosition(Core.Metrics["KOIndicator", x]);
                    koSprite.Draw(spriteBatch);
                }
            }
        }


        private void DrawText(SpriteBatch spriteBatch)
        {

            if (Core.Settings.Get<bool>("SongDebug"))
            {
                DrawDebugText(spriteBatch);
            }
        }

        private void DrawDebugText(SpriteBatch spriteBatch)
        {

            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("BPM: {0:F2}", _gameSong.Bpm),
                   Core.Metrics["SongDebugBPM", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Offset: {0:F3}", _gameSong.Offset),
                    Core.Metrics["SongDebugOffset", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + String.Format("{0:F3}", _phraseNumber), Core.Metrics["SongDebugPhrase", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Hitoffset: {0:F3}", _debugLastHitOffset),
                       Core.Metrics["SongDebugHitOffset", 0], Color.Black);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Length: {0:F3}", _gameSong.Length),
           Core.Metrics["SongDebugLength", 0], Color.Black);
        }

        private void DrawBorders(SpriteBatch spriteBatch)
        {
            var brush = new PrimitiveLine(Core.GraphicsDevice) { Colour = Color.Black };

            brush.ClearVectors();
            brush.AddVector(new Vector2(0, 275));
            brush.AddVector(new Vector2(800, 275));
            brush.Render(spriteBatch);

            brush.ClearVectors();
            brush.AddVector(new Vector2(0, 325));
            brush.AddVector(new Vector2(800, 325));
            brush.Render(spriteBatch);

            brush.ClearVectors();

        }

        #endregion
    }
}
