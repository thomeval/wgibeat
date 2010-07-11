using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;

namespace WGiBeat.Drawing.Sets
{
    public class NoteBarSet : DrawableObject 
    {
        private readonly MetricsManager _metrics;
        public readonly Player[] Players;
        private readonly GameType _gameType;
        private readonly NoteBar[] _noteBars;

        public NoteBarSet()
        {
            _noteBars = new NoteBar[4];
        }
        public NoteBarSet(MetricsManager metrics, Player[] players, GameType gameType)
            : this()
        {
            _metrics = metrics;
            Players = players;
            _gameType = gameType;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }
    }
}
