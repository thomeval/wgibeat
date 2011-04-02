using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class NoteBarSet : DrawableObjectSet
    {
        private readonly NoteBar[] _noteBars;

        public event EventHandler PlayerFaulted;
        public event EventHandler PlayerArrowHit;

        public NoteBarSet(MetricsManager metrics, Player[] players, GameType gameType) : base(metrics, players, gameType)
        {
            _noteBars = new NoteBar[4];
            InitNoteBars();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _noteBars.Length; x++)
            {
                if (!Players[x].Playing)
                {
                    continue;
                }
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
                _noteBars[x] = NoteBar.CreateNoteBar((int) Players[x].Level, 0);
                _noteBars[x].Position = (_metrics["NoteBar", x]);
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
            else if (_noteBars[player].CurrentNote() != null)
            {
                _noteBars[player].ResetAll();

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
            //Create next note bar.
            var numArrow = (int)Players[player].Level;
            var numReverse = (Players[player].IsBlazing) ? (int)Players[player].Level / 2 : 0;
            _noteBars[player] = NoteBar.CreateNoteBar(numArrow, numReverse, _metrics["NoteBar", player]);
        }

        public int NumberCompleted(int player)
        {
            return _noteBars[player].NumberCompleted() + _noteBars[player].NumberReverse(); 
        }

        public int NumberIncomplete(int player)
        {
            return _noteBars[player].Notes.Count - _noteBars[player].NumberCompleted();
        }

        public void MarkAllCompleted(int player)
        {
            _noteBars[player].MarkAllCompleted();
        }
    }
}
