using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing.Sets
{
    public class LevelBarSet : DrawableObject 
    {

        private readonly MetricsManager _metrics;
        public readonly Player[] Players;
        private readonly GameType _gameType;
        private readonly LevelBar[] _levelBars;

        public LevelBarSet()
        {
            _levelBars = new LevelBar[4];
        }
        public LevelBarSet(MetricsManager metrics, Player[] players, GameType gameType)
            : this()
        {
            _metrics = metrics;
            Players = players;
            _gameType = gameType;
            CreateLevelBars();
        }

        private void CreateLevelBars()
        {
            for (int x = 0; x < _levelBars.Length; x++)
            {
                _levelBars[x] = new LevelBar
                                    {
                                        Parent = this,
                                        BarPosition = _metrics["LevelBar", x],
                                        TextPosition = _metrics["LevelText", x],
                                        Height = 25,
                                        Width = 216,
                                        PlayerID = x,
                                    };
                _levelBars[x].SetPosition(_metrics["LevelBarBase",x]);
                
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.COOPERATIVE:
                    for (int x = 0; x < _levelBars.Length; x++)
                    {
                        if (Players[x].Playing)
                        {
                            _levelBars[x].Draw(spriteBatch);
                        }
                    }
                    break;
            }

        }
    }
}
