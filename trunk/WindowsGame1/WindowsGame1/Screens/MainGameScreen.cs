using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Drawing.Sets;
using WGiBeat.Managers;
using WGiBeat.Notes;

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
        private CountdownSet _countdownSet;

        private NoteBar[] _notebars;
        private PerformanceBar _performanceBar;
        private const int PLAYER_COUNT = 4;
        private GameSong _gameSong;
        private TimeSpan? _startTime;
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
            _notebars = new NoteBar[PLAYER_COUNT];
            _performanceBar = new PerformanceBar
                                  {Width = 350, Players = Core.Players};
            var freeLocation = _performanceBar.GetFreeLocation((GameType) Core.Cookies["CurrentGameType"] == GameType.COOPERATIVE);
            _performanceBar.Position = Core.Metrics["PerformanceBar", freeLocation];
            _lifeBarSet = new LifeBarSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _levelbarSet = new LevelBarSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _hitsbarSet = new HitsBarSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _scoreSet = new ScoreSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _noteJudgementSet = new NoteJudgementSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _countdownSet = new CountdownSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);
            _beatlineSet = new BeatlineSet(Core.Metrics, Core.Players, (GameType)Core.Cookies["CurrentGameType"]);

            _beatlineSet.NoteMissed += BeatlineNoteMissed;
            _beatlineSet.CPUNoteHit += BeatlineNoteCPUHit;
            _beatlineSet.Large = LargeBeatlinesSuitable();

            _displayState = 0;
            _songLoadDelay = 0.0;
            _confidence = 0;
            _lastBlazeCheck = 0;
            _lastLifeRecord = -0.5;
            
            for (int x = 0; x < PLAYER_COUNT; x++)
            {

                _lifeBarSet.Reset();
                Core.Players[x].ResetStats();

                _notebars[x] = NoteBar.CreateNoteBar((int)Core.Players[x].Level, 0);
                _notebars[x].Position = (Core.Metrics["NoteBar", x]);

            }

            _gameSong = (GameSong)Core.Cookies["CurrentSong"];

            _startTime = null;
            _beatlineSet.EndingPhrase = _gameSong.GetEndingTimeInPhrase();
            _beatlineSet.Bpm = _gameSong.Bpm;
            _beatlineSet.SetSpeeds();

            InitSprites();

            base.Initialize();
        }

        private void InitSprites()
        {
            _clearSprite = new Sprite
                               {
                                   SpriteTexture = TextureManager.Textures("StageClearIndicator")
                               };
            _koSprite = new Sprite
                            {
                                SpriteTexture = TextureManager.Textures("KOIndicator")
                            };
        }

        private bool LargeBeatlinesSuitable()
        {
            var result = !(Core.Players[0].Playing && Core.Players[1].Playing);
            result = result && !(Core.Players[2].Playing && Core.Players[3].Playing);
            return result;
        }

        #region Updating, Beatline maintenance

        private int _timeElapsed;

        public override void Update(GameTime gameTime)
        {

            if (_startTime == null)
            {
                Core.Songs.PlaySong(_gameSong);
                _startTime = new TimeSpan(gameTime.TotalRealTime.Ticks);
            }
            _timeElapsed = (int)(gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay);
            _phraseNumber = _gameSong.ConvertMSToPhrase(_timeElapsed);
            CheckForEndings(gameTime);
            SyncSong();
            _beatlineSet.MaintainBeatlineNotes(_phraseNumber);
            MaintainBlazings();
            MaintainCPUArrows();
            RecordPlayerLife();
            RecordPlayerPlayTime(gameTime.ElapsedRealTime.TotalMilliseconds);
            base.Update(gameTime);
        }

        private void MaintainCPUArrows()
        {
            for (int x = 0; x < 3; x++)
            {
                if (!Core.Players[x].IsCPUPlayer)
                {
                    continue;
                }

                var nextHit = 1.0 * (_notebars[x].NumberCompleted() + 1) / (_notebars[x].Notes.Count() + 1);
                var phraseDecimal = _phraseNumber - Math.Floor(_phraseNumber);

                if (phraseDecimal > nextHit)
                {
                    _notebars[x].MarkCurrentCompleted();
                    Core.Players[x].Hits++;
                }
                
            }
        }

        private void RecordPlayerPlayTime(double milliseconds)
        {
            foreach (Player player in Core.Players)
            {
                if (player.IsHumanPlayer)
                {
                    player.PlayTime += (long) milliseconds;
                }
            }
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
            double minBlazingAmount = 0;

            switch ((GameType)Core.Cookies["CurrentGameType"])
            {
                case GameType.NORMAL:
                case GameType.TEAM:
                    minBlazingAmount = 100;
                    break;
                case GameType.COOPERATIVE:
                    minBlazingAmount = 60;
                    break;
            }

            if (_phraseNumber - _lastBlazeCheck > 0.25)
            {
                _lastBlazeCheck += 0.25;
                for (int x = 0; x < 4; x++)
                {
                    if ((Core.Players[x].IsBlazing))
                    {
                        Core.Players[x].Life--;

                        if (Core.Players[x].Life < minBlazingAmount)
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
        private Sprite _koSprite;
        private Sprite _clearSprite;

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
            var delay = Core.Songs.GetCurrentSongProgress() - _timeElapsed;
            if ((Math.Abs(delay) > 20))
            {
                System.Diagnostics.Debug.WriteLine(delay);
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
        /// Executes whenever the key has been pressed that corresponds to some inputAction in the game.
        /// </summary>
        /// <param name="inputAction">The Action that the user has requested.</param>
        public override void PerformAction(InputAction inputAction)
        {
            var songDebug = Core.Settings.Get<bool>("SongDebug");

            if ((inputAction.Player > 0) && (Core.Players[inputAction.Player - 1].CPU))
            {
                return;
            }

            switch (inputAction.Action)
            {
                case "LEFT":
                case "RIGHT":
                case "UP":
                case "DOWN":
                    HitArrow(inputAction);
                    break;

                case "BEATLINE":
                    HitBeatline(inputAction.Player - 1);
                    break;
                case "SELECT":
                    ToggleBlazing(inputAction.Player - 1);
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
                    if (Core.Players[player].Life > 100)
                    {
                        Core.Players[player].IsBlazing = true;
                    }
                    break;
                case GameType.COOPERATIVE:
                    if (Core.Players[player].Life > 60)
                    {
                        Core.Players[player].IsBlazing = true;
                    }
                    break;
            }

        }

        private void HitArrow(InputAction inputAction)
        {
            var player = inputAction.Player - 1;
            if ((Core.Players[player].KO) || (!Core.Players[player].Playing))
            {
                return;
            }

            if ((_notebars[player].CurrentNote() != null) && (Note.ActionToDirection(inputAction) == _notebars[player].CurrentNote().Direction))
            {
                _notebars[player].MarkCurrentCompleted();
                Core.Players[player].Hits++;
                Core.Players[player].TotalHits++;
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
            var timeLeft = _gameSong.Length*1000 - _timeElapsed;
            var ts = new TimeSpan(0, 0, 0, 0, (int)timeLeft);

            return ts.Minutes + ":" + String.Format("{0:D2}", Math.Max(0, ts.Seconds));
        }

        private bool SongPassed()
        {
            //var timeElapsed = gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds;
           // var timeLeft = _gameSong.Length * 1000 - timeElapsed;
            var timeLeft = _gameSong.Length * 1000 - _timeElapsed;
            return timeLeft <= 0.0;
        }

        private void BeatlineNoteMissed(object sender, EventArgs e)
        {
            var player = ((Beatline)sender).Id;
            _lifeBarSet.AdjustLife(
                _noteJudgementSet.AwardJudgement(BeatlineNoteJudgement.MISS, player, 0, 0), player);
        }

        private void BeatlineNoteCPUHit(object sender, EventArgs e)
        {
            var player = (int) sender;
            var judgement = Core.CPUManager.GetNextJudgement(Core.Cookies["CPUSkillLevel"].ToString(), Core.Players[player].Streak);

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
            _performanceBar.Draw(spriteBatch);

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
                SpriteTexture = TextureManager.Textures("MainGameScreenBackground"),
                Width = Core.Window.ClientBounds.Width,
            };
            background.Draw(spriteBatch);
        }

        private void DrawCountdowns(SpriteBatch spriteBatch)
        {
            _countdownSet.Draw(spriteBatch, _phraseNumber);
        }

        private void DrawClearIndicators(SpriteBatch spriteBatch)
        {
            if (_displayState != 1)
            {
                return;
            }
            for (int x = 0; x < PLAYER_COUNT; x++)
            {
                if ((!Core.Players[x].KO) && (Core.Players[x].Playing))
                {
                    _clearSprite.Position = (Core.Metrics["KOIndicator", x]);
                    _clearSprite.Draw(spriteBatch);
                }
            }
        }

        private void DrawKOIndicators(SpriteBatch spriteBatch)
        {

            for (int x = 0; x < PLAYER_COUNT; x++)
            {
                if (Core.Players[x].KO)
                {
                    _koSprite.Position = (Core.Metrics["KOIndicator", x]);
                    _koSprite.Draw(spriteBatch);
                }
            }
        }

        private void DrawSongInfo(SpriteBatch spriteBatch)
        {
            TextureManager.DrawString(spriteBatch, "" + CalculateTimeLeft(), "DefaultFont",
                Core.Metrics["SongTimeLeft", 0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, _gameSong.Title, "DefaultFont",
                Core.Metrics["SongTitle", 0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, _gameSong.Artist, "DefaultFont", 
                Core.Metrics["SongArtist", 0], Color.Black, FontAlign.LEFT);
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
            TextureManager.DrawString(spriteBatch,String.Format("{0:F3}", _phraseNumber), "DefaultFont", Core.Metrics["SongDebugPhrase", 0], Color.Black,FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("BPM: {0:F2}", _gameSong.Bpm),
       "DefaultFont", Core.Metrics["SongDebugBPM", 0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("Offset: {0:F3}", _gameSong.Offset),
                    "DefaultFont", Core.Metrics["SongDebugOffset", 0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("Hitoffset: {0:F3}", _debugLastHitOffset),
                "DefaultFont", Core.Metrics["SongDebugHitOffset", 0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("Length: {0:F3}", _gameSong.Length),
                "DefaultFont", Core.Metrics["SongDebugLength", 0], Color.Black, FontAlign.LEFT);
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
