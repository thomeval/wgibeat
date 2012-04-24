﻿using System;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Drawing.Sets
{
    public class GrooveMomentumBarSet : DrawableObjectSet
    {
        private double _displayedGrooveMomentum;
        private readonly GrooveMomentumBar _gmbar;
        public GrooveMomentumBarSet(MetricsManager metrics, Player[] players, GameType gameType) : base(metrics, players, gameType)
        {
            _gmbar = new GrooveMomentumBar {Position = metrics["GrooveMomentumBar", 0], Width = 275};
        }

        private const int GM_CHANGE_SPEED = 12;
        public void UpdateDisplayedGM()
        {

            var diff = Player.GrooveMomentum - _displayedGrooveMomentum;
            if (Math.Abs(diff) < 0.001)
            {
                _displayedGrooveMomentum = Player.GrooveMomentum;
            }
            else
            {
              
                var changeMx = Math.Min(1, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * GM_CHANGE_SPEED);
                _displayedGrooveMomentum += diff * (changeMx);
            }

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_gameType != GameType.COOPERATIVE)
            {
                return;
            }

            UpdateDisplayedGM();
            _gmbar.DisplayedGrooveMomentum = _displayedGrooveMomentum;
            if (Players[0].Playing || Players[1].Playing)
            {
                _gmbar.Position = _metrics["GrooveMomentumBar", 0];
                _gmbar.Draw(spriteBatch);
            }
            if (Players[2].Playing || Players[3].Playing)
            {
                _gmbar.Position = _metrics["GrooveMomentumBar", 1];
                _gmbar.Draw(spriteBatch);
            }
        
        }
    }
}