using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public abstract class DrawableObjectSet :DrawableObject
    {
        protected readonly MetricsManager _metrics;
        public readonly Player[] Players;
        protected readonly GameType _gameType;

        protected DrawableObjectSet(MetricsManager metrics, Player[] players, GameType gameType)
        {
            _metrics = metrics;
            Players = players;
            _gameType = gameType;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            
        }

    }
}
