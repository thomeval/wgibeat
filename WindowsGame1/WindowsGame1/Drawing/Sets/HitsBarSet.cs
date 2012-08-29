using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class HitsBarSet : DrawableObjectSet
    {
        private readonly double[] _hitsOpacity;
        private readonly double[] _streakOpacity;
        private readonly double[] _overmaskOpacity;
        private readonly double[] _streakOvermaskOpacity;
        private Color _textColor = Color.Black;
        private Sprite _baseSprite;
        private Sprite _streakBaseSprite;
        private Sprite _baseOvermaskSprite;
        private Sprite _streakOvermaskSprite;


        private readonly long[] _lastMilestone;
        private readonly int[] _lastStreak;
        private readonly Color[] _overmaskColors = {Color.Red,  Color.Blue, Color.Lime, Color.Yellow};
   

        public HitsBarSet(MetricsManager metrics, Player[] players, GameType type)
            : base(metrics, players, type)
        {
            _hitsOpacity = new double[4];
            _overmaskOpacity = new double[4];
            _streakOvermaskOpacity = new double[4];
            _lastMilestone = new long[4];
            _lastStreak = new int[4];
            _streakOpacity = new double[4];
            
        }


        private void SetupSprites()
        {
            _baseSprite = new Sprite();
            _baseOvermaskSprite = new Sprite();
            _streakBaseSprite = new Sprite();
            _streakOvermaskSprite = new Sprite();
            _baseSprite = _metrics.Setup2DFromMetrics(DeterminePrefix() + "HitsBar",0);
            _streakBaseSprite = _metrics.Setup2DFromMetrics("StreakBar", 0);
            _baseOvermaskSprite = new Sprite();

            _baseOvermaskSprite = _metrics.Setup2DFromMetrics(DeterminePrefix() + "HitsBar", 0);
            _streakOvermaskSprite = _metrics.Setup2DFromMetrics("StreakBar", 0);
            _baseOvermaskSprite.SpriteTexture = TextureManager.CreateWhiteMask(DeterminePrefix() + "HitsBar");
            _streakOvermaskSprite.SpriteTexture = TextureManager.CreateWhiteMask("StreakBar");

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
 
            if (_baseSprite == null)
            {
                SetupSprites();
            }
            if (SyncGameType)
            {
                DrawStreakBar(spriteBatch,0);
                DrawHitsBar(spriteBatch,0);

                return;
            }
            for (int x = 0; x < 4; x++)
            {

                if (!Players[x].Playing)
                {
                    continue;
                }
                DrawStreakBar(spriteBatch, x);
                DrawHitsBar(spriteBatch, x);

            }
        }



        private const int HITSBAR_SHOW_SPEED = 600;
        private const int HITSBAR_HIDE_SPEED = 1000;
        private const int OVERMASK_HIDE_SPEED = 750;

        private void DrawHitsBar(SpriteBatch spriteBatch, int player)
        {
            if (Players[player].Hits < 25)
            {
                _hitsOpacity[player] = Math.Max(_hitsOpacity[player] - (TextureManager.LastDrawnPhraseDiff * HITSBAR_HIDE_SPEED), 0);
                _lastMilestone[player] = 0;
            }
            else
            {
                _hitsOpacity[player] = Math.Min(_hitsOpacity[player] + (TextureManager.LastDrawnPhraseDiff * HITSBAR_SHOW_SPEED), 255);
            }
            _overmaskOpacity[player] = Math.Max(0, _overmaskOpacity[player] - (TextureManager.LastDrawnPhraseDiff * OVERMASK_HIDE_SPEED));
            
            _baseSprite.ColorShading.A = (byte) _hitsOpacity[player];
            _baseSprite.Position = _baseOvermaskSprite.Position = _metrics[DeterminePrefix() + "HitsBar", player];
            _textColor.A = (byte) _hitsOpacity[player];
            _baseSprite.Draw(spriteBatch);

            DrawOvermask(spriteBatch, player);

            var textPosition = _baseSprite.Position.Clone();
            textPosition.X += 25;
            TextureManager.DrawString(spriteBatch, String.Format("{0:D3}", Players[player].Hits), "DefaultFont",
                   textPosition, _textColor, FontAlign.CENTER);
        }

        private const int STREAKBAR_SHOW_SPEED = 600;
        private const int STREAKBAR_HIDE_SPEED = 3000;
        private void DrawStreakBar(SpriteBatch spriteBatch, int player)
        {
            if (Players[player].Streak < 2)
            {
                _streakOpacity[player] = Math.Max(_streakOpacity[player] - (TextureManager.LastDrawnPhraseDiff * STREAKBAR_HIDE_SPEED), 0);
                _lastStreak[player] = Players[player].Streak;
            }
            else
            {
                _streakOpacity[player] = Math.Min(_streakOpacity[player] + (TextureManager.LastDrawnPhraseDiff * STREAKBAR_SHOW_SPEED), 255);
            }
            _streakOvermaskOpacity[player] = Math.Max(0, _streakOvermaskOpacity[player] - (TextureManager.LastDrawnPhraseDiff * OVERMASK_HIDE_SPEED));

            _streakBaseSprite.ColorShading.A = (byte)_streakOpacity[player];
            _streakBaseSprite.Position = _streakOvermaskSprite.Position = _metrics[DeterminePrefix() + "StreakBar", player];
            _textColor.A = (byte)_streakOpacity[player];
            _streakBaseSprite.Draw(spriteBatch);

            DrawStreakOvermask(spriteBatch,player);
            var textPosition = _streakBaseSprite.Position.Clone();
            textPosition.X += _streakBaseSprite.Width - 25;
            TextureManager.DrawString(spriteBatch, String.Format("x{0}", Players[player].Streak), "DefaultFont",
                   textPosition, _textColor, FontAlign.CENTER);
        }

        private void DrawOvermask(SpriteBatch spriteBatch, int player)
        {
            if (PlayerAtNewMilestone(player))
            {
                _overmaskOpacity[player] = 190;
                _lastMilestone[player] = Players[player].Hits;
            }

            _baseOvermaskSprite.ColorShading = _overmaskColors[player];
            _baseOvermaskSprite.ColorShading.A = (byte) _overmaskOpacity[player];
            _baseOvermaskSprite.Draw(spriteBatch);
        }

        private void DrawStreakOvermask(SpriteBatch spriteBatch, int player)
        {
            if ((Players[player].Streak >= 2) && (Players[player].Streak > _lastStreak[player]))
            {
                
                    _streakOvermaskOpacity[player] = 190;
                    _lastStreak[player] = Players[player].Streak;
                
            }

            _streakOvermaskSprite.ColorShading = _overmaskColors[player];
            _streakOvermaskSprite.ColorShading.A = (byte)_streakOvermaskOpacity[player];
            _streakOvermaskSprite.Draw(spriteBatch);
        }
        private bool PlayerAtNewMilestone(int x)
        {
            return ((Players[x].Hits > _lastMilestone[x]) && Players[x].Hits > 0) && ((Players[x].Hits == 50) || (Players[x].Hits%100 == 0));
        }


        private string DeterminePrefix()
        {
            switch (_gameType)
            {
                    case GameType.SYNC_PRO:
                    case GameType.SYNC_PLUS:
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
                case GameType.SYNC_PRO:
                    case GameType.SYNC_PLUS:
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
                case GameType.SYNC_PRO:
                    case GameType.SYNC_PLUS:
                    for (int x = 0; x < 4; x++)
                    {
                        Players[x].Hits = 0;
                    }
                    break;
            }
        }
    }
}