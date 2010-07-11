using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;


namespace WGiBeat.Drawing.Sets
{
    public class BeatlineSet : DrawableObject
    {
        private readonly MetricsManager _metrics;
        public readonly Player[] Players;
        private readonly GameType _gameType;

        public BeatlineSet()
        {

        }
        public BeatlineSet(MetricsManager metrics, Player[] players, GameType gameType)
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
