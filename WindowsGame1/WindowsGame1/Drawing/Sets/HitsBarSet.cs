using System;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class HitsBarSet : DrawableObjectSet
    {
        private readonly byte[] _opacity;
        private Color _textColor = Color.Black;
        private SpriteMap _baseSprite;
        private SpriteMap _baseOvermaskSprite;

        private readonly byte[] _overmaskOpacity;
        private readonly long[] _lastMilestone;
        private readonly Color[] _overmaskColors = {Color.Red,  Color.Blue, Color.Lime, Color.Yellow};

        public HitsBarSet(MetricsManager metrics, Player[] players, GameType type)
            : base(metrics, players, type)
        {
            _opacity = new byte[4];
            _overmaskOpacity = new byte[4];
            _lastMilestone = new long[4];
            SetupSprites();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_gameType == GameType.SYNC)
            {
                DrawHitsBar(spriteBatch, 0, DeterminePrefix() + "HitsBar");
                DrawHitsBar(spriteBatch,1, DeterminePrefix() + "HitsBar" );
                return;
            }
            for (int x = 0; x < 4; x++)
            {

                if (!Players[x].Playing)
                {
                    continue;
                }
                DrawHitsBar(spriteBatch, x, DeterminePrefix() + "HitsBar");
            }
        }

        private void DrawHitsBar(SpriteBatch spriteBatch, int player, string assetName)
        {
            if (Players[player].Hits < 25)
            {
                _opacity[player] = (byte)Math.Max(_opacity[player] - 8, 0);
                _lastMilestone[player] = 0;
            }
            else
            {
                _opacity[player] = (byte)Math.Min(_opacity[player] + 8, 255);
            }
            _overmaskOpacity[player] = (byte)Math.Max(0, _overmaskOpacity[player] - 4);
            _baseSprite.SpriteTexture = TextureManager.Textures(assetName);
            _baseSprite.ColorShading.A = _opacity[player];
            _textColor.A = _opacity[player];
            _baseSprite.Draw(spriteBatch, player, _metrics[assetName, player]);

            DrawOvermask(spriteBatch, player,assetName);

            TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", Players[player].Hits), "DefaultFont",
                   _metrics[DeterminePrefix() + "HitsText", player], _textColor, FontAlign.CENTER);
        }

        private void DrawOvermask(SpriteBatch spriteBatch, int player, string assetName)
        {
            if (PlayerAtNewMilestone(player))
            {
                _overmaskOpacity[player] = 160;
                _lastMilestone[player] = Players[player].Hits;
            }

            _baseOvermaskSprite.ColorShading = _overmaskColors[player];
            _baseOvermaskSprite.ColorShading.A = _overmaskOpacity[player];
            _baseOvermaskSprite.Draw(spriteBatch, player, _metrics[assetName, player]);
        }

        private bool PlayerAtNewMilestone(int x)
        {
            return ((Players[x].Hits > _lastMilestone[x]) && Players[x].Hits > 0) && ((Players[x].Hits == 50) || (Players[x].Hits%100 == 0));
        }

        private void SetupSprites()
        {
            _baseSprite = new SpriteMap
            {
                Columns = 1,
                Rows = 4,
                SpriteTexture = TextureManager.Textures( DeterminePrefix() + "HitsBar")
            };

            _baseOvermaskSprite = new SpriteMap
                                      {
                                          Columns = 1,
                                          Rows = 4,
                                          SpriteTexture = TextureManager.CreateWhiteMask( DeterminePrefix() + "HitsBar"),
                                          ColorShading = {A = 128}
                                      };
            if (_gameType == GameType.SYNC)
            {
                _baseSprite.Rows = 2;
                _baseOvermaskSprite.Rows = 2;
            }
        }


        private string DeterminePrefix()
        {
            switch (_gameType)
            {
                case GameType.COOPERATIVE:
                    return "Coop";
                    case GameType.SYNC:
                    return "Sync";
                default:
                    return "";
            }
        }

        public void IncrementHits(int amount, int player)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.COOPERATIVE:
                case GameType.TEAM:
                case GameType.VS_CPU:
                    Players[player].Hits += amount;
                    Players[player].TotalHits+= amount;
                    break;
                case GameType.SYNC:
                    Players[0].Hits += amount;
                    //Total Hits aren't shared, but normal hits are.
                    Players[player].TotalHits+= amount;
                    for (int x = 1; x < 4; x++ )
                    {
                        Players[x].Hits = Players[0].Hits;
                    }
                        break;
            }
        }

        public void ResetHits(int player)
        {
            switch (_gameType)
            {
                case GameType.NORMAL:
                case GameType.COOPERATIVE:
                case GameType.TEAM:
                case GameType.VS_CPU:
                    Players[player].Hits = 0;
                    break;
                case GameType.SYNC:
                    for (int x = 0; x < 4; x++)
                    {
                        Players[x].Hits = 0;
                    }
                    break;
            }
        }
    }
}