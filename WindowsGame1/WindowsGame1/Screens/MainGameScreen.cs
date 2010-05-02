using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGame1.AudioSystem;
using WindowsGame1.Drawing;
using WindowsGame1.Notes;

namespace WindowsGame1.Screens
{
    public class MainGameScreen : GameScreen
    {

        private List<BeatlineNote> _beatlineNotes;
        private List<DisplayedJudgement> _displayedJudgements;

        private double _phraseNumber;
        private double _hitoffset;

        private Lifebar[] _lifebars;
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
            _lifebars = new Lifebar[_playerCount];
            _displayState = 0;
            _lastBeatlineNote = -1;
            for (int x = 0; x < _playerCount; x++)
            {

                _lifebars[x] = new NormalLifebar {Height = 30, Width = 260, SideLocation = x};
                _lifebars[x].SetPosition(Core.Metrics["NormalLifebar", x]);

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
                    Core.Players[x].ResetStats();
                }
                _lifebars[x].SetLife(Core.Players[x].Life);
                _notebars[x] = NoteBar.CreateNoteBar((int) Core.Players[x].Level, 0);
                _notebars[x].SetPosition(Core.Metrics["Notebar", x]);
                _startTime = null;
            }

            //StartFModSong();
            _gameSong = Core.Settings.Get<GameSong>("CurrentSong");

            Core.Songs.LoadSong(_gameSong);
            //se = Core.Content.Load<SoundEffect>("Audio/" + _gameSong.AssetName);
            //sei = se.CreateInstance();

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

        public override void Update(GameTime gameTime)
        {
            MaintainBeatlineNotes();
            MaintainDisplayedJudgements();

            if (_startTime == null)
            {
                Core.Songs.PlaySong();
                _startTime = new TimeSpan(gameTime.TotalRealTime.Ticks);
            }
            if (SongPassed(gameTime) && (_displayState == 0))
            {
                _displayState = 1;
                _transitionTime = gameTime.TotalRealTime.TotalSeconds + 3;

            }
            if ((_displayState == 1) && (gameTime.TotalRealTime.TotalSeconds >= _transitionTime))
            {
                SaveSongToFile();
                Core.ScreenTransition("Evaluation");
            }
            _phraseNumber = (gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds - _gameSong.Offset * 1000) / 1000 * (_gameSong.Bpm / 240);
            base.Update(gameTime);
        }

        private void SaveSongToFile()
        {
            if (Core.Settings.Get<int>("SongDebug") == 1)
            {
                SongManager.SaveToFile(_gameSong);
            }
        }

        List<DisplayedJudgement> djToRemove = new List<DisplayedJudgement>();
        private void MaintainDisplayedJudgements()
        {

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
        }

        private List<BeatlineNote> _notesToRemove;
        private double _lastBeatlineNote = -1;


        private void MaintainBeatlineNotes()
        {

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
                if (!bnr.Hit)
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

        public double CeilingPlus(double number)
        {
            if (Math.Ceiling(number) == number)
            {
                return number + 1;
            }
            return Math.Ceiling(number);
        }

        #region Actions/Input
        /// <summary>
        /// Executes whenever the key has been pressed that corresponds to some action in the game.
        /// </summary>
        /// <param name="action"></param>
        public override void PerformAction(Action action)
        {
            bool songDebug = Core.Settings.Get<int>("SongDebug") == 1;
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
                Core.Players[player].MissedArrow();

                _lifebars[player].SetLife(Core.Players[player].Life);
            }
        }

        private void HitBeatline(int player)
        {
            if (player >= _playerCount)
            {
                return;
            }
            if ((Core.Players[player].KO) || (!Core.Players[player].Playing))
            {
                return;
            }
            if (NearestBeatlineNote(player) == null)
            {
                return;
            }
            _hitoffset = CalculateHitOffset(player);
            var nearestBeatline = NearestBeatlineNote(player);

            //Prevent the player from missing beatlines very far away by mistake
            //such as double tapping the beatline key.
            if (_hitoffset > 900)
            {
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
                    return 20;
                case Difficulty.EASY:
                    return 50;
                case Difficulty.MEDIUM:
                    return 75;
                case Difficulty.HARD:
                    return 150;
                default:
                    return 0;
            }
        }

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
                AwardJudgement(0, player);
            }

        }

        private void AwardJudgement(int judgement, int player)
        {
            Texture2D tex;
            switch (judgement)
            {
                case 1:
                    //IDEAL
                    Core.Players[player].Streak++;
                    double multiplier = ((9.0 + Math.Max(1, Core.Players[player].Streak))/10.0);
                    Core.Players[player].Score += (long) (1000*_notebars[player].NumberCompleted()*multiplier);
                    Core.Players[player].AddLife(1 * _notebars[player].NumberCompleted());
                    Core.Players[player].Judgements[0]++;
                    tex = TextureManager.Textures["noteJudgement1"];
                    break;
                case 2:
                    //COOL
                    Core.Players[player].Score += 750 * _notebars[player].NumberCompleted();
                    Core.Players[player].AddLife(0.5 * _notebars[player].NumberCompleted());
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
                    Core.Players[player].Life -= 1 * _notebars[player].NumberCompleted();
                    Core.Players[player].Judgements[3]++;
                    tex = TextureManager.Textures["noteJudgement4"];
                    break;
                case 5:
                    //MISS
                    Core.Players[player].MissedBeat();
                    tex = TextureManager.Textures["noteJudgement5"];
                    break;
                default:
                    //FAIL
                    Core.Players[player].Streak = -1;
                    Core.Players[player].Life -= 3 * (_notebars[player].Notes.Count() - _notebars[player].NumberCompleted() + 1);
                    Core.Players[player].Momentum = (long)(Core.Players[player].Momentum * 0.7);
                    Core.Players[player].Judgements[4]++;
                    tex = TextureManager.Textures["noteJudgement0"];
                    break;
            }
            var newDj = new DisplayedJudgement { DisplayUntil = _phraseNumber + 0.5, Height = 40, Width = 150, Texture = tex, Player = player };
            newDj.SetPosition(Core.Metrics["Judgement", player]);
            _displayedJudgements.Add(newDj);
            _lifebars[player].SetLife(Core.Players[player].Life);
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

        private string CalculateTimeLeft(GameTime gameTime)
        {
            double timeElapsed = gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds;

            double timeLeft = _gameSong.Length * 1000 - timeElapsed;

            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)timeLeft);

            //var timeToGo = TimeSpan.TicksPerSecond * _gameSong.

            return ts.Minutes + ":" + String.Format("{0:D2}", Math.Max(0,ts.Seconds));
        }
        
        private bool SongPassed(GameTime gameTime)
        {
            double timeElapsed = gameTime.TotalRealTime.TotalMilliseconds - _startTime.Value.TotalMilliseconds;
            double timeLeft = _gameSong.Length * 1000 - timeElapsed;

            return timeLeft <= 0.0;
        }
       

        #region Drawing
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {


            // TODO: Add your drawing code here

            //Begin drawing textures.

            //Draw a texture at a specific position.
            //Position can be defined as a Vector point (normal image size)
            //or as a rectangle (scaled to rectangle size).

            //Draw the lifebars and notebars.
            for (int x = 0; x < 4; x++)
            {
                if (Core.Players[x].Playing)
                {
                    _notebars[x].Draw(spriteBatch);
                    _lifebars[x].Draw(spriteBatch);
                }
            }

            //Draw beatline judgements.
            foreach (DisplayedJudgement dj in _displayedJudgements)
            {
                int opacity = Convert.ToInt32(Math.Max(0, (dj.DisplayUntil - _phraseNumber)*510));
                opacity = Math.Min(opacity, 255);
                dj.Opacity = Convert.ToByte(opacity);
                dj.Draw(spriteBatch);
            }
            
            if (_phraseNumber < 0)
            {
                DrawCountdowns(spriteBatch);
            }
            DrawHitsCounters(spriteBatch);
            DrawStreakCounters(spriteBatch);
            DrawLevelBars(spriteBatch);
            DrawBeat(spriteBatch);
            DrawKOIndicators(spriteBatch);
            DrawSongInfo(spriteBatch,gameTime);
            DrawClearIndicators(spriteBatch);
            DrawText(spriteBatch);
            DrawBorders(spriteBatch);
        }

        private void DrawCountdowns(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < Core.Players.Count(); x++)
            {
                if (!Core.Players[x].Playing)
                {
                    return;
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
, Core.Metrics["SongTimeLeft", 0], Color.White);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + _gameSong.Title
, Core.Metrics["SongTitle", 0], Color.White);
            spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + _gameSong.Artist
, Core.Metrics["SongArtist", 0], Color.White);
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
                    _streakNumbers.SpriteMap.Color.A = currentJudgement.Opacity;

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
                       Core.Metrics["LevelText", x], Color.White);

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

        private void DrawHitsCounters(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _playerCount; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                if (Core.Players[x].Hits < 25)
                {
                    continue;
                }
                var baseSprite = new Sprite
                {
                    Height = 55,
                    Width = 60,
                    SpriteTexture =
                        (x % 2 == 0)
                            ? TextureManager.Textures["hitsBarLeft"]
                            : TextureManager.Textures["hitsBarRight"],
                };
                baseSprite.SetPosition(Core.Metrics["HitsBar", x]);
                baseSprite.Draw(spriteBatch);
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("{0:D3}", Core.Players[x].Hits),
       Core.Metrics["HitsText", x], Color.White);

            }
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

                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "P" + (x + 1), Core.Metrics["PlayerText", x], Color.White);
                spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "" + Core.Players[x].DisplayedScore,
                                       Core.Metrics["ScoreText", x], Color.White);

                
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("{0:D3}", (int)Core.Players[x].Life),
                                       Core.Metrics["NormalLifebarText", x], Color.White);

            }
            if (Core.Settings.Get<int>("SongDebug") == 1)
            {
            DrawDebugText(spriteBatch);
            }
        }

        private void DrawDebugText(SpriteBatch spriteBatch)
        {

                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("BPM: {0:F2}", _gameSong.Bpm),
                       Core.Metrics["SongDebugBPM", 0], Color.White);
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Offset: {0:F3}", _gameSong.Offset),
                        Core.Metrics["SongDebugOffset", 0], Color.White);
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], "" + String.Format("{0:F3}", _phraseNumber), Core.Metrics["SongDebugPhrase", 0], Color.White);
                spriteBatch.DrawString(TextureManager.Fonts["DefaultFont"], String.Format("Hitoffset: {0:F3}",  _hitoffset),
                           Core.Metrics["SongDebugHitOffset",0], Color.White);
  
        }

        private void AdjustDisplayedScores()
        {
            for (int x = 0; x < 3; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }

                long amount = (long) Math.Max(50, (Core.Players[x].Score - Core.Players[x].DisplayedScore) / 10);

                Core.Players[x].DisplayedScore = Math.Min(Core.Players[x].Score, Core.Players[x].DisplayedScore + amount);
            }
        }

        private void DrawBorders(SpriteBatch spriteBatch)
        {
            var brush = new PrimitiveLine(Core.GraphicsDevice) { Colour = Color.White };
            brush.AddVector(new Vector2(400, 0));
            brush.AddVector(new Vector2(400, 600));
            brush.Render(spriteBatch);
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

            DrawBeatlineBases(spriteBatch);

            foreach (BeatlineNote bn in _beatlineNotes)
            {
                var markerBeatOffset = (int)(BEAT_ZOOM_DISTANCE * (_phraseNumber - bn.Position));
                if ((-1 * markerBeatOffset) > BEAT_ZOOM_DISTANCE * BEAT_VISIBILITY)
                {
                    continue;
                }
                var markerSprite = new Sprite
                {
                    Height = 34,
                    Width = 5,
                    SpriteTexture = TextureManager.Textures["beatMarkerP1"],
                    X = (int)Core.Metrics["BeatlineBarBase", bn.Player].X + 28 - (markerBeatOffset),
                    Y = (int)Core.Metrics["BeatlineBarBase", bn.Player].Y + 3

                };
                if (bn.Hit)
                {
                    markerSprite.X = (int)Core.Metrics["BeatlineBarBase", bn.Player].X + 28 + bn.DisplayPosition;
                    //TODO: Fix
                    markerSprite.ColorShading.A = 128;
                }
                else
                {


                    if (markerBeatOffset > 0)
                    {
                        markerSprite.ColorShading.A = (byte)(255 + 1.1 * CalculateHitOffset(bn));
                    }
                    else
                    {
                        markerSprite.ColorShading.A = 255;
                    }
                }
                markerSprite.Draw(spriteBatch);

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
