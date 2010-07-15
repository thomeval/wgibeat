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
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class MainGameScreen : GameScreen
    {

        //TODO: Consider refactoring beatlines into Sets.
        private List<BeatlineNote> _beatlineNotes;

        private double _phraseNumber;
        private double _hitoffset;
        private double _songLoadDelay;

        private LifeBarSet _lifeBarSet;
        private LevelBarSet _levelbarSet;
        private HitsBarSet _hitsbarSet;
        private ScoreSet _scoreSet;
        private NoteJudgementSet _noteJudgementSet;
        private NoteBar[] _notebars;
        private int _playerCount;
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
            _lifeBarSet = new LifeBarSet (Core.Metrics, Core.Players, Core.Settings.Get<GameType>("CurrentGameType"));
            _levelbarSet = new LevelBarSet(Core.Metrics, Core.Players,Core.Settings.Get<GameType>("CurrentGameType"));
            _hitsbarSet = new HitsBarSet(Core.Metrics, Core.Players, Core.Settings.Get<GameType>("CurrentGameType"));
            _scoreSet = new ScoreSet(Core.Metrics, Core.Players, Core.Settings.Get<GameType>("CurrentGameType"));
            _noteJudgementSet = new NoteJudgementSet(Core.Metrics, Core.Players, Core.Settings.Get<GameType>("CurrentGameType"));
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
                    _lifeBarSet.Reset();
                    Core.Players[x].ResetStats();

                }

                _notebars[x] = NoteBar.CreateNoteBar((int) Core.Players[x].Level, 0);
                _notebars[x].SetPosition(Core.Metrics["NoteBar", x]);

            }

            _gameSong = Core.Settings.Get<GameSong>("CurrentSong");
            Core.Songs.LoadSong(_gameSong);

            _startTime = null;

            _beatlineNotes = new List<BeatlineNote>();
            _notesToRemove = new List<BeatlineNote>();

            base.Initialize();
        }

        #region Updating, Beatline maintenance

        private Timer _maintenanceThread;
        private int _timeCheck;
        public override void Update(GameTime gameTime)
        {

            if (_startTime == null)
            {
                Core.Songs.PlaySong();
                _startTime = new TimeSpan(gameTime.TotalRealTime.Ticks);
            }

            if (_maintenanceThread == null)
            {
                _maintaining = false;
                _maintenanceThread = new Timer(DoMaintenance,null,0,20);
            }

            CheckForEndings(gameTime);
            _phraseNumber = 1.0 * (gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay - _gameSong.Offset * 1000) / 1000 * (_gameSong.Bpm / 240);
            _timeCheck = (int)(gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds + _songLoadDelay);
            base.Update(gameTime);
        }

        private void CheckForEndings(GameTime gameTime)
        {
            if (SongPassed(gameTime) && (_displayState == 0))
            {
                _displayState = 1;
                _transitionTime = gameTime.TotalRealTime.TotalSeconds + 3;
            }
            if ((_displayState == 1) && (gameTime.TotalRealTime.TotalSeconds >= _transitionTime))
            {
                if (Core.Settings.Get<bool>("SongDebug"))
                {
                    SongManager.SaveToFile(_gameSong);
                }
                _maintenanceThread.Dispose();
                _maintenanceThread = null;
                Core.ScreenTransition("Evaluation");
            }
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

            MaintainBeatlineNotes();

            //FMOD cannot reliably determine the position of the song. Using GetCurrentSongProgress()
            //as the default timing mechanism makes it jerky and slows the game down, so we attempt
            //to match current time with the song time by periodically sampling it. A hill climbing method
            // is used here.
            var delay = Core.Songs.GetCurrentSongProgress() - _timeCheck;
            if ((_confidence < 15) && (Math.Abs(delay) > 25))
            {
                _confidence = 0;
                _songLoadDelay += delay/2.0;
            }
            else if (_confidence < 15)
            {
                _confidence++;
            }

            _maintaining = false;
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
                    _lifeBarSet.AdjustLife(_noteJudgementSet.AwardJudgement(BeatlineNoteJudgement.MISS, bnr.Player, 0,0), bnr.Player);
                }
            }

            if ((_phraseNumber + 2 > _lastBeatlineNote) && (_lastBeatlineNote + 1 < GetEndingTimeInPhrase()))
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
            return ((_gameSong.Length - _gameSong.Offset) * 1000 + _songLoadDelay) / 1000 * (_gameSong.Bpm / 240);
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
                case Action.SYSTEM_LENGTH_DECREASE:
                    if (songDebug)
                    {
                        _gameSong.Length -= 0.1;
                    }
                    break;
                case Action.SYSTEM_LENGTH_INCREASE:
                    if (songDebug)
                    {
                        _gameSong.Length += 0.1;
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

            if ((_notebars[player].CurrentNote() != null) && (Note.ActionToDirection(action) == _notebars[player].CurrentNote().Direction))
            {
                _notebars[player].MarkCurrentCompleted();
                Core.Players[player].Hits++;
            }
            else if (_notebars[player].CurrentNote() != null)
            {
                _notebars[player].ResetAll();
                
                _lifeBarSet.AdjustLife(Core.Players[player].MissedArrow(),player);
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
                _notebars[player] = NoteBar.CreateNoteBar((int)Core.Players[player].Level, 0, Core.Metrics["NoteBar", player]);

            }
            else
            {
                AwardJudgement(null, player);
                //Create next note bar.
                _notebars[player] = NoteBar.CreateNoteBar((int)Core.Players[player].Level, 0, Core.Metrics["NoteBar", player]);
            }


            //Mark the beatlinenote as hit (it will be displayed differently and hold position)
            if (nearestBeatline != null)
            {
                nearestBeatline.Hit = true;
                nearestBeatline.DisplayPosition = CalculateAbsoluteBeatlinePosition(nearestBeatline.Position, nearestBeatline.Player);
                nearestBeatline.Position = _phraseNumber + 0.3;
            }

            Monitor.Exit(_beatlineNotes);
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
                case Difficulty.INSANE:
                    return 200;
                default:
                    return 0;
            }
        }

        #region Beatline and Judgement

        private int CalculateAbsoluteBeatlinePosition(double position, int player)
        {
            return (int)((position - _phraseNumber) * Core.Players[player].BeatlineSpeed * BEAT_ZOOM_DISTANCE);
        }


        private void AwardJudgement(BeatlineNote nearest, int player)
        {
            double offset = (nearest == null) ? 9999 : CalculateHitOffset(nearest);
            offset = Math.Abs(offset);
            BeatlineNoteJudgement result = BeatlineNoteJudgement.FAIL;

            for (int x = 0; x < _noteJudgementSet.JudgementCutoffs.Count(); x++ )
            {
                if (offset < _noteJudgementSet.JudgementCutoffs[x])
                {
                    result = (BeatlineNoteJudgement) x;
                    break;
                }
            }

            var lifeAdjust = _noteJudgementSet.AwardJudgement(result, player, _notebars[player].NumberCompleted(), _notebars[player].Notes.Count - _notebars[player].NumberCompleted());
            _lifeBarSet.AdjustLife(lifeAdjust, player);

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
            _noteJudgementSet.Draw(spriteBatch,_phraseNumber);

           
            if (_phraseNumber < 0)
            {
                DrawCountdowns(spriteBatch);
            }
            
            DrawBeat(spriteBatch);
            DrawKOIndicators(spriteBatch);
            DrawSongInfo(spriteBatch,gameTime);
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

        private readonly double[] _threshholds = {-1.00, -0.75, -0.5, -0.25, 0.0};
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
                        countdownSpriteMap.ColorShading.A = (byte) Math.Min(255,(_threshholds[y] -  _phraseNumber) * 255 * 4);
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
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Hitoffset: {0:F3}",  _hitoffset),
                           Core.Metrics["SongDebugHitOffset",0], Color.Black);
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
                var noteSpeed = Core.Players[bn.Player].BeatlineSpeed;
                var markerBeatOffset = (int)(noteSpeed * BEAT_ZOOM_DISTANCE * (_phraseNumber - bn.Position));

                //Dont render notes outside the visibility range.
                if (((-1 * markerBeatOffset) > BEAT_ZOOM_DISTANCE * BEAT_VISIBILITY) && (!bn.Hit))
                {
                    continue;
                }

                var markerPosition = new Vector2{Y = (int)Core.Metrics["BeatlineBarBase", bn.Player].Y + 3};
                if (bn.Hit)
                {
                    markerPosition.X = (int)Core.Metrics["BeatlineBarBase", bn.Player].X + 28 + (bn.DisplayPosition);
                    markerSprite.ColorShading.A = 128;
                }
                else
                {
                    markerPosition.X = (int)Core.Metrics["BeatlineBarBase", bn.Player].X + 28 -(markerBeatOffset);     
                
                    if (markerBeatOffset > 0)
                    {
                        markerSprite.ColorShading.A = (byte)(Math.Max(0, 255 - 10 * markerBeatOffset));
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
            pulseSprite.ColorShading.A = (byte)(pulseSprite.Width * 255 / 80);
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
