using System;
using System.Linq;
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
        private double _lastDraw;

        public event EventHandler PlayerFaulted;
        public event EventHandler PlayerArrowHit;

        public NoteBarSet(MetricsManager metrics, Player[] players, GameType gameType) : base(metrics, players, gameType)
        {
            _noteBars = new NoteBar[4];
            InitNoteBars();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch,0.0);
        }

        private const int REDNESS_ANIMATION_SPEED = 384;
        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            var amount = Math.Max(0, (phraseNumber - _lastDraw) * REDNESS_ANIMATION_SPEED);
            _lastDraw = phraseNumber;

            for (int x = 0; x < _noteBars.Length; x++)
            {
                if (!Players[x].Playing)
                {
                    continue;
                }
                _noteBars[x].Redness = (int) Math.Max(0, _noteBars[x].Redness - amount);
                _noteBars[x].Draw(spriteBatch);
            }
        }

        public void CancelReverse(int player)
        {
            _noteBars[player].CancelReverse();
        }

        public void InitNoteBars()
        {
            
            for (int x = 0; x < _noteBars.Length; x++)
            {
                CreateNextNoteBar(x);
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

            if ((_noteBars[player].CurrentNote() != null) && (Note.ActionToDirection(inputAction) == _noteBars[player].CurrentNote().Direction))
            {
                _noteBars[player].MarkCurrentCompleted();
                if (PlayerArrowHit != null)
                {
                    PlayerArrowHit(player, null);
                }
            }
            else if ((_noteBars[player].CurrentNote() != null))
            {
                _noteBars[player].PlayerFaulted();
                
                if (PlayerFaulted != null)
                {
                    PlayerFaulted(player, null);
                }              
            }
        }

        public bool AllCompleted(int player)
        {
            return _noteBars[player].AllCompleted();
        }

        public void CreateNextNoteBar(int player)
        {
            if (player == (int) AggregatorPlayerID.ALL)
            {
                player = 0;
            }
            //Create next note bar.
            var numArrow = (int)Players[player].Level;
            int numReverse = GetReverseNoteCount(player);

            _noteBars[player] = NoteBar.CreateNoteBar(numArrow, numReverse, _metrics["NoteBar", player]);
            _noteBars[player].ID = player;
            SyncNoteBars(_noteBars[player]);
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
            return numReverse;
        }

        public void SyncNoteBars(NoteBar notebar)
        {
            if (_gameType == GameType.SYNC)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (!Players[x].Playing)
                    {
                        continue;
                    }
                    _noteBars[x] = notebar.Clone();
                    _noteBars[x].Position = _metrics["NoteBar", x];
                }
            }
        }

        public void TruncateNotes(int player, int level)
        {
            if (_gameType == GameType.SYNC)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (Players[x].Playing)
                    {
                        _noteBars[x].TruncateNotes(level);
                    }
                }
            }
            else
            {
                _noteBars[player].TruncateNotes(level);
            }
            
        }
        public int NumberCompleted(int player)
        {
            if (_gameType == GameType.SYNC)
            {
                return (from e in _noteBars select e.NumberCompleted()).Max();
            }
            return _noteBars[player].NumberCompleted(); 
        }

        public int NumberReverse(int player)
        {
            if (_gameType == GameType.SYNC)
            {
                return (from e in _noteBars select e.NumberReverse()).Max();
            }
            return _noteBars[player].NumberReverse();  
        }

        public int NumberIncomplete(int player)
        {
            if (_gameType == GameType.SYNC)
            {
                return (from e in _noteBars select e.Notes.Count + e.NumberCompleted()).Max();
            }
            return _noteBars[player].Notes.Count - _noteBars[player].NumberCompleted();
        }

        public void MarkAllCompleted(int player)
        {
            _noteBars[player].MarkAllCompleted();
        }
    }
}
