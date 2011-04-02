using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class LevelBarSet : DrawableObjectSet 
    {

        private readonly LevelBar[] _levelBars;

        public LevelBarSet(MetricsManager metrics, Player[] players, GameType gameType)
            : base(metrics,players,gameType)
        {
            _levelBars = new LevelBar[4];
            CreateLevelBars();
        }

        private void CreateLevelBars()
        {
            for (int x = 0; x < _levelBars.Length; x++)
            {
                _levelBars[x] = new LevelBar
                                    {
                                        Parent = this,
                                        Height = 25,
                                        Width = 216,
                                        PlayerID = x,
                                    };
                _levelBars[x].Position = (_metrics["LevelBarBase", x]);
                
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.COOPERATIVE:
                case GameType.TEAM:
                case GameType.VS_CPU:
                    for (int x = 0; x < _levelBars.Length; x++)
                    {
                        if (Players[x].Playing)
                        {
                            _levelBars[x].Draw(spriteBatch);
                        }
                    }
                    break;

                    case GameType.SYNC:
                    if (Players[0].Playing || Players[1].Playing)
                    {
                        _levelBars[0].Position = _metrics["SyncLevelBar", 0];
                        _levelBars[0].Draw(spriteBatch); 
                    }
                    if (Players[2].Playing || Players[3].Playing)
                    {
                        _levelBars[0].Position = _metrics["SyncLevelBar", 1];
                        _levelBars[0].Draw(spriteBatch);
                    }
                    break;
            }

        }
    }
}
