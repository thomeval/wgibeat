using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Drawing.Sets;
using WGiBeat.Helpers;
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
        private BeatlineHitAggregator _hitAggregator;
        private VisualizerBackground _visualBackground;
        private RecordReplayer _recordReplayer;
        private SongTimeLine _songTimeLine;

        private PerformanceBar _performanceBar;
        private GrooveMomentumBarSet _gmBarSet;

        private const int PLAYER_COUNT = 4;
        private GameSong _gameSong;
        private TimeSpan? _startTime;
        private int _displayState;
        private double _transitionTime;
        private double _lastLifeRecord;

        private Sprite3D _koSprite;
        private Sprite3D _clearSprite;
        private Sprite3D _background;
        private Sprite3D _textBackground;

        public MainGameScreen(GameCore core)
            : base(core)
        {
        }

        public override void Initialize()
        {
            _gameSong = (GameSong)Core.Cookies["CurrentSong"];
            var currentGameType = (GameType) Core.Cookies["CurrentGameType"];
            _performanceBar = new PerformanceBar
                                  { Metrics = Core.Metrics, Players = Core.Players, GameType = currentGameType};
            _performanceBar.SetPosition();
          
           
            _lifeBarSet = new LifeBarSet(Core.Metrics, Core.Players, currentGameType);
            _lifeBarSet.BlazingEnded += ((sender, e) => _noteBarSet.CancelReverse((int) e.Object));

            _levelbarSet = new LevelBarSet(Core.Metrics, Core.Players, currentGameType);
            _hitsBarSet = new HitsBarSet(Core.Metrics, Core.Players, currentGameType);
            _scoreSet = new ScoreSet(Core.Metrics, Core.Players, currentGameType);
            _noteJudgementSet = new NoteJudgementSet(Core.Metrics, Core.Players, currentGameType,_lifeBarSet,_scoreSet);
            _countdownSet = new CountdownSet(Core.Metrics, Core.Players, currentGameType);
            _beatlineSet = new BeatlineSet(Core.Metrics, Core.Players, currentGameType);
            _gmBarSet = new GrooveMomentumBarSet(Core.Metrics,Core.Players,currentGameType);
            _hitAggregator = new BeatlineHitAggregator(Core.Players, currentGameType);
            _hitAggregator.HitsAggregated += HitsAggregated;
            _noteBarSet = new NoteBarSet(Core.Metrics, Core.Players, currentGameType);
            _noteBarSet.PlayerFaulted += (PlayerFaulted);
            _noteBarSet.PlayerArrowHit += (PlayerArrowHit);
            _songTimeLine = new SongTimeLine {Position = new Vector2(365, 530), Size = new Vector2(370, 80)};

            _beatlineSet.NoteMissed += BeatlineNoteMissed;
            _beatlineSet.CPUNoteHit += BeatlineNoteCPUHit;
            _recordReplayer = new RecordReplayer(currentGameType);
            _recordReplayer.LoadRecord(_gameSong.GetHashCode(), currentGameType);
            _recordReplayer.Position = Core.Metrics["RecordReplayer",GetFreeLocation()];
            _recordReplayer.Size = Core.Metrics["RecordReplayer.Size",0];
            _displayState = 0;
            _songLoadDelay = 0.0;
            _lastLifeRecord = 0.5;
            _lastUpdate = 0.0;
            
            for (int x = 0; x < PLAYER_COUNT; x++)
            {

                _lifeBarSet.Reset();
                Core.Players[x].ResetStats();

            }
            _noteBarSet.InitNoteBars();

            
            
            _startTime = null;
            _dspActive = false;
            _panic = Core.Cookies.ContainsKey("Panic");
            _beatlineSet.EndingPhrase = _gameSong.GetEndingTimeInPhrase();
            _beatlineSet.Bpm = _gameSong.CurrentBPM(0.0);
            _beatlineSet.AddTimingPointMarkers(_gameSong);
            _beatlineSet.SetSpeeds();
            _noteBarSet.Visible = true;
            _visualBackground = new VisualizerBackground
                                    {
                                        Size = new Vector2(800, 600),
                                        AudioManager = Core.Audio,
                                        Position = new Vector2(0, 0),
                                        SongChannel = (int)Core.Cookies["GameSongChannel"],
                                        MaxBrightness = Convert.ToByte(Core.Settings["BackgroundAnimation"])
                                    };
            InitSprites();

            base.Initialize();
        }

        private int GetFreeLocation()
        {           
            for (int x = 3; x > -1; x--)
            {
                if (!Core.Players[x].Playing)
                {
                    return x;                    
                }
            }

            return 4;
        }

        private void HitsAggregated(object sender, ObjectEventArgs e)
        {
            var ar = (AggregatorResponse) e.Object;

            switch (ar.Player)
            {
                case AggregatorPlayerID.ALL:
                    for (int x = 0; x < 4; x++ )
                    {
                            ApplyJudgement(ar.Judgement,x,ar.Multiplier);
                    }
                    //GameManager.ApplyJudgement(ar.Judgement,x,ar.Multiplier);
                        break;
                    default:
                        ApplyJudgement(ar.Judgement, (int)ar.Player, ar.Multiplier);
                    break;
            }
            if (ar.Judgement == BeatlineNoteJudgement.MISS)
            {
                if (ar.Player ==  AggregatorPlayerID.ALL)
                {
                     _noteBarSet.TruncateNotes(0, (int)Core.Players[0].Level);
                }
                else
                {
                    _noteBarSet.TruncateNotes((int)ar.Player, (int)Core.Players[(int)ar.Player].Level);
                }
            }
            else
            {
                _noteBarSet.CreateNextNoteBar((int)ar.Player, IsNextNoteSuper((int) ar.Player));
            }
         
        }

        private void PlayerFaulted(object sender, EventArgs e)
        {
            var player = (int) sender;
            var gameType = (GameType) Core.Cookies["CurrentGameType"];
            //TODO: Life adjustments and momentum adjustments are handled inconsistently. Refactor
            if (gameType == GameType.SYNC_PRO || gameType == GameType.SYNC_PLUS)
            {
                var numPlayers = (from e2 in Core.Players where e2.Playing select e2).Count();
                    _lifeBarSet.AdjustLife(Core.Players[player].MissedArrow() / numPlayers, 0); 
            }
            else
            {
                _lifeBarSet.AdjustLife(Core.Players[player].MissedArrow(), player);  
            }

            
            _levelbarSet.AdjustForFault(player);
            _noteBarSet.TruncateNotes(player, (int)Core.Players[player].Level);
            _hitsBarSet.ResetHits(player);
        }

        private void PlayerArrowHit(object sender, EventArgs e)
        {
            var player = (int) sender;
            _hitsBarSet.IncrementHits(1,player);
        }

        private void InitSprites()
        {
            _clearSprite = new Sprite3D
                               {
                                   Texture = TextureManager.Textures("StageClearIndicator")
                               };
            _koSprite = new Sprite3D
                            {
                                Texture = TextureManager.Textures("KOIndicator")
                            };

            if (File.Exists(_gameSong.Path + "\\" + _gameSong.BackgroundFile))
            {
                TextureManager.CreateAndAddTexture(_gameSong.Path + "\\" + _gameSong.BackgroundFile, "SongBackground");
                _background = new Sprite3D { Height = 600, Width = 800, Texture = TextureManager.Textures("SongBackground") };
            }
            else
            {
                _background = new Sprite3D
                {
                    Height = 600,
                    Width = 800,
                    Texture = TextureManager.Textures("MainGameScreenBackground"),
                };            
            }

            _textBackground = new Sprite3D
                                  {
                                      Texture = TextureManager.Textures("MainGameTextBackground"),
                                      Position = Core.Metrics["MainGameTextBackground", 0]
                                  };
        }

        #region Updating, Beatline maintenance

        private int _timeElapsed;

        public override void Update(GameTime gameTime)
        {

            if (_startTime == null)
            {
                Core.Songs.PlayCachedSong((int) Core.Cookies["GameSongChannel"]);
                _startTime = new TimeSpan(gameTime.TotalRealTime.Ticks);
               

            }
            _timeElapsed = (int)(gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay);
            
            _phraseNumber = _gameSong.ConvertMSToPhrase(_timeElapsed);
            CheckForEndings(gameTime);
            SyncSong();
            _beatlineSet.Bpm = _gameSong.CurrentBPM(_phraseNumber);
            _beatlineSet.MaintainBeatlineNotes(_phraseNumber);
            _lifeBarSet.MaintainBlazings(_phraseNumber);
            MaintainBlazingDSP();
            var phraseDecimal = _beatlineSet.GetPhraseDecimal(_phraseNumber);
            
            _noteBarSet.MaintainCPUArrows(phraseDecimal);
            MaintainGrooveMomentum(_phraseNumber);
            RecordPlayerLife();
            RecordPlayerPlayTime(gameTime.ElapsedRealTime.TotalMilliseconds);
            base.Update(gameTime);
        }

        private bool _dspActive;
        private float _dspIntensity = 1;
        private double _desiredIntensity;
        private int _dspLifePeak = 100;
        private float DSP_UPDATE_SPEED = 0.125f;
        private void MaintainBlazingDSP()
        {

            var intensityMax = Convert.ToDouble(Core.Settings["BlazingBassBoost"],
                                                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            if (!_dspActive)
            {
                Core.Audio.ActivateDSP((int) Core.Cookies["GameSongChannel"]);
                _dspActive = true;
            }

            var playerAdjustedLife = (Core.Players.Where(e => e.Playing).Select(e => e.IsBlazing ? e.Life - 100 : 0));
            var adjustmentFactor = Math.Min(1, playerAdjustedLife.Average()/_dspLifePeak);
            //This should be 1 if no one is blazing. Otherwise somewhere between 1 and intensityMax. IntensityMax if all players are
            //blazing and at or above _dspLifePeak.
            _desiredIntensity = ((intensityMax - 1) * adjustmentFactor) + 1;

            double delta;
            if (_desiredIntensity > _dspIntensity)
            {
                delta = DSP_UPDATE_SPEED * TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds;
                _dspIntensity = (float)Math.Min(_desiredIntensity, delta + _dspIntensity);
            }
            else

            {
                delta = -1*DSP_UPDATE_SPEED*TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds;
                _dspIntensity = (float) Math.Max(_desiredIntensity, delta + _dspIntensity);
            }      
                
      
            Core.Audio.SetDSPIntensity(_dspIntensity);
        }

        private double _lastUpdate;
        private void MaintainGrooveMomentum(double phraseNumber)
        {
            // TODO: Change to milliseconds?
            if (_phraseNumber < 0.0)
            {
                return;
            }
            var diff = phraseNumber - _lastUpdate;
            var amount = 0.25*diff;
            
  Player.GrooveMomentum = ((Player.GrooveMomentum - 0.5)*(1 - amount)) + 0.5;
                
            _lastUpdate = phraseNumber;
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
            if (_phraseNumber <= _lastLifeRecord)
            {
                return;
            }
            foreach (Player player in Core.Players)
            {
                player.RecordCurrentLife();
            }
            _lastLifeRecord++;
        }


        private void CheckForEndings(GameTime gameTime)
        {
            if ((SongPassed() ||AllPlayersKOed()) && (_displayState == 0))
            {
                _displayState = 1;
                _transitionTime = gameTime.TotalRealTime.TotalSeconds + 3;
                _noteBarSet.Visible = false;
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
                case "BPM_DECREASE_SMALL":
                    if (songDebug)
                    {
                        _gameSong.SetCurrentBPM(_phraseNumber, _gameSong.CurrentBPM(_phraseNumber) - 0.01);
                    }
                    break;
                case "BPM_INCREASE_SMALL":
                    if (songDebug)
                    {
                        _gameSong.SetCurrentBPM(_phraseNumber, _gameSong.CurrentBPM(_phraseNumber) + 0.01);
                    }
                    break;
                case  "BPM_DECREASE_BIG":
                    if (songDebug)
                    {
                        _gameSong.SetCurrentBPM(_phraseNumber, _gameSong.CurrentBPM(_phraseNumber) - 0.1);
                    }
                    break;
                case "BPM_INCREASE_BIG":
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

            //Award Score
            _hitAggregator.RegisterHit(player,judgement);

        }


        #endregion

        #region Helper Methods

        private void ApplyJudgement(BeatlineNoteJudgement judgement, int player, int multiplier)
        {
            _noteJudgementSet.AwardJudgement(judgement, player, multiplier, _noteBarSet.NumberCompleted(player) + _noteBarSet.NumberReverse(player));
            _levelbarSet.AdjustMomentum(judgement, player);
            
            //Keep scoring sane by adding a delay to the awarding of Groove Momentum.
            //Otherwise, the last player to hit each phrase has an advantage.
            if (!Core.Cookies["CurrentGameType"].Equals(GameType.COOPERATIVE))
            {
                return;
            }

            var thread = new Thread(AdjustGrooveMomentum);
            double mx = _noteBarSet.NumberCompleted(player) + _noteBarSet.NumberReverse(player);
            //Ideal streaks provide bonus groove momentum (up to 2x the normal amount)
            if (judgement == BeatlineNoteJudgement.IDEAL)
            {
                var mx2 = Convert.ToDouble((9 + Math.Max(1, Core.Players[player].Streak)));
                mx2 = Math.Min(2.0, mx2 / 10);
                mx *= mx2;
            }
            thread.Start(new GMAdjustment {Judgement = judgement, Multiplier = mx});
        }

        private readonly double[] _gmAdjustments = {0.08, 0.06, 0.03, 0.0, -0.15, -0.04};


        private void AdjustGrooveMomentum(object input)
        {
            Thread.Sleep(150);
            var adjustment = (GMAdjustment) input;
             
            var isPositive = _gmAdjustments[(int) adjustment.Judgement] > 0.0;
            var mx = (isPositive) ? adjustment.Multiplier : 1;
            Player.GrooveMomentum += _gmAdjustments[(int)adjustment.Judgement] * mx;          
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
            _hitAggregator.RegisterHit(player, BeatlineNoteJudgement.MISS);
       
            
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

            //Award Score
            ApplyJudgement(judgement, player, 1);
            _noteBarSet.CreateNextNoteBar(player, IsNextNoteSuper(player));
            Core.Players[player].NextCPUJudgement = Core.CPUManager.GetNextJudgement(Core.Cookies["CPUSkillLevel"].ToString(), Core.Players[player].Streak);
            
        }

        private bool IsNextNoteSuper(int player)
        {
            var result = _beatlineSet.NearestBeatlineNote(player, _phraseNumber);
            return result != null && result.NoteType == BeatlineNoteType.SUPER;
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            TextureManager.LastDrawnPhraseNumber = _phraseNumber;
            DrawBackground();

            //Draw the component sets.
             _scoreSet.Draw();
            _lifeBarSet.Draw( _phraseNumber);
            _levelbarSet.Draw( _phraseNumber);
            _hitsBarSet.Draw();

            _beatlineSet.Draw( _phraseNumber);
            _performanceBar.Opacity = 255 - _recordReplayer.Opacity;
            _performanceBar.Draw();
            _recordReplayer.Draw(_phraseNumber);
            _gmBarSet.Draw();

            //Draw the notebars.
            _noteBarSet.Draw();
            _noteJudgementSet.Draw( _phraseNumber);

            if (_phraseNumber < 0)
            {
                DrawCountdowns();
            }
            DrawSongTimeLine();
            DrawKOIndicators();
            DrawSongInfo();
            DrawClearIndicators();
            DrawText();
           
        }

        private void DrawSongTimeLine()
        {
            if (Core.Settings.Get<bool>("SongDebug"))
            {
                return;
            }
            _songTimeLine.Song = _gameSong;
            _songTimeLine.CurrentPosition = _gameSong.ConvertPhraseToMS(_phraseNumber) / 1000;
            _songTimeLine.AudioEnd = Core.Audio.GetChannelLength((int) Core.Cookies["GameSongChannel"]) / 1000;               
            _songTimeLine.Draw();
        }

        private readonly Color[] _visualizerColors = {
                                                new Color(0, 0, 128),
                                                new Color(0, 0, 255),
                                                new Color(0, 128, 255),
                                                new Color(0, 192, 192),
                                                new Color(0, 192, 96),
                                                new Color(0, 255, 0),
                                                new Color(128, 255, 0),
                                                new Color(255, 255, 0),
                                                new Color(255, 192, 0),
                                                new Color(255, 120, 0),
                                                new Color(255, 0, 0),
                                                new Color(255, 0, 128),
                                                new Color(255, 0, 255)
                                                     };

        private readonly Color[] _blazingColors = {
                                                      new Color(255, 0, 0),
                                                      new Color(0, 255, 0),
                                                      new Color(0, 0, 255),
                                                      new Color(255, 0, 0)
                                                  };

        private double _rainbowPoint;

        private void DrawBackground()
        {
            
            _background.Draw();
            _visualBackground.Opacity = Math.Min(255,(Math.Pow(GetAverageLevel(),1.8) - 1)*5);

            var anyBlazing = (from e in Core.Players where e.Playing && e.IsBlazing select e).Any();
            
            if (anyBlazing)
            {
                _rainbowPoint = (_rainbowPoint + TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * 2) %3;
                _visualBackground.Colour = Color.Lerp(_blazingColors[(int) Math.Floor(_rainbowPoint)], _blazingColors[(int) Math.Ceiling(_rainbowPoint)],
                                                      (float) (_rainbowPoint - Math.Floor(_rainbowPoint)));
            }
            else
            {
                _visualBackground.Colour = _visualizerColors[(int)Math.Floor(GetAverageLevel() - 1)]; 
            }

            _visualBackground.Draw(_phraseNumber);
            _textBackground.Draw();

        }

        private double GetAverageLevel()
        {
            if ((GameType) Core.Cookies["CurrentGameType"] == GameType.SYNC_PLUS)
            {
                return Core.Players[0].Level;
            }
            return (from e in Core.Players where e.Playing select e.Level).Average();
        }

        private void DrawCountdowns()
        {
            _countdownSet.Draw( _phraseNumber);
        }

        private void DrawClearIndicators()
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
                    _clearSprite.Draw();
                }
            }
        }

        private void DrawKOIndicators()
        {

            for (int x = 0; x < PLAYER_COUNT; x++)
            {
                if ((Core.Players[x].KO) && (Core.Players[x].Playing))
                {
                    _koSprite.Position = (Core.Metrics["KOIndicator", x]);
                    _koSprite.Draw();
                }
            }
        }

        private void DrawSongInfo()
        {
            FontManager.DrawString("" + CalculateTimeLeft(), "DefaultFont",
                Core.Metrics["SongTimeLeft", 0], Color.Black, FontAlign.Left);
            var scale = FontManager.ScaleTextToFit(_gameSong.Title, "DefaultFont", 310, 100);
            FontManager.DrawString(_gameSong.Title, "DefaultFont",
                Core.Metrics["SongTitle", 0], scale, Color.Black, FontAlign.Left);
            scale = FontManager.ScaleTextToFit(_gameSong.Artist, "DefaultFont", 310, 100);
            FontManager.DrawString(_gameSong.Artist, "DefaultFont", 
                Core.Metrics["SongArtist", 0],scale, Color.Black, FontAlign.Left);
            FontManager.DrawString(String.Format("{0:F2}", _phraseNumber), "DefaultFont", Core.Metrics["SongDebugPhrase", 0], Color.Black, FontAlign.Left);
        }

        private void DrawText()
        {

            if (Core.Settings.Get<bool>("SongDebug"))
            {
                DrawDebugText();
            }
        }

        private void DrawDebugText()
        {

            FontManager.DrawString(String.Format("BPM: {0:F2}", _beatlineSet.Bpm),
       "DefaultFont", Core.Metrics["SongDebugBPM", 0], Color.Black, FontAlign.Left);
            FontManager.DrawString(String.Format("Offset: {0:F3}", _gameSong.Offset),
                    "DefaultFont", Core.Metrics["SongDebugOffset", 0], Color.Black, FontAlign.Left);
            FontManager.DrawString(String.Format("Hitoffset: {0:F3}", _debugLastHitOffset),
                "DefaultFont", Core.Metrics["SongDebugHitOffset", 0], Color.Black, FontAlign.Left);
            FontManager.DrawString(String.Format("Length: {0:F3}", _gameSong.Length),
                "DefaultFont", Core.Metrics["SongDebugLength", 0], Color.Black, FontAlign.Left);
          //  TextureManager.DrawString( _gameSong.ConvertPhraseToMS(_phraseNumber) + " ms","DefaultFont",new Vector2(375,350),Color.Black,FontAlign.LEFT );
        }


        #endregion
    }
}
