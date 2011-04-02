using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Drawing.Sets;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Screens
{
    public class MainGameScreen : GameScreen
    {

        private double _phraseNumber;
        private double _debugLastHitOffset;
        private double _songLoadDelay;

        private LifeBarSet _lifeBarSet;
        private LevelBarSet _levelbarSet;
        private HitsBarSet _hitsBarSet;
        private ScoreSet _scoreSet;
        private NoteJudgementSet _noteJudgementSet;
        private NoteBarSet _noteBarSet;
        private BeatlineSet _beatlineSet;
        private CountdownSet _countdownSet;

        private PerformanceBar _performanceBar;
        private const int PLAYER_COUNT = 4;
        private GameSong _gameSong;
        private TimeSpan? _startTime;
        private int _displayState;
        private double _transitionTime;
        private double _lastLifeRecord;

        private Sprite _koSprite;
        private Sprite _clearSprite;
        private Sprite _background;

        public MainGameScreen(GameCore core)
            : base(core)
        {
        }

        public override void Initialize()
        {

            var currentGameType = (GameType) Core.Cookies["CurrentGameType"];
            _performanceBar = new PerformanceBar
                                  {Width = 350, Players = Core.Players};
            var freeLocation = _performanceBar.GetFreeLocation(currentGameType == GameType.COOPERATIVE);
            _performanceBar.Position = Core.Metrics["PerformanceBar", freeLocation];
            _lifeBarSet = new LifeBarSet(Core.Metrics, Core.Players, currentGameType);
            _lifeBarSet.BlazingEnded += ((sender, e) => _noteBarSet.CancelReverse((int) e.Object));

            _levelbarSet = new LevelBarSet(Core.Metrics, Core.Players, currentGameType);
            _hitsBarSet = new HitsBarSet(Core.Metrics, Core.Players, currentGameType);
            _scoreSet = new ScoreSet(Core.Metrics, Core.Players, currentGameType);
            _noteJudgementSet = new NoteJudgementSet(Core.Metrics, Core.Players, currentGameType,_lifeBarSet,_scoreSet);
            _countdownSet = new CountdownSet(Core.Metrics, Core.Players, currentGameType);
            _beatlineSet = new BeatlineSet(Core.Metrics, Core.Players, currentGameType);
            _noteBarSet = new NoteBarSet(Core.Metrics, Core.Players, currentGameType);
            _noteBarSet.PlayerFaulted += (PlayerFaulted);
            _noteBarSet.PlayerArrowHit += (PlayerArrowHit);

            _beatlineSet.NoteMissed += BeatlineNoteMissed;
            _beatlineSet.CPUNoteHit += BeatlineNoteCPUHit;
            _beatlineSet.Large = LargeBeatlinesSuitable();
            
            _displayState = 0;
            _songLoadDelay = 0.0;
            _lastLifeRecord = -0.5;
            
            for (int x = 0; x < PLAYER_COUNT; x++)
            {

                _lifeBarSet.Reset();
                Core.Players[x].ResetStats();

            }
            _noteBarSet.InitNoteBars();

            _gameSong = (GameSong)Core.Cookies["CurrentSong"];
            _beatlineSet.AddBPMChangeMarkers(_gameSong);

            _startTime = null;
            _panic = Core.Cookies.ContainsKey("Panic");
            _beatlineSet.EndingPhrase = _gameSong.GetEndingTimeInPhrase();
            _beatlineSet.Bpm = _gameSong.CurrentBPM(0.0);
            _beatlineSet.SetSpeeds();

            InitSprites();

            base.Initialize();
        }

        private void PlayerFaulted(object sender, EventArgs e)
        {
            var player = (int) sender;
            _lifeBarSet.AdjustLife(Core.Players[player].MissedArrow(), player);
            _hitsBarSet.ResetHits(player);
        }

        private void PlayerArrowHit(object sender, EventArgs e)
        {
            var player = (int) sender;
            _hitsBarSet.IncrementHits(1,player);
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
            _background = new Sprite
            {
                Height = 600,
                Width = 800,
                SpriteTexture = TextureManager.Textures("MainGameScreenBackground"),
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
            _beatlineSet.Bpm = _gameSong.CurrentBPM(_phraseNumber);
            _beatlineSet.MaintainBeatlineNotes(_phraseNumber);
            _lifeBarSet.MaintainBlazings(_phraseNumber);
            _noteBarSet.MaintainCPUArrows(_phraseNumber);
            RecordPlayerLife();
            RecordPlayerPlayTime(gameTime.ElapsedRealTime.TotalMilliseconds);
            base.Update(gameTime);
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

        private bool _panic;

        private void SyncSong()
        {

            if ((_displayState != 0) || (_panic) || (!Core.Songs.IsCurrentSongPlaying()))
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
                _songLoadDelay += delay / 2.0;
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
                    _noteBarSet.HitArrow(inputAction);
                    break;

                case "BEATLINE":
                    HitBeatline(inputAction.Player - 1);
                    break;
                case "SELECT":
                    _lifeBarSet.ToggleBlazing(inputAction.Player - 1);
                    break;
                case  "BPM_DECREASE":
                    if (songDebug)
                    {
                        _gameSong.SetCurrentBPM(_phraseNumber, _gameSong.CurrentBPM(_phraseNumber) - 0.1);
                    }
                    break;
                case "BPM_INCREASE":
                    if (songDebug)
                    {
                        _gameSong.SetCurrentBPM(_phraseNumber, _gameSong.CurrentBPM(_phraseNumber) + 0.1);
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


        private void HitBeatline(int player)
        {

            if ((Core.Players[player].KO) || (!Core.Players[player].Playing))
            {
                return;
            }

            _debugLastHitOffset = _beatlineSet.CalculateHitOffset(player, _phraseNumber);

            var complete = _noteBarSet.AllCompleted(player);
            var judgement = _beatlineSet.AwardJudgement(_phraseNumber, player, complete);
            if (judgement == BeatlineNoteJudgement.COUNT)
            {
                return;
            }

            //Check if all notes in the notebar have been hit and act accordingly.
            if (complete)
            {
                //Increment Player Level
                Core.Players[player].Momentum += (long)(MomentumJudgementMultiplier(judgement) * MomentumIncreaseByDifficulty(Core.Players[player].PlayerOptions.PlayDifficulty));
            }
            //Award Score
            ApplyJudgement(judgement, player);
            _noteBarSet.CreateNextNoteBar(player);

        }

        private double MomentumJudgementMultiplier(BeatlineNoteJudgement judgement)
        {
            switch (judgement)
            {
                case BeatlineNoteJudgement.IDEAL:
                    return 1.0;
                    case BeatlineNoteJudgement.COOL:
                    return 2.0/3;
                    case BeatlineNoteJudgement.OK:
                    return 1.0/3;
                case BeatlineNoteJudgement.BAD:
                    return 0.0;
            }
            return 0.0;
        }



        #endregion

        #region Helper Methods

        private void ApplyJudgement(BeatlineNoteJudgement judgement, int player)
        {
            _noteJudgementSet.AwardJudgement(judgement, player, _noteBarSet.NumberCompleted(player),
                                                              _noteBarSet.NumberIncomplete(player));
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
                    return 175;
                case Difficulty.INSANE:
                    return 300;
                default:
                    return 0;
            }
        }

        private string CalculateTimeLeft()
        {
            var timeLeft = _gameSong.Length*1000 - _timeElapsed;
            var ts = new TimeSpan(0, 0, 0, 0, (int)timeLeft);

            return ts.Minutes + ":" + String.Format("{0:D2}", Math.Max(0, ts.Seconds));
        }

        private bool SongPassed()
        {

            var timeLeft = _gameSong.Length * 1000 - _timeElapsed;
            return timeLeft <= 0.0;
        }

        private void BeatlineNoteMissed(object sender, EventArgs e)
        {
            var player = (int) sender;

       _noteJudgementSet.AwardJudgement(BeatlineNoteJudgement.MISS, player, 0, 0);
            if (Core.Players[player].CPU)
            {
                Core.Players[player].NextCPUJudgement =
                    Core.CPUManager.GetNextJudgement(Core.Cookies["CPUSkillLevel"].ToString(),
                                                     Core.Players[player].Streak);
            }
        }

        private void BeatlineNoteCPUHit(object sender, EventArgs e)
        {
            var player = (int) sender;
            var judgement = Core.Players[player].NextCPUJudgement;

            if (judgement == BeatlineNoteJudgement.COUNT)
            {
                judgement = Core.CPUManager.GetNextJudgement(Core.Cookies["CPUSkillLevel"].ToString(), Core.Players[player].Streak);
            }

            _noteBarSet.MarkAllCompleted(player);

            if ((judgement != BeatlineNoteJudgement.MISS) && (judgement != BeatlineNoteJudgement.FAIL))
            {
                Core.Players[player].Momentum += (long)(MomentumJudgementMultiplier(judgement) * MomentumIncreaseByDifficulty(Core.Players[player].PlayerOptions.PlayDifficulty));
            }

            //Award Score
            ApplyJudgement(judgement, player);
            _noteBarSet.CreateNextNoteBar(player);
            Core.Players[player].NextCPUJudgement = Core.CPUManager.GetNextJudgement(Core.Cookies["CPUSkillLevel"].ToString(), Core.Players[player].Streak);
            
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawBorders(spriteBatch);

            //Draw the notebars.
          _noteBarSet.Draw(spriteBatch);

            //Draw the component sets.
            _lifeBarSet.Draw(spriteBatch, _phraseNumber);
            _levelbarSet.Draw(spriteBatch);
            _hitsBarSet.Draw(spriteBatch);
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
            _background.Draw(spriteBatch);
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
            var scale = TextureManager.ScaleTextToFit(_gameSong.Title, "DefaultFont", 310, 100);
            TextureManager.DrawString(spriteBatch, _gameSong.Title, "DefaultFont",
                Core.Metrics["SongTitle", 0], scale, Color.Black, FontAlign.LEFT);
            scale = TextureManager.ScaleTextToFit(_gameSong.Artist, "DefaultFont", 310, 100);
            TextureManager.DrawString(spriteBatch, _gameSong.Artist, "DefaultFont", 
                Core.Metrics["SongArtist", 0],scale, Color.Black, FontAlign.LEFT);
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
            TextureManager.DrawString(spriteBatch, String.Format("BPM: {0:F2}", _beatlineSet.Bpm),
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
