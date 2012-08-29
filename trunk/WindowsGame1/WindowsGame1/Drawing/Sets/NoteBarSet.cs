﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Helpers;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class NoteBarSet : DrawableObjectSet
    {
        private readonly NoteBar[] _noteBars;
        private readonly NoteBarProgress[] _noteBarProgresses;
        private int[] _syncNotebarPositions;
        public bool Visible { get; set; }

        public event EventHandler PlayerFaulted;
        public event EventHandler PlayerArrowHit;
       

        public NoteBarSet(MetricsManager metrics, Player[] players, GameType gameType) : base(metrics, players, gameType)
        {
            _noteBars = new NoteBar[4];
            _syncNotebarPositions = new int[4];
            _noteBarProgresses = new NoteBarProgress[4];
            InitNoteBars();
        }



        private const int REDNESS_ANIMATION_SPEED = 750;
        private const int FADEOUT_SPEED = 500;
        public override void Draw(SpriteBatch spriteBatch)
        {
            var amount = Math.Max(0,  TextureManager.LastDrawnPhraseDiff * REDNESS_ANIMATION_SPEED);
            var fadeAmount = Math.Max(0, TextureManager.LastDrawnPhraseDiff*FADEOUT_SPEED);
            var mx = Math.Max(0.5, 1 - (TextureManager.LastDrawnPhraseDiff * 20));

            for (int x = 0; x < _noteBars.Length; x++)
            {
                if (!Players[x].Playing)
                {
                    continue;
                }
                if ((!Visible) || (Players[x].KO))
                {
                    _noteBars[x].Opacity = Math.Max(0, _noteBars[x].Opacity - fadeAmount);
                }
                else
                {
                    _noteBars[x].Opacity = 255;
                }
                _noteBars[x].Redness = Math.Max(0, _noteBars[x].Redness - amount);
                _noteBars[x].XDisplayOffset *= mx;
                _noteBars[x].Draw(spriteBatch);
   
            }

            if (_gameType == GameType.SYNC_PRO)
            {
                DrawProgressSyncPro(spriteBatch);
            }
            else if (_gameType == GameType.SYNC_PLUS)
            {
                DrawProgressSyncPlus(spriteBatch);
            }
            else
            {
                DrawProgress(spriteBatch);
            }
        
        }

        private void DrawProgressSyncPlus(SpriteBatch spriteBatch)
        {
            _noteBarProgresses[0].Value = (from e in _noteBars where Players[e.ID].Playing select e.NumberCompleted()).Sum();
            _noteBarProgresses[0].Maximum = (int) (Players[0].Level*(from e in Players where e.Playing select e).Count());
            _noteBarProgresses[0].Draw(spriteBatch);
        }

        private void DrawProgressSyncPro(SpriteBatch spriteBatch)
        {
            _noteBarProgresses[0].Value = (from e in _noteBars where Players[e.ID].Playing select e.NumberCompleted()).Sum();
            _noteBarProgresses[0].Maximum = (from e in _noteBars where Players[e.ID].Playing select e.Notes.Count).Sum();
            _noteBarProgresses[0].Draw(spriteBatch);
        }

        private void DrawProgress(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _noteBars.Length; x++)
            {
                if (!Players[x].Playing)
                {
                    continue;
                }
                _noteBarProgresses[x].Value = _noteBars[x].NumberCompleted();
                _noteBarProgresses[x].Maximum = _noteBars[x].Notes.Count();
                _noteBarProgresses[x].Draw(spriteBatch);
            }
          
        }

        public void CancelReverse(int player)
        {
            _noteBars[player].CancelReverse();
            if (SyncGameType)
            {
                for (int x = 0; x < 4; x++)
                {
                    _noteBars[x].CancelReverse();
                }
            }
        }

        public void InitNoteBars()
        {
            var visibleCount = 0;
            for (int x = 0; x < 4; x++)
            {
                _syncNotebarPositions[x] = visibleCount;
                if (Players[x].Playing)
                {
                    visibleCount++;
                }
            }
         
            for (int x = 0; x < _noteBars.Length; x++)
            {
              
                CreateNextNoteBar(x);
                _noteBars[x].RednessSprite =  new Sprite
                                                  {
                    ColorShading = Color.Red,
                    SpriteTexture = TextureManager.CreateWhiteMask("BeatMeter"),
                    Height = 125,
                    Width = 350,
                    Position = SyncGameType ? _metrics["SyncBeatlineBarBase",_syncNotebarPositions[x]]: _metrics["BeatlineBarBase",x]
                };
                _noteBarProgresses[x] = new NoteBarProgress
                                            {
                                                Size = SyncGameType ? _metrics["SyncNoteBarProgress.Size", 0] : _metrics["NoteBarProgress.Size", 0], 
                                                ID = x,
                                                Position = SyncGameType ? _metrics["SyncNoteBarProgress", 0] : _metrics["NoteBarProgress", x]
                                            };
              
            }

            if (SyncGameType)
            {
                
                _noteBarProgresses[0].TextureSet = visibleCount;
                _noteBarProgresses[0].Height *= visibleCount;
            }
              
        }

        public void MaintainCPUArrows(double phraseNumber)
        {
            for (int x = 0; x < 4; x++)
            {
                if (!Players[x].IsCPUPlayer)
                {
                    continue;
                }

                var nextHit = 1.0 * (_noteBars[x].NumberCompleted() + 1) / (_noteBars[x].Notes.Count() + 1);
                var phraseDecimal = phraseNumber - Math.Floor(phraseNumber);

                if (phraseDecimal > nextHit)
                {
                    _noteBars[x].MarkCurrentCompleted();
                    Players[x].Hits++;
                }

            }
        }

        public void HitArrow(InputAction inputAction)
        {
            var player = inputAction.Player - 1;
            if ((Players[player].KO) || (!Players[player].Playing))
            {
                return;
            }

            if (_noteBars[player].CurrentNote() == null) 
            {
                return;
            }
            if ((Note.ActionToDirection(inputAction) == _noteBars[player].CurrentNote().Direction))
            {
                _noteBars[player].MarkCurrentCompleted();
                SyncRemainingNotes();
                if (PlayerArrowHit != null)
                {
                    PlayerArrowHit(player, null);
                }
            }
            else if ((_noteBars[player].CurrentNote() != null))
            {
                _noteBars[player].PlayerFaulted();
                SyncRemainingNotes();
                if (PlayerFaulted != null)
                {
                    PlayerFaulted(player, null);
                }              
            }
        }

        private void SyncRemainingNotes()
        {
            if (_gameType != GameType.SYNC_PLUS)
            {
                return;
            }
            for (int x = 0; x< 4; x++)
            {
                _noteBars[x].DisplayLimit = _noteBars[x].NumberCompleted() + NumberIncomplete(0);
            }
        }

        public bool AllCompleted(int player)
        {
            if (_gameType == GameType.SYNC_PLUS)
            {
                return NumberCompleted(0) >= SyncPlusLevel;
            }
            return _noteBars[player].AllCompleted();
        }

        public void CreateNextNoteBar(int player)
        {
            if (player == (int) AggregatorPlayerID.ALL)
            {
                player = 0;
            }
            //Create next note bar.
            var numArrow = _gameType == GameType.SYNC_PLUS ? SyncPlusLevel : (int) Players[player].Level;
            int numReverse = GetReverseNoteCount(player);

            _noteBars[player] = NoteBar.CreateNoteBar(numArrow, numReverse, SyncGameType ? _metrics["SyncNoteBar", _syncNotebarPositions[player]] : _metrics["NoteBar", player]);
            _noteBars[player].DisplayLimit = numArrow;
            _noteBars[player].ID = player;
            _noteBars[player].RednessSprite = new Sprite
                                                  {
                ColorShading = Color.Red,
                SpriteTexture = TextureManager.CreateWhiteMask("BeatMeter"),
                Height = 125,
                Width = 350,
                Position =  SyncGameType ? _metrics["SyncBeatlineBarBase", _syncNotebarPositions[player]] : _metrics["BeatlineBarBase", player]
            };
            SyncNoteBars(_noteBars[player]);
        }

        public int SyncPlusLevel
        {
            get { return (int)(Players[0].Level * (from e in Players where e.Playing select e).Count()); }
        }

        private int GetReverseNoteCount(int player)
        {
            var numReverse = 0;

            if (Players[player].IsBlazing)
            {
                if (Players[player].Life >= 200)
                {
                    numReverse = (int) Players[player].Level*2/3;
                }
                else if (Players[player].Life >= 150)
                {
                    numReverse = (int)Players[player].Level * 1/2;
                }
                else
                {
                    numReverse = (int) Players[player].Level/3;
                }
            }
            if (_gameType == GameType.SYNC_PLUS)
            {
                numReverse *= (from e in Players where e.Playing select e).Count();
            }
            return numReverse;
        }

        public void SyncNoteBars(NoteBar notebar)
        {
            if (SyncGameType)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (!Players[x].Playing)
                    {
                        continue;
                    }
                    
                    _noteBars[x] = notebar.Clone();
                    _noteBars[x].DisplayLimit = notebar.DisplayLimit;
                    _noteBars[x].ID = x;
                    _noteBars[x].Position = _metrics["SyncNoteBar", _syncNotebarPositions[x]];
                    _noteBars[x].RednessSprite.Position = _metrics["SyncBeatlineBarBase", _syncNotebarPositions[x]];
                }
            }
        }

        public void TruncateNotes(int player, int level)
        {
            if (_gameType == GameType.SYNC_PRO)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (Players[x].Playing)
                    {
                        _noteBars[x].TruncateNotes(level);
                    }
                }
            }
            else if (_gameType == GameType.SYNC_PLUS)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (Players[x].Playing)
                    {
                        _noteBars[x].TruncateNotes(SyncPlusLevel);
                    }
                }
                SyncRemainingNotes();
            }
            else
            {
                _noteBars[player].TruncateNotes(level);
            }
            
        }
        public int NumberCompleted(int player)
        {
            if (_gameType == GameType.SYNC_PRO)
            {
                return (from e in _noteBars select e.NumberCompleted()).Max();
            }
            if (_gameType == GameType.SYNC_PLUS)
            {
                return Math.Min((from e in _noteBars select e.NumberCompleted()).Sum(),SyncPlusLevel) ;
            }
            return _noteBars[player].NumberCompleted(); 
        }

        public int NumberReverse(int player)
        {
            if (_gameType == GameType.SYNC_PRO)
            {
                return (from e in _noteBars select e.NumberReverse()).Max();
            }
            if (_gameType == GameType.SYNC_PLUS)
            {
                return (from e in _noteBars select e.NumberReverse()).Sum();
            }
            return _noteBars[player].NumberReverse();  
        }

        public int NumberIncomplete(int player)
        {
            if (_gameType == GameType.SYNC_PRO)
            {
                return (from e in _noteBars select e.Notes.Count - e.NumberCompleted()).Max();
            }
            if (_gameType == GameType.SYNC_PLUS)
            {
                return (SyncPlusLevel - NumberCompleted(0));
            }
            return _noteBars[player].Notes.Count - _noteBars[player].NumberCompleted();
        }

        public void MarkAllCompleted(int player)
        {
            _noteBars[player].MarkAllCompleted();
        }
    }
}
