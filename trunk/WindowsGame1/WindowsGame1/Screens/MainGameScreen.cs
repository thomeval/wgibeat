using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Drawing.Sets;
using WGiBeat.Notes;

namespace WGiBeat.Screens
{
    public class MainGameScreen : GameScreen
    {

        private List<BeatlineNote> _beatlineNotes;
        private List<DisplayedJudgement> _displayedJudgements;

        private double _phraseNumber;
        private double _hitoffset;
        private double _songLoadDelay;

        private LifebarSet _lifebarSet;
        private HitsBarSet _hitsbarSet;
        private NoteBar[] _notebars;
        private int _playerCount = 0;
        private GraphicNumber _streakNumbers;
        private GameSong _gameSong;
        private TimeSpan? _startTime;
        private int _displayState;
        private double _transitionTime;


        public MainGameScreen(GameCore core) : base(core)
        {
        }

        public override void Initialize()
        {
            _playerCount = 4;
            _notebars = new NoteBar[_playerCount];
            _lifebarSet = new LifebarSet (Core.Metrics, Core.Players, Core.Settings.Get<GameType>("CurrentGameType"));
            _hitsbarSet = new HitsBarSet(Core.Metrics, Core.Players, Core.Settings.Get<GameType>("CurrentGameType"));
            _displayState = 0;
            _songLoadDelay = 0.0;
            _confidence = 0;
            _lastBeatlineNote = -1;
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
                                                  (Difficulty) Core.Settings.Get<int>("P" + (x + 1) + "Difficulty"),
                                              Streak = -1
                                          };
                }
                else
                {
                    _lifebarSet.Reset();
                    Core.Players[x].ResetStats();

                }


                _notebars[x] = NoteBar.CreateNoteBar((int) Core.Players[x].Level, 0);
                _notebars[x].SetPosition(Core.Metrics["Notebar", x]);

            }

            _gameSong = Core.Settings.Get<GameSong>("CurrentSong");
            Core.Songs.LoadSong(_gameSong);

            _startTime = null;


            _beatlineNotes = new List<BeatlineNote>();
            _notesToRemove = new List<BeatlineNote>();
            _displayedJudgements = new List<DisplayedJudgement>();

            _streakNumbers = new GraphicNumber
            {
                SpacingAdjustment = 1,
                SpriteMap = new SpriteMap
                {
                    Columns = 3,
                    Rows = 4,
                    SpriteTexture = TextureManager.Textures["streakNumbers"]
                }
            };
            base.Initialize();
        }

        #region Updating

        private Timer _maintenanceThread;
        private int timeCheck;
        public override void Update(GameTime gameTime)
        {

            if (_startTime == null)
            {
                Core.Songs.PlaySong(Core.Settings.Get<double>("SongVolume"));
                _startTime = new TimeSpan(gameTime.TotalRealTime.Ticks);
            }
            if (SongPassed(gameTime) && (_displayState == 0))
            {
                _displayState = 1;
                _transitionTime = gameTime.TotalRealTime.TotalSeconds + 3;

            }
            if (_maintenanceThread == null)
            {
                _maintenanceThread = new Timer(DoMaintenance,null,0,20);
            }
            if ((_displayState == 1) && (gameTime.TotalRealTime.TotalSeconds >= _transitionTime))
            {
                SaveSongToFile();
                SaveHighScore();
                _maintenanceThread.Dispose();
                _maintenanceThread = null;
                Core.ScreenTransition("Evaluation");
            }         
            _phraseNumber = 1.0 * (gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay - _gameSong.Offset * 1000) / 1000 * (_gameSong.Bpm / 240);
            timeCheck = (int)(gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay);
            base.Update(gameTime);
        }

        private int _confidence;
        private bool _maintaining;
        private void DoMaintenance(object state)
        {
          if (_maintaining)
          {
              return;
          }

            _maintaining = true;
            if (_displayState != 0)
            {
                return;
            }
                //_phraseNumber = 1.0 * (Core.Songs.GetCurrentSongProgress() - _gameSong.Offset * 1000) / 1000 * (_gameSong.Bpm / 240);
                MaintainBeatlineNotes();
                MaintainDisplayedJudgements();

            //FMOD cannot reliably determine the position of the song. Using GetCurrentSongProgress()
            //as the default timing mechanism makes it jerky and slows the game down, so we attempt
            //to match current time with the song time by periodically sampling it. A hill climbing method
            // is used here.
            var delay = Core.Songs.GetCurrentSongProgress() - timeCheck;
                if ((_confidence < 15) && (Math.Abs(delay) > 25))
                {
                    _confidence = 0;
                    _songLoadDelay +=  delay / 2.0;
                }
                else if (_confidence < 15)
                {
                    _confidence++;
                }

            _maintaining = false;
        }
        private void SaveSongToFile()
        {
            if (Core.Settings.Get<bool>("SongDebug"))
            {
                SongManager.SaveToFile(_gameSong);
            }
        }

        private void SaveHighScore()
        {
            Core.Settings.Set("HighScorePlayer", -1);
            //EvaluationScreen doesn't know what the current song is, so highscores must be saved here.
            long highest = Core.Songs.GetHighScore(_gameSong.GetHashCode(),
                                                   Core.Settings.Get<GameType>("CurrentGameType"));
            int awardedPlayer = -1;
            for (int x = 0; x < 4; x++)
            {
                if ((Core.Players[x].Playing) &&(Core.Players[x].Score > highest))
                {
                    //Store player with high score so that EvaluationScreen can display it.
                    Core.Settings.Set("HighScorePlayer", x);
                    highest = Math.Max(highest, Core.Players[x].Score);
                    awardedPlayer = x;
                }
            }

            if (awardedPlayer != -1)
            {
                Core.Songs.SetHighScore(_gameSong.GetHashCode(), Core.Settings.Get<GameType>("CurrentGameType"), highest);
                Core.Songs.SaveHighScores("Scores.conf");
            }

        }

        List<DisplayedJudgement> djToRemove = new List<DisplayedJudgement>();
        private void MaintainDisplayedJudgements()
        {
            Monitor.Enter(_displayedJudgements);
            foreach (DisplayedJudgement dj in _displayedJudgements)
            {
                if (dj.DisplayUntil <= _phraseNumber)
                {
                    djToRemove.Add(dj);
                }
            }

            foreach (DisplayedJudgement djr in djToRemove)
            {
                _displayedJudgements.Remove(djr);
            }
            djToRemove.Clear();
            Monitor.Exit(_displayedJudgements);
        }

        private List<BeatlineNote> _notesToRemove;
        private double _lastBeatlineNote = -1;


        private void MaintainBeatlineNotes()
        {

            Monitor.Enter(_beatlineNotes);
            foreach (BeatlineNote bn in _beatlineNotes)
            {
                if ((CalculateHitOffset(bn) < -200) || (Core.Players[bn.Player].KO))
                {
                    _notesToRemove.Add(bn);
                }
            }

            foreach (BeatlineNote bnr in _notesToRemove)
            {
                _beatlineNotes.Remove(bnr);
                if ((!bnr.Hit) && (!Core.Players[bnr.Player].KO))
                {
                    AwardJudgement(5, bnr.Player);
                }
            }

            if ((_phraseNumber + 1 > _lastBeatlineNote) && (_lastBeatlineNote +1 < GetEndingTimeInPhrase()))
            {
                _lastBeatlineNote++;
                for (int x = 0; x < _playerCount; x++)
                {
                    if ((!Core.Players[x].KO) && (Core.Players[x].Playing))
                    {
                        _beatlineNotes.Add(new BeatlineNote {Player = x, Position = _lastBeatlineNote});
                    }
                }
            }

            Monitor.Exit(_beatlineNotes);
            _notesToRemove.Clear();
        }

        private double GetEndingTimeInPhrase()
        {
            if (!_startTime.HasValue)
            {
                return 9999;
            }
            return ((_gameSong.Length - _gameSong.Offset) * 1000) / 1000 * (_gameSong.Bpm / 240);
        }

        #endregion

        #region Actions/Input
        /// <summary>
        /// Executes whenever the key has been pressed that corresponds to some action in the game.
        /// </summary>
        /// <param name="action"></param>
        public override void PerformAction(Action action)
        {
            bool songDebug = Core.Settings.Get<bool>("SongDebug");
            switch (action)
            {
                case Action.P1_LEFT:
                case Action.P1_RIGHT:
                case Action.P1_UP:
                case Action.P1_DOWN:
                    HitArrow(action, 0);
                    break;

                case Action.P2_LEFT:
                case Action.P2_RIGHT:
                case Action.P2_UP:
                case Action.P2_DOWN:
                    HitArrow(action, 1);
                    break;
                case Action.P3_LEFT:
                case Action.P3_RIGHT:
                case Action.P3_UP:
                case Action.P3_DOWN:
                    HitArrow(action, 2);
                    break;
                case Action.P4_LEFT:
                case Action.P4_RIGHT:
                case Action.P4_UP:
                case Action.P4_DOWN:
                    HitArrow(action, 3);
                    break;
                case Action.P1_BEATLINE:
                    HitBeatline(0);
                    break;
                case Action.P2_BEATLINE:
                    HitBeatline(1);
                    break;
                case Action.P3_BEATLINE:
                    HitBeatline(2);
                    break;
                case Action.P4_BEATLINE:
                    HitBeatline(3);
                    break;
                case Action.SYSTEM_BPM_DECREASE:
                    if (songDebug)
                    {
                        _gameSong.Bpm -= 0.1;
                    }
                    break;
                case Action.SYSTEM_BPM_INCREASE:
                    if (songDebug)
                    {
                        _gameSong.Bpm += 0.1;
                    }
                    break;
                case Action.SYSTEM_OFFSET_DECREASE_BIG:
                    if (songDebug)
                    {
                        _gameSong.Offset -= 0.1;
                    }
                    break;
                case Action.SYSTEM_OFFSET_INCREASE_BIG:
                    if (songDebug)
                    {
                        _gameSong.Offset += 0.1;
                    }
                    break;
                case Action.SYSTEM_OFFSET_DECREASE_SMALL:
                    if (songDebug)
                    {
                        _gameSong.Offset -= 0.01;
                    }
                    break;
                case Action.SYSTEM_OFFSET_INCREASE_SMALL:
                    if (songDebug)
                    {
                        _gameSong.Offset += 0.01;
                    }
                    break;
                case Action.SYSTEM_BACK:
                    _maintenanceThread.Dispose();
                    _maintenanceThread = null;
                    Core.Songs.StopSong();
                    Core.ScreenTransition("SongSelect");
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

            if ((_notebars[player].CurrentNote() != null) && (ActionToDirection(action) == _notebars[player].CurrentNote().Direction))
            {
                _notebars[player].MarkCurrentCompleted();
                Core.Players[player].Hits++;
            }
            else if (_notebars[player].CurrentNote() != null)
            {
                _notebars[player].ResetAll();
                
                _lifebarSet.AdjustLife(Core.Players[player].MissedArrow(),player);
            }
        }

        private void HitBeatline(int player)
        {

            if ((Core.Players[player].KO) || (!Core.Players[player].Playing))
            {
                return;
            }
            if (NearestBeatlineNote(player) == null)
            {
                return;
            }

            Monitor.Enter(_beatlineNotes);
            _hitoffset = CalculateHitOffset(player);
            var nearestBeatline = NearestBeatlineNote(player);

            //Prevent the player from missing beatlines very far away by mistake
            //such as double tapping the beatline key.
            if (_hitoffset > 900)
            {
                Monitor.Exit(_beatlineNotes);
                return;
            }

            //Check if all notes in the notebar have been hit and act accordingly.
            if (_notebars[player].AllCompleted())
            {
                //Increment Player Level
                Core.Players[player].Momentum += MomentumIncreaseByDifficulty(Core.Players[player].PlayDifficulty);

                //Award Score
                AwardJudgement(nearestBeatline, player);
                //Create next note bar.
                _notebars[player] = NoteBar.CreateNoteBar((int)Core.Players[player].Level, 0, Core.Metrics["Notebar", player]);

            }
            else
            {
                AwardJudgement(null, player);
                //Create next note bar.

                _notebars[player] = NoteBar.CreateNoteBar((int)Core.Players[player].Level, 0, Core.Metrics["Notebar", player]);
            }


            //Mark the beatlinenote as hit (it will be displayed differently and hold position)
            if (nearestBeatline != null)
            {
                nearestBeatline.Hit = true;
                nearestBeatline.DisplayPosition = CalculateAbsoluteBeatlinePosition(nearestBeatline.Position);
                nearestBeatline.Position = _phraseNumber + 0.3;
            }

            Monitor.Exit(_beatlineNotes);
        }

        private NoteDirection ActionToDirection(Action action)
        {
            switch (action)
            {
                case Action.P1_LEFT:
                case Action.P2_LEFT:
                case Action.P3_LEFT:
                case Action.P4_LEFT:
                    return NoteDirection.LEFT;
                case Action.P1_RIGHT:
                case Action.P2_RIGHT:
                case Action.P3_RIGHT:
                case Action.P4_RIGHT:
                    return NoteDirection.RIGHT;
                case Action.P1_UP:
                case Action.P2_UP:
                case Action.P3_UP:
                case Action.P4_UP:
                    return NoteDirection.UP;
                case Action.P1_DOWN:
                case Action.P2_DOWN:
                case Action.P3_DOWN:
                case Action.P4_DOWN:
                    return NoteDirection.DOWN;
                default:
                    return NoteDirection.COUNT;
            }
        }

        #endregion


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
                default:
                    return 0;
            }
        }

        #region Beatline and Judgement

        private int CalculateAbsoluteBeatlinePosition(double position)
        {
            return (int)(BEAT_ZOOM_DISTANCE * (position - _phraseNumber));
        }


        private void AwardJudgement(BeatlineNote nearest, int player)
        {
            double offset = (nearest == null) ? 9999 : CalculateHitOffset(nearest);
            offset = Math.Abs(offset);

            if (offset < 20)
            {
                //IDEAL
                AwardJudgement(1,player);
            }
            else if (offset < 50)
            {
                //COOL
                AwardJudgement(2, player);
            }
            else if (offset < 125)
            {
                //OK
                AwardJudgement(3, player);
            }
            else if (offset < 250)
            {
                //BAD
                AwardJudgement(4, player);
            }
            else
            {
                //FAIL
                AwardJudgement(0, player);
            }

        }

        private void AwardJudgement(int judgement, int player)
        {
            Texture2D tex;
            double lifeAdjust = 0;
            switch (judgement)
            {
                case 1:
                    //IDEAL
                    Core.Players[player].Streak++;
                    double multiplier = ((9.0 + Math.Max(1, Core.Players[player].Streak))/10.0);
                    Core.Players[player].Score += (long) (1000*_notebars[player].NumberCompleted()*multiplier);
                    lifeAdjust = (1 * _notebars[player].NumberCompleted());
                    Core.Players[player].Judgements[0]++;
                    tex = TextureManager.Textures["noteJudgement1"];
                    break;
                case 2:
                    //COOL
                    Core.Players[player].Score += 750 * _notebars[player].NumberCompleted();
                    lifeAdjust = (0.5 * _notebars[player].NumberCompleted());
                    Core.Players[player].Streak = -1;
                    Core.Players[player].Judgements[1]++;
                    tex = TextureManager.Textures["noteJudgement2"];
                    break;
                case 3:
                    //OK
                    Core.Players[player].Score += 500 * _notebars[player].NumberCompleted();
                    Core.Players[player].Streak = -1;
                    Core.Players[player].Judgements[2]++;
                    tex = TextureManager.Textures["noteJudgement3"];
                    break;
                case 4:
                    //BAD
                    Core.Players[player].Score += 250 * _notebars[player].NumberCompleted();
                    Core.Players[player].Streak = -1;
                    lifeAdjust = 1 * _notebars[player].NumberCompleted();
                    Core.Players[player].Judgements[3]++;
                    tex = TextureManager.Textures["noteJudgement4"];
                    break;
                case 5:
                    //MISS
                    lifeAdjust = Core.Players[player].MissedBeat();
                    tex = TextureManager.Textures["noteJudgement5"];
                    break;
                default:
                    //FAIL
                    Core.Players[player].Streak = -1;
                    lifeAdjust -= (int)(1 + Core.Players[player].PlayDifficulty) * (_notebars[player].Notes.Count() - _notebars[player].NumberCompleted() + 1);
                    Core.Players[player].Momentum = (long)(Core.Players[player].Momentum * 0.7);
                    Core.Players[player].Judgements[4]++;
                    tex = TextureManager.Textures["noteJudgement0"];
                    break;
            }
            var newDj = new DisplayedJudgement { DisplayUntil = _phraseNumber + 0.5, Height = 40, Width = 150, Texture = tex, Player = player };
            newDj.SetPosition(Core.Metrics["Judgement", player]);
            
            _lifebarSet.AdjustLife(lifeAdjust, player);

            Monitor.Enter(_displayedJudgements);
            _displayedJudgements.Add(newDj);
            Monitor.Exit(_displayedJudgements);
            
        }

        private BeatlineNote NearestBeatlineNote(int player)
        {
            return (from e in _beatlineNotes where (e.Player == player) && (!e.Hit) orderby CalculateHitOffset(e) select e).FirstOrDefault();
        }

        private double CalculateHitOffset(int player)
        {
            return CalculateHitOffset(NearestBeatlineNote(player));
        }

        private double CalculateHitOffset(BeatlineNote bln)
        {
            return (bln.Position - _phraseNumber) * 1000 * 240 / _gameSong.Bpm;
        }

        #endregion

        private string CalculateTimeLeft(GameTime gameTime)
        {
            var timeElapsed = gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds;
            var timeLeft = _gameSong.Length * 1000 - timeElapsed;

            var ts = new TimeSpan(0, 0, 0, 0, (int)timeLeft);

            return ts.Minutes + ":" + String.Format("{0:D2}", Math.Max(0,ts.Seconds));
        }
        
        private bool SongPassed(GameTime gameTime)
        {
            var timeElapsed = gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds;
            var timeLeft = _gameSong.Length * 1000 - timeElapsed;

            return timeLeft <= 0.0;
        }
       

        #region Drawing

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

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            //Begin drawing textures.

            //Draw the lifebars and notebars.
            for (int x = 0; x < 4; x++)
            {
                if (Core.Players[x].Playing)
                {
                    _notebars[x].Draw(spriteBatch);
                }
            }
            _lifebarSet.Draw(spriteBatch);
            _hitsbarSet.Draw(spriteBatch);

            //Draw beatline judgements.
            Monitor.Enter(_displayedJudgements);
            foreach (DisplayedJudgement dj in _displayedJudgements)
            {
                int opacity = Convert.ToInt32(Math.Max(0, (dj.DisplayUntil - _phraseNumber)*510));
                opacity = Math.Min(opacity, 255);
                dj.Opacity = Convert.ToByte(opacity);
                dj.Draw(spriteBatch);
            }
            Monitor.Exit(_displayedJudgements);

            DrawBorders(spriteBatch);            
            if (_phraseNumber < 0)
            {
                DrawCountdowns(spriteBatch);
            }
            
            DrawStreakCounters(spriteBatch);
            DrawLevelBars(spriteBatch);
            DrawBeat(spriteBatch);
            DrawKOIndicators(spriteBatch);
            DrawSongInfo(spriteBatch,gameTime);
            DrawClearIndicators(spriteBatch);
            DrawText(spriteBatch);
            
        }

        private void DrawCountdowns(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < Core.Players.Count(); x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                var countdownSpriteMap = new SpriteMap()
                                             {
                                                 Columns = 1,
                                                 Rows = 4,
                                                 SpriteTexture = TextureManager.Textures["countdown"]
                                             };

                if (_phraseNumber < -0.75)
                {
                    countdownSpriteMap.Draw(spriteBatch,0,200,60,Core.Metrics["Countdown",x]);
                }
                else if (_phraseNumber < -0.5)
                {
                    countdownSpriteMap.Draw(spriteBatch, 1, 200, 60, Core.Metrics["Countdown", x]);
                }
                else if (_phraseNumber < -0.25)
                {
                    countdownSpriteMap.Draw(spriteBatch, 2, 200, 60, Core.Metrics["Countdown", x]);
                }
                else if (_phraseNumber < -0.0)
                {
                    countdownSpriteMap.Draw(spriteBatch, 3, 200, 60, Core.Metrics["Countdown", x]);
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


        private void DrawSongInfo(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + CalculateTimeLeft(gameTime)
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

        private void DrawStreakCounters(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _playerCount; x++)
            {
                if (Core.Players[x].Streak > 0)
                {
                    var currentJudgement = GetDisplayedJudgement(x);
                    if (currentJudgement == null)
                    {
                        return;
                    }
                    if (currentJudgement.Texture != TextureManager.Textures["noteJudgement1"])
                    {
                        return;
                    }
                    _streakNumbers.SpriteMap.ColorShading.A = currentJudgement.Opacity;

                    _streakNumbers.DrawNumber(spriteBatch, Core.Players[x].Streak, Core.Metrics["StreakText", x], 30, 40);
                }
            }
        }

        private DisplayedJudgement GetDisplayedJudgement(int player)
        {
            return
                (from e in _displayedJudgements where e.Player == player orderby e.DisplayUntil select e).FirstOrDefault
                    ();
        }
        private void DrawLevelBars(SpriteBatch spriteBatch)
        {
            const int LEVELBAR_MAX_WIDTH = 185;
            for (int x = 0; x < _playerCount; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                var baseSprite = new Sprite
                {
                    Height = 25,
                    Width = 216,
                    SpriteTexture = TextureManager.Textures["levelBarBase"]

                };
                baseSprite.SetPosition(Core.Metrics["LevelBarBase", x]);
                baseSprite.Draw(spriteBatch);
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + (int)Core.Players[x].Level,
                       Core.Metrics["LevelText", x], Color.Black);

                double levelFraction = Core.Players[x].Level - (int)Core.Players[x].Level;
                var barWidth = (int)(levelFraction * LEVELBAR_MAX_WIDTH);
                var barTexture = GetLevelBarTexture(Core.Players[x].Level);

                if (Core.Players[x].Level == Core.Players[x].MaxDifficulty())
                {
                    barWidth = LEVELBAR_MAX_WIDTH;
                }
                var barSprite = new Sprite
                {
                    Height = 19,
                    Width = barWidth,
                    SpriteTexture = TextureManager.Textures[barTexture]
                };
                barSprite.SetPosition(Core.Metrics["LevelBar", x]);

                barSprite.Draw(spriteBatch);
            }
        }

        //TODO: Refactor
        private string GetLevelBarTexture(double level)
        {
            if (level > 8)
            {
                return "levelBarFront4";
            }
            if (level > 5)
            {
                return "levelBarFront3";

            }
            if (level > 3)
            {
                return "levelBarFront2";
            }
            return "levelBarFront1";
        }


        private void DrawText(SpriteBatch spriteBatch)
        {

            AdjustDisplayedScores();
            for (int x = 0; x < _playerCount; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "P" + (x + 1), Core.Metrics["PlayerText", x], Color.Black);
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + Core.Players[x].DisplayedScore,
                                       Core.Metrics["ScoreText", x], Color.Black);

              

            }
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
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Hitoffset: {0:F3}",  _hitoffset),
                           Core.Metrics["SongDebugHitOffset",0], Color.Black);
  
        }

        private void AdjustDisplayedScores()
        {
            for (int x = 0; x < 3; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }

                var amount = (long) Math.Max(50, (Core.Players[x].Score - Core.Players[x].DisplayedScore) / 10);

                Core.Players[x].DisplayedScore = Math.Min(Core.Players[x].Score, Core.Players[x].DisplayedScore + amount);
            }
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
        //How distant the beatline notes are from each other.
        //Increase this to make them move faster and more apart.
        const int BEAT_ZOOM_DISTANCE = 200;
        private void DrawBeat(SpriteBatch spriteBatch)
        {

            //Prevent notes from being drawn outside the beatline base.
            //Higher means longer visibility range.
            const double BEAT_VISIBILITY = 1.1;
            DrawBeatlinePulses(spriteBatch);
            DrawBeatlineBases(spriteBatch);

            var markerSprite = new SpriteMap
            {
                Columns = 1,
                Rows = 4,
                SpriteTexture = TextureManager.Textures["beatMarkers"],

            };
            Monitor.Enter(_beatlineNotes);
            foreach (BeatlineNote bn in _beatlineNotes)
            {
                var markerBeatOffset = (int)(BEAT_ZOOM_DISTANCE * (_phraseNumber - bn.Position));

                //Dont render notes outside the visibility range.
                if ((-1 * markerBeatOffset) > BEAT_ZOOM_DISTANCE * BEAT_VISIBILITY)
                {
                    continue;
                }

                var markerPosition = new Vector2{Y = (int)Core.Metrics["BeatlineBarBase", bn.Player].Y + 3};
                if (bn.Hit)
                {
                    markerPosition.X = (int)Core.Metrics["BeatlineBarBase", bn.Player].X + 28 + bn.DisplayPosition;
                    markerSprite.ColorShading.A = 128;
                }
                else
                {
                    markerPosition.X = (int)Core.Metrics["BeatlineBarBase", bn.Player].X + 28 - (markerBeatOffset);

                                   
                
                    if (markerBeatOffset > 0)
                    {
                        markerSprite.ColorShading.A = (byte)(Math.Max(0, 255 + 1.5 * CalculateHitOffset(bn)));
                    }
                    else
                    {
                        markerSprite.ColorShading.A = 255;
                    }
                }
                markerSprite.Draw(spriteBatch,bn.Player,5,34,markerPosition);

            }
            Monitor.Exit(_beatlineNotes);
        }

        private void DrawBeatlinePulses(SpriteBatch spriteBatch)
        {
            var pulseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures["BeatFlame"]
            };

            pulseSprite.Width = (int) (80*(Math.Ceiling(_phraseNumber) - (_phraseNumber)));
            pulseSprite.Height = 42;
            for (int x = 0; x < _playerCount; x++)
            {
                if ((!Core.Players[x].Playing) || (Core.Players[x].KO))
                {
                    continue;
                }
                pulseSprite.SetPosition((int) Core.Metrics["BeatlineBarBase", x].X + 30,
                                        (int) Core.Metrics["BeatlineBarBase", x].Y - 5);
                pulseSprite.DrawTiled(spriteBatch, 83 - pulseSprite.Width, 0, pulseSprite.Width, 34);
                
            }
        }

        private void DrawBeatlineBases(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _playerCount; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                var baseSprite = new Sprite
                {
                    Height = 40,
                    Width = 250,
                    SpriteTexture = TextureManager.Textures["beatMeter"],
                    X = (int)Core.Metrics["BeatlineBarBase", x].X,
                    Y = (int)Core.Metrics["BeatlineBarBase", x].Y
                };
                baseSprite.Draw(spriteBatch);
            }

        }
        #endregion
    }
}
